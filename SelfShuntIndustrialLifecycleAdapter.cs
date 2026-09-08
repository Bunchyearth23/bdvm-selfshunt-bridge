using System;
using System.Collections.Generic;
using SelfShunt.API;

namespace BDVM.SelfShuntBridge;

public interface ISelfShuntIndustrialLifecycleSink
{
    bool TryObserveExternalJob(string operationId, string jobId, string stationId, string cargoId);
    bool TryObserveDelivery(string operationId, string jobId, decimal cumulativeQuantity);
    bool TryResumeProduction(string operationId, string jobId);
}

public sealed class SelfShuntIndustrialLifecycleAdapter : IDisposable
{
    private readonly ISelfShuntIntegrationApi api;
    private readonly ISelfShuntIndustrialLifecycleSink sink;
    private readonly Action<string> log;
    private readonly Dictionary<string, decimal> delivered = new Dictionary<string, decimal>(StringComparer.Ordinal);
    private readonly HashSet<string> resumed = new HashSet<string>(StringComparer.Ordinal);

    public SelfShuntIndustrialLifecycleAdapter(ISelfShuntIntegrationApi api, ISelfShuntIndustrialLifecycleSink sink, Action<string>? log = null)
    {
        this.api = api ?? throw new ArgumentNullException(nameof(api));
        this.sink = sink ?? throw new ArgumentNullException(nameof(sink));
        this.log = log ?? (_ => { });
        api.EventPublished += OnEvent;
    }

    public bool TryRegister(SelfShuntExternalJobRegistration registration)
    {
        if (!api.IsHost || !api.IsExternalEconomicAuthority)
        {
            log("selfshunt-external-job-refused:not-authoritative");
            return false;
        }
        var accepted = api.TryRegisterExternalJob(registration);
        log(accepted ? "selfshunt-external-job-accepted:" + registration.JobId : "selfshunt-external-job-refused:" + registration.JobId);
        return accepted;
    }

    private void OnEvent(SelfShuntIntegrationEvent value)
    {
        if (!api.IsHost || value == null || value.SchemaVersion != 1 || string.IsNullOrWhiteSpace(value.JobId) ||
            string.IsNullOrWhiteSpace(value.OperationId) || value.ObservedPayout != 0)
        {
            log("selfshunt-lifecycle-refused:invalid-authority-schema-or-payout");
            return;
        }

        if (value.Type == SelfShuntIntegrationEventType.ExternalJobCreated)
        {
            var accepted = sink.TryObserveExternalJob(value.OperationId, value.JobId, value.StationId, value.CargoId);
            log((accepted ? "selfshunt-external-job-observed:" : "selfshunt-external-job-rejected:") + value.JobId);
            return;
        }

        if (value.Type == SelfShuntIntegrationEventType.DeliveryObserved)
        {
            if (value.CumulativeQuantity < 0 || (delivered.TryGetValue(value.JobId, out var known) && value.CumulativeQuantity < known))
            {
                log("selfshunt-delivery-refused:non-monotonic:" + value.JobId);
                return;
            }
            if (delivered.TryGetValue(value.JobId, out known) && value.CumulativeQuantity == known) return;
            if (!sink.TryObserveDelivery(value.OperationId + ":delivery:" + value.CumulativeQuantity, value.JobId, value.CumulativeQuantity))
            {
                log("selfshunt-delivery-rejected:" + value.JobId);
                return;
            }
            delivered[value.JobId] = value.CumulativeQuantity;
            log("selfshunt-delivery-observed:" + value.JobId + ":" + value.CumulativeQuantity);
            return;
        }

        if (value.Type == SelfShuntIntegrationEventType.Completed && resumed.Add(value.JobId))
        {
            if (!sink.TryResumeProduction(value.OperationId + ":resume", value.JobId))
            {
                resumed.Remove(value.JobId);
                log("selfshunt-production-resume-rejected:" + value.JobId);
                return;
            }
            log("selfshunt-production-resumed:" + value.JobId);
        }
    }

    public void Dispose() => api.EventPublished -= OnEvent;
}
