# bamux

An ASP.NET Core UX host for the Bam Toolkit — serves the registration UI and its own direct identity API routes, talking to `bamid` over TCP.

## Overview

`bamux` was a dormant console shell (`BamConsoleContext.StaticMain`) until it was stood up as a real web host. It now builds a minimal `WebApplication` (`Microsoft.NET.Sdk.Web`) that serves `IndexPage`/`RegisterPage`/`RegisterResultPage` (moved here from `bamsvc`) and exposes its own `/api/register` and `/api/profile/{handle}` — the routes the registration UI's same-origin `fetch()` calls.

Those routes are handled by `IdentityUxRoutes`, which delegates to `bamid`'s generated `RegistrationServiceClient` **directly over TCP** (port 24515 by default, derived from bamid's server name), bypassing `bamsvc` entirely — an explicit routing decision, not an oversight. `bamsvc` has an identically-shaped `IdentityGatewayRoutes` class serving the same two routes independently, for callers that go through it instead.

`Program_bak.cs` (excluded from compilation) is the pre-rewrite legacy entry point — 199 lines written entirely against the old `Bam.Net.*` namespaces (`Bam.Net.CommandLine`, `Bam.Net.Incubation`, `Bam.Net.ServiceProxy`, `Bam.Net.Server`), showing what `bamux` used to be: a `DeployableCommandLineTool`-based `BamServer` host with start/stop/restart lifecycle management. Kept only as historical reference.

The repository also carries its own private nested framework snapshot under `common/` (~30 submodules plus legacy `BamCommon`, same duplicate-checkout pattern as `bamdb`) — explicitly excluded from the build now that the SDK's default file globbing would otherwise sweep it in.

## Key Classes

| Class | Description |
|---|---|
| `Program` (top-level statements) | Builds the `bamid`-backed `RegistrationServiceClient`, maps pages and `IdentityUxRoutes`, runs the web host (default port 8082). |
| `IdentityUxRoutes` | Maps `/api/register` and `/api/profile/{handle}` onto `IRegistrationService` calls against `bamid`, direct from the browser — not a proxy through `bamsvc`. |
| `IndexPage` / `RegisterPage` / `RegisterResultPage` | HTML pages, moved here from `bamsvc`. |
| `Program_bak` | Legacy, uncompiled: the original `BamServer`-hosting entry point. |

## Dependencies

**Project References:** `bam.base`, `bam.presentation`, `bamid.client` (private repo — the generated client for `bamid`, the standalone identity/user-management host).

**Target Framework:** net10.0 (`Microsoft.NET.Sdk.Web`), packaged via `bamux.nuspec`.

## Running Tests

```bash
dotnet run --project bamux.tests/bamux.tests.csproj -- --ut
```

## Known Gaps / Not Yet Implemented

- **Stale package identity:** `bamux.csproj` still sets `PackageId` to `bamweb` (its pre-rename identity), and `bamux.nuspec` likewise still has `id=bamweb`/`description=bamweb`. (The `RootNamespace` override that used to say `Bam.Net.Application` has since been removed.)
- The repository contains a large tree of generated DAO artifacts under `bamux/common/common_dao_tmp_snpd/` that look like leftover code-generation output rather than checked-in source.
- `bamux/README.md` (the nested *project-folder* README, not this repo-root one) contains an older, unrelated writeup — it never renders anywhere (GitHub only shows the repo-root README) and is now further out of date. Left in place rather than removed, since deleting a pre-existing file wasn't part of this documentation pass.
