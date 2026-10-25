# BurnSystems.Make (bsmake) Build Tool

## Overview

`BurnSystems Make` (CLI name: `bsmake`) is a build tool designed to integrate project configuration and artifact generation into existing .NET solutions. It extends the capabilities of MSBuild by allowing developers to execute complex, non-standard build steps—such as running code generators, pre-compilation tasks, or specific build agents—directly within the standard build lifecycle.

Our primary goal is to enable developer teams to manage larger, multi-component projects where manual tools or dynamic processes are required before standard compilation can occur.

## Key Features & Use Cases

* ***Integrated Build Lifecycle:*** `bsmake` seamlessly integrates with MSBuild by running custom logic before building the solution or project.
* ***Configuration Awareness:*** Passes the active build configuration (`Debug`, `Release`, etc.) to build agents and forwards it to underlying `dotnet build` / `dotnet clean` executions.
* ***Incremental Builds:*** The tool minimizes unnecessary work by checking the modification times of source files and generated artifacts, only executing expensive steps (like building an internal agent) when necessary.
* ***Cross-Platform Support:*** Designed to operate effectively in both Windows and Linux environments.

Further information is available in [Design Specification](docs/design.adoc).

## Getting Started

### Installation (As a .NET Tool)

The most straightforward way to use `BurnSystems Make` is by installing it as a global `.NET CLI` tool:

    dotnet tool install -g BSMake

### Usage Commands

The core functionality revolves around simple, standardized verbs and options:

* ***Build:*** Performs the complete build cycle for the current scope (solution or project).
  * `bsmake build [-c|--configuration <configuration>]`
  * Example: `bsmake build -c Release`
* ***Clean:*** Cleans up all generated artifacts and intermediary files from the local directory.
  * `bsmake clean [-c|--configuration <configuration>]`
  * Example: `bsmake clean -c Release`

### Configuration Support

You can pass the `-c` or `--configuration` parameter to specify the target configuration (e.g., `Debug`, `Release`):
* In **Solution Context**, `bsmake` forwards `-c <configuration>` to the solution-level build agent and to `dotnet build` / `dotnet clean`.
* In **Project Context**, `bsmake` forwards `-c <configuration>` to the project's build agent executable.

### Context-Specific Execution Details

The behavior of `bsmake` depends on where it is called:

#### Solution Context (Calling in a Directory containing `.sln`/`.slnx`)
When executing `bsmake build` in a solution root, the tool performs a multi-step orchestration process:

1. Checks and builds any necessary "Build Agents" (e.g., external executables), passing the specified configuration (`-c <configuration>`) upon execution.
2. Calls `dotnet build` (with `-c <configuration>` if specified) on the entire solution file, allowing MSBuild to manage dependency order across all projects.

#### Project Context (Calling within a single project directory)
When executing `bsmake build` or `bsmake clean` inside a specific project folder:

1. Checks, builds, and executes any necessary "Build Agents" (passing the active verb like `build` or `clean`, and `-c <configuration>` if provided).

It is assumed that bsmake is called by MSBuild on project-level before targeting 'CoreCompile' (for build) and before 'Clean' (for clean). This can be achieved by adding the following targets in `.csproj` or `Directory.Build.targets` file (more information in [Design Specification](docs/design.adoc)):

```xml
<Project>
  <Target Name="RunBSMake" BeforeTargets="CoreCompile">
    <Exec Command="bsmake build $(MSBuildProjectDirectory) -c $(Configuration)" />
  </Target>
  <Target Name="RunBSMakeClean" BeforeTargets="Clean">
    <Exec Command="bsmake clean $(MSBuildProjectDirectory) -c $(Configuration)" />
  </Target>
</Project>
```

### Build Agent Helper Library (`BurnSystems.Make.BuildAgent`)

Build agents can reference the `BurnSystems.Make.BuildAgent` package/assembly which provides utility classes:
* `BsMake`: Helper class to easily dispatch async tasks based on the active verb (`make.Verb(Verbs.Build, ...)` / `make.Verb(Verbs.Clean, ...)`).
* `Verbs`: Standard verb constants (`Verbs.Build = "build"`, `Verbs.Clean = "clean"`).
* `ProgramArguments.Parse(args)`: Parses command-line arguments to easily retrieve the active verb via `arguments.Verb` and configuration via `arguments.Configuration`.
* `NpmHelper`: Helper methods for npm and npx operations (`InstallIfNotInstalled`, `NpmInstall`, `Execute`, `DeleteNodeModules`).
* `FileHelper.MergeFiles(options)`: Merges files with modification date checking (`OnlyIfModified`) and custom header prefixes, returning `true` if files were updated.
* `DirectoryHelper.CopyFiles(...)` / `DirectoryHelper.CopyFilesRecursively(...)`: Copies files matching search patterns non-recursively or recursively with subdirectories.
* `ProcessInvoke.Run(...)`: Runs external processes or commands with cross-platform PATH and extension resolution.
* `EnvironmentHelper`: Utilities for operating system and architecture detection.

Detailed method documentation and code examples are available in [Build Agent Documentation](docs/buildagent.adoc).
