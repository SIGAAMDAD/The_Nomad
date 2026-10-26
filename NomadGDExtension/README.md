# Nomad game GDExtension

Open `NomadGDExtension.sln` in Visual Studio, or open this folder as a CMake
project. The maintained MSVC project delegates Build, Rebuild, and Clean to
CMake, so IDE and command-line builds use the same source list and dependencies.

From the repository root:

```powershell
.\NomadGDExtension\build.cmd
.\NomadGDExtension\build.cmd -Configuration Release
.\NomadGDExtension\build.cmd -ConfigureOnly
```

The default MSVC presets use Visual Studio 2026 (CMake 4.2+). For Visual Studio
2022 and CMake 3.24+, pass `-Toolchain msvc2022`, or select the `msvc2022-debug`
/ `msvc2022-release` CMake presets. When using the maintained `.sln` with VS2022,
set the MSBuild property `NomadToolchain=msvc2022` (or set that environment
variable before opening Visual Studio). Both versions need the C++ workload,
a Windows SDK, and Python 3.8+. Use `-Jobs 4` to limit parallel projects.

From this directory, the direct equivalents are:

```powershell
cmake --preset msvc-debug
cmake --build --preset msvc-debug --parallel 4
```

On Linux x86_64, install G++, GNU Make, binutils, CMake 3.24+, and Python 3.8+.
Run from this directory:

```sh
sh build.sh Debug
sh build.sh Release
# Or generate a GNU Makefile explicitly:
cmake -S . -B build/linux-debug -G "Unix Makefiles" -DCMAKE_CXX_COMPILER=g++ -DCMAKE_BUILD_TYPE=Debug -DGODOTCPP_TARGET=template_debug
cmake --build build/linux-debug --parallel 4
```

`linux-debug` and `linux-release` presets generate GNU Makefiles. SDK paths
can be overridden with `-DNOMAD_FMOD_RUNTIME_DIR=/path/to/linux/fmod/libs` and
`-DNOMAD_STEAM_RUNTIME_DIR=/path/to/linux/steam/libs` after the configuration
argument to `build.sh`, or when configuring CMake directly. Linux requires
x86_64 Linux SDK binaries, not Windows import libraries.

This project builds both Godot extensions and their shared framework library.
Outputs go to `addons/nomad/bin/windows/` or `addons/nomad/bin/linux/`; SDK
runtimes are copied alongside them. Debug and Release outputs coexist.
Generated Visual Studio solutions are under `build/vs2026/msvc-debug/` or
`build/vs2022/msvc2022-debug/`, with matching Release directories.

The existing bootstrap and complete inventory implementation are compiled.
Unfinished Story implementation files remain excluded.
