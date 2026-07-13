# bamux

The Bam Toolkit's console UX host — a thin executable that boots the framework's menu-driven console (`bam.console`) as a standalone process.

## Overview

`bamux` is a minimal .NET console application. Its entire compiled surface is a single `Program.Main`, which delegates straight to `BamConsoleContext.StaticMain(args)` in `bam.console`:

```csharp
namespace Bam.Application
{
    class Program
    {
        static void Main(string[] args) => BamConsoleContext.StaticMain(args);
    }
}
```

All actual behavior — resolving `[ConsoleCommand]`-decorated menus, dispatching arguments — lives in `bam.console` and `bam.base`; `bamux` exists to give that console experience its own launchable/packagable entry point (referenced in the bamtk repository structure as the "UX server").

The `bamux` repository also carries its own nested copy of ~30 framework submodules (`submodules/`) plus a legacy `common` submodule (`BamCommon`), mirroring the pattern used by `bamdb`: a top-level checkout built by `bamtk.sln`, and a private standalone snapshot for building `bamux.sln` in isolation. The nested copy still points at the legacy `Bam.Core`/`Bam.Net.Shared` chain and is not part of the current `Bam.*` migration.

## Known Gaps / Not Yet Implemented

- **Stale project metadata:** `bamux.csproj` sets `<RootNamespace>Bam.Net.Application</RootNamespace>` (the legacy namespace) even though the actual code inside is already migrated to `Bam.Application`. The `PackageId` (`bamweb`) and `bamux.nuspec` (`id=bamweb`, `description=bamweb`) likewise still carry the project's old pre-rename identity. These should be updated to match the `bamux` naming used everywhere else.
- The repository contains a large tree of generated DAO artifacts under `bamux/common/common_dao_tmp_snpd/` that look like leftover code-generation output rather than checked-in source.

## Dependencies

**Project References:** `bam.base`, `bam.console`.

**Target Framework:** net10.0 (Exe), packaged via `bamux.nuspec`.
