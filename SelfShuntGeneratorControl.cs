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
        => TrySetStrictEconomyPolicy(operationId, true);

    public bool TrySetStrictEconomyPolicy(string operationId, bool suspended)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(operationId)) return false;

        if (!api.SetNaturalCarPopulationSuspended(operationId + ":cars", suspended)) return false;
        if (!api.SetNewGenerationSuspended(operationId + ":jobs", "", suspended))
        {
            api.SetNaturalCarPopulationSuspended(operationId + ":cars-rollback", !suspended);
            return false;
        }
        return api.IsNewGenerationSuspended("") == suspended && api.IsNaturalCarPopulationSuspended == suspended;
    }
}
