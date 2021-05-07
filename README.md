# BridgeSource2Plugin

Plugin for exporting assets from Quixel Bridge to Source 2.  
**Only tested with HL Alyx.**

[![youtube demo video](http://img.youtube.com/vi/mxbicmO3Kug/0.jpg)](https://www.youtube.com/watch?v=mxbicmO3Kug)  
[![update & asset sprayer demo](http://img.youtube.com/vi/_U26K0bTlmc/0.jpg)](https://www.youtube.com/watch?v=_U26K0bTlmc)

## Features

- Exporting geometry & textures
- Automatic VMAT and VMDL creation
- Automatic compiling of exported assets with `resourcecompiler.exe`
- Automatic VMDL LOD setup from all exported LODs
- Automatic .spray -prefab creation from assets with multiple variations
- Option to specify shaders to use in materials
- Option to change export scale of 3d assets

**Supported:**

- Asset types (as seen in Bridge):
  - 3D Assets (\* see note below)
  - 3D Plants \*
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

- Modular assets with multiple meshes in a single .fbx
- Multiple materials (e.g. plants that have a separate billboard material for lowest LOD)

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
