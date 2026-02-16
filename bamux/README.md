# bamux

A BAM web application host (formerly "bamweb") providing a console-driven web server with DAO-based data access.

## Overview

bamux is a console application that serves as a web application host built on the BAM framework. Historically known as "bamweb" (the root namespace is `Bam.Net.Application` and the nuspec file is `bamux.nuspec`), it uses `BamConsoleContext.StaticMain` as its entry point, integrating with the BAM console menu system for interactive operation.

The project includes a legacy `Program_bak.cs` that reveals the application's original architecture: a `BamServer` hosting multiple web applications with configurable host bindings, content roots, service proxy responders, and process mode management (Dev/Test/Prod). The backup code shows capabilities for starting, stopping, and restarting the server, logging HTTP responses and service proxy calls, and configuring app-specific host prefixes.

The current active code (`Program.cs`) has been simplified to just the `BamConsoleContext.StaticMain(args)` call. The project also contains generated DAO (Data Access Object) classes in `common/common_dao_tmp_snpd/` for entity types like `Left`, `Right`, `LeftRight`, `TestTable`, `TestFkTable`, and `DaoReferenceObject`, suggesting it is used for testing DAO generation and data access patterns.

## Key Classes

| Class | Description |
|---|---|
| `Program` | Current entry point using `BamConsoleContext.StaticMain` |
| `Program_bak` | Legacy entry point with BamServer lifecycle management (start/stop/restart) |
| `DaoReferenceObject` | Generated DAO for reference object data access |
| `DaoReferenceObjectWithForeignKey` | Generated DAO with foreign key relationships |
| `Left` / `Right` / `LeftRight` | Generated DAO entities for relationship testing |
| `TestTable` / `TestFkTable` | Generated DAO entities for table-level testing |

## Dependencies

**Project References:**
- `bam.base` -- Core BAM framework library
- `bam.console` -- Console menu infrastructure

**Target Framework:** net10.0
**Output Type:** Exe
**PackageId:** bamweb
**RootNamespace:** Bam.Net.Application

## Usage Examples

```bash
# Run interactively
dotnet run --project bamux

# The legacy server (Program_bak) supported these arguments:
# --apps "app1,app2"   Only serve specified apps
# --content "/path"    Content root directory
# --verbose            Log 200 and 404 responses
# --ProcessMode "Dev"  Override process mode
```

## Known Gaps / Not Yet Implemented

- The current `Program.cs` only calls `BamConsoleContext.StaticMain(args)` with no menus or commands registered, making it a no-op without registered menu containers.
- The legacy server code in `Program_bak.cs` is fully commented out / not compiled as the active entry point.
- The generated DAO classes in `common/common_dao_tmp_snpd/` appear to be temporary/test artifacts.
- No active web server, controllers, or request handling is present in the current code.
- Cross-platform build output paths reference `$(HOMEDRIVE)$(HOMEPATH)/.bam/build/` which may not exist.
