# Autotile

Implementation of autotile algorithm for tilemaps with json configuration.

## Features

- **Autotiling pipeline**
- **Tile configuration** (includes bitmasks and connections)

## Tests

Wrote tests for most important parts of the library to ensure they work as expected. To run tests just `cd tests` from project root, build and run. Tests are run automatically from `Main`.

## Installation

Can be added to a project as a git submodule and then referenced using dotnet, example:

```bash
git submodule add <link to this repo>
git submodule update --init --recursive
cd your/project/path
dotnet add reference ../path/to/autotile/lib/autotile.csproj
```

Or can be compiled as a dll and then referenced in the project:

```bash
# Create release .dll
git clone <link to this repo>
cd autotile/lib
dotnet build -c Release

# Add dll to the project
cd /path/to/the/project
dotnet add package /path/to/compiled/gamecore.dll
```
