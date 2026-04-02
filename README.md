# AstroManager.NinaShared

Public shared packages for the AstroManager N.I.N.A. plugin.

## Contents

- `AstroManager.Contracts`
- `AstroManager.PluginShared`

## Purpose

This repository is the public shared-code surface used by the AstroManager N.I.N.A. plugin.
It is intended to be published as NuGet packages via GitHub Packages.

## Publish Flow

1. Update shared code.
2. Bump package versions.
3. Build locally.
4. Pack both projects.
5. Publish packages to GitHub Packages.
6. Update `AstroManager.NinaPlugin` to the new package versions.

## Notes

- License: MIT
- Package IDs:
  - `AstroManager.Contracts`
  - `AstroManager.PluginShared`
