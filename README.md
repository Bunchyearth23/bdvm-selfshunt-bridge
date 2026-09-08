# BDVM - SelfShunt Bridge

`BDVM.SelfShuntBridge` is the optional, fail-closed connection between BDVM's economy and the versioned `SelfShunt.API` contract supplied by the authorized BDVM SelfShunt fork.

## Status

| Property | Value |
| --- | --- |
| Module kind | Optional runtime bridge |
| Target framework | .NET Framework 4.8 (`net48`) |
| Required contracts | `BDVM.Common`, `SelfShunt.API` |
| Runtime dependency | Compatible `SelfShunt` fork |
| Standalone | No |

SelfShunt and its DLLs are not bundled here. If it is absent or incompatible, omit the bridge. BDVM can still run, but it cannot ask SelfShunt to suppress competing job generation or natural rolling-stock population. The current `BDVM.Full` bundle declares SelfShunt as a requirement because it includes this bridge.

## Responsibilities

- Locate a compatible SelfShunt integration service through `SelfShuntBridgeLocator`.
- Implement `IBdvmCompetingGeneratorControl` with `SelfShuntGeneratorControl`.
- Allow the authoritative host to request suppression of SelfShunt job generation.
- Allow the authoritative host to request suppression of natural rolling-stock population when the BDVM finite market owns supply.
- Report missing, incompatible or non-authoritative conditions rather than pretending the request succeeded.

## Boundaries

The bridge does not bundle or replace SelfShunt, spawn vehicles, operate trains, create contracts or mutate money. It is a narrow coordination layer. It must not call an unversioned internal SelfShunt implementation or continue optimistically when the API cannot confirm control.

## Build and dependencies

The project expects `BDVM.Common` as a sibling under `src/` and `SelfShunt.API.csproj` from the authorized fork at the integration workspace's expected path. With both available:

```powershell
dotnet build .\BDVM.SelfShuntBridge.csproj -c Release
```

A standalone checkout may pass an equivalent project layout or update the project reference locally. Final packaging will consume a versioned API artifact rather than copy upstream implementation code into this repository.

## Testing and installation

Validation checks host-only behavior and fail-closed handling for missing or incompatible services. This is not an independent Unity Mod Manager mod. Install matching builds of `BDVM.Full` and the BDVM SelfShunt fork; do not install only `BDVM.SelfShuntBridge.dll`.

## Upstream and provenance

- Original repository: [Chump-the-Lump/DV-SelfShunter](https://github.com/Chump-the-Lump/DV-SelfShunter).
- BDVM fork: [Bunchyearth23/DV-SelfShunter](https://github.com/Bunchyearth23/DV-SelfShunter), branch `dvcompany-integration`.
- Recorded source revision: `329c85cf51715404af3b4d455239d9fc54f5ac5b`.
- Original author credit: `Chump_the_Lump`.

The author permits forks with credit. Keep the original repository link, source revision and credit visible in derived distributions.

## Compatibility

The bridge requires the compatible, versioned `SelfShunt.API`. Missing capabilities or authority cause refusal. Future API revisions must be negotiated explicitly rather than inferred from implementation details.

## License

The BDVM bridge code is licensed under the Apache License, Version 2.0. See [LICENSE](LICENSE). SelfShunt-derived work remains subject to the permission and attribution recorded above.
