using System;
using BDVM.Common;
using SelfShunt.API;

namespace BDVM.SelfShuntBridge;

public sealed class SelfShuntGeneratorControl : IBdvmCompetingGeneratorControl
{
    private readonly ISelfShuntIntegrationApi api;

    public SelfShuntGeneratorControl(ISelfShuntIntegrationApi api)
    {
        this.api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public bool IsAvailable => api.IsHost && api.CanControlNewGeneration && api.CanControlNaturalCarPopulation;

    public bool IsStrictEconomyPolicyApplied =>
        api.IsNewGenerationSuspended("") && api.IsNaturalCarPopulationSuspended;

    public bool TryApplyStrictEconomyPolicy(string operationId)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(operationId)) return false;

        // Suspend free rolling-stock population first. If the second operation fails,
        // the world remains in the safer (scarcer) state and the caller fails closed.
        if (!api.SetNaturalCarPopulationSuspended(operationId + ":cars", true)) return false;
        if (!api.SetNewGenerationSuspended(operationId + ":jobs", "", true)) return false;
        return IsStrictEconomyPolicyApplied;
    }
}
