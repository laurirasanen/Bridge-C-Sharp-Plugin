# BridgeSource2Plugin

Plugin for exporting assets from Quixel Bridge to Source 2.  
**Only tested with HL Alyx.**

## Features

- Exporting geometry & textures
- Automatic VMAT and VMDL creation (need manual compile after export)
- Automatic VMDL LOD setup from all exported LODs

**Supported:**

- Asset types (as seen in Bridge):
  - 3D Assets
  - 3D Plants (\* see notes below)
  - Surfaces
  - Decals
  - Atlases
- Texture maps:
  - Albedo
  - Normal
  - Roughness
  - Ambient Occlusion
  - Metalness
  - Opacity
  - Transmission

**Not supported:**

- Models with multiple variations (e.g. most plants)
- Multiple materials (e.g. plants that have a separate billboard material for lowest LOD)
- Different shaders, defaults to `vr_complex.vfx` or `vr_projected_decals.vfx` depending on asset type

## Usage

`BridgeSource2Plugin.exe --help`  
See [releases tab](https://github.com/laurirasanen/BridgeSource2Plugin/releases) for a precompiled binary.

## Bridge export settings

- Export Target:
  - `Export Target`: `Custom Socket Export`
  - `Socket Port`: Same as the plugin, default: `24981`
- Textures:
  - `Format`: `TGA` (JPG tends to freeze Material Editor)
- Models:
  - `Megascans`: `FBX`
  - `LODs`: However many you want, should get set up automatically in VMDL. See note above regarding billboard material support.

## Demo

[![youtube demo video](http://img.youtube.com/vi/mxbicmO3Kug/0.jpg)](https://www.youtube.com/watch?v=mxbicmO3Kug)
