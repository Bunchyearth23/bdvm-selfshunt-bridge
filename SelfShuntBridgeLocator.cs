using System;
using System.Reflection;
using SelfShunt.API;

namespace BDVM.SelfShuntBridge;

public static class SelfShuntBridgeLocator
{
    public static bool TryCreate(out SelfShuntGeneratorControl? control, out string resultCode)
    {
        control = null;
        var implementation = Type.GetType("SelfShunt.SelfShuntApi, SelfShunt", false);
        if (implementation == null)
        {
            resultCode = "selfshunt-not-loaded";
            return false;
        }

        var property = implementation.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        if (!(property?.GetValue(null, null) is ISelfShuntIntegrationApi api))
        {
            resultCode = "selfshunt-api-unavailable";
            return false;
        }

        if (api.ApiVersion != 1)
        {
            resultCode = "selfshunt-api-incompatible";
            return false;
        }

        control = new SelfShuntGeneratorControl(api);
        if (!control.IsAvailable)
        {
            control = null;
            resultCode = "selfshunt-control-not-authoritative";
            return false;
        }

        resultCode = "selfshunt-control-ready";
        return true;
    }
}
