# BDVM - SelfShunt Bridge

Optional fail-closed bridge between BDVM and the versioned `SelfShunt.API` contract.

## Dependency status

This module is **not standalone**. It requires `BDVM.Common` and the compatible [BDVM SelfShunt fork](https://github.com/Bunchyearth23/DV-SelfShunter), whose `dvcompany-integration` branch provides `SelfShunt.API`.

SelfShunt and its DLLs are not bundled here. If SelfShunt is absent or exposes an incompatible API, omit this bridge; BDVM remains usable without SelfShunt, but cannot control SelfShunt job generation or natural rolling-stock population.

## Upstream and provenance

Original repository: https://github.com/Chump-the-Lump/DV-SelfShunter  
Source revision: `329c85cf51715404af3b4d455239d9fc54f5ac5b`  
Credit: `Chump_the_Lump`

## License

The BDVM bridge code is licensed under the Apache License, Version 2.0. See `LICENSE`. SelfShunt remains credited above.
