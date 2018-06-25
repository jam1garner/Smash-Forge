Smash Forge (BFRES WIP Implementation)
===========

## 
## BFRES Features
 - Can open and preview BFRES models, skeleton animations, and textures
 - Wii U and Switch support. 
 - Exportable models, animations, and textures
 - Improved shaders from previous tool
 - Debug shader options (in view - render settings) to help debug mods
 
 
 ## Todo
 - Support BC7 textures.
 - SARC support. 
 - Fix up model exporting (bones have issues, and weights, and materials).
 - Add animation importing.
 - Add model importing.
 - Add every other type of animation (texture pattern, visual anim, srt anim, ect)
 - Add physically based rendering
 - Full decoding and compression of all texture types for editing.
 - Merge to master forge branch (code is too messy and not complete atm)

# Current libs used by this branch
Gericom's [EveryFileExplorer (Yaz0 decomp/comp)](https://github.com/Syroot/NintenTools.Bfres)
masterf0x's [RedCarpet (Sarc loading)](https://github.com/Gericom/EveryFileExplorer)
Syroot's [bfres library (Wii U)](https://github.com/Syroot/NintenTools.Bfres)
Syroot's bfres library (Wii U)  as a base for switch. 

[Bug Tracker](https://github.com/jam1garner/Smash-4-Bone-Animator/issues) | [Request a feature](https://github.com/jam1garner/Smash-4-Bone-Animator/issues) | [![Build status](https://ci.appveyor.com/api/projects/status/o73kaah41uewf1kx/branch/master?svg=true)](https://ci.appveyor.com/project/Sammi-Husky/smash-4-bone-animator/branch/master)
## Features
 - Can open, preview and edit Smash 4 boneset files (.vbn)
 - Can play/import/export Smash 4 animation files (.omo), maya anim files (.anim), and NW4R Animation files (CHR0)
 - Can open .pac archives containing multiple animations
 - Can import bones from .mdl0 models
 - Can preview Namco models (.nud), Namco textures (.nut) and Smash 4 Level Data (.lvd)
 - Can view and preview stage camera animation (path.bin and CMR0 types)
 - Can edit ACMD files, just use file -> open then select a .mtable 
 - Can edit Parameter files (Certain .bins)
 - Can import models from DAE and DAT
 - Can import textures from DDS
 - Can convert DAT level data into Smash 4 level data (LVD)
 - Can export DAE and NUD models
 - Can create, edit and delete parts of LVDs
 - Can edit materials for Namco models including exporting or importing materials from any model available.
 - Can edit and preivew Smash 4 movesets in real time (ACMD)
 - Can edit and preview Smash 4 material animations in real time (.mta)
 - Can open and preview BFRES (Wii U) Models (Only available on Wii U General Branch)
 - Includes a set of standard materials to help get started with more quickly adding and editing materials
 
NOTE: this entire program is still very much a WIP. There may be bugs or incomplete features. Please stick with us as we're working on it!
 
 While credit on projects made using this tool is in no way necessary it is very much appreciated, thank you to those who use it.
 
## Building
### Windows
 - Open the solution file in Visual Studio and build


## Known Bugs/Issues
For known bugs and issues, check out the [Bug Tracker](https://github.com/jam1garner/Smash-4-Bone-Animator/issues)

## Credits
This application uses Open Source components. You can find the source code of their open source projects along with license information below. We acknowledge and are grateful to these developers for their contributions to open source.

Project: 

- [Dock Panel Suite](https://github.com/dockpanelsuite/dockpanelsuite)
- Copyright (c) 2007 Weifen Luo (email: weifenluo@yahoo.com)
- MIT License: https://github.com/dockpanelsuite/dockpanelsuite/blob/master/license.txt

Project:
- [OpenTK](https://github.com/opentk/opentk)
- Copyright (c) 2006 - 2014 Stefanos Apostolopoulos <stapostol@gmail.com>
- MIT/X11 License: https://github.com/opentk/opentk/blob/develop/License.txt

Project:
- [SALT](https://github.com/Sammi-Husky/Sm4sh-Tools)
- Copyright (c) 2014 - 2016 Sammi Husky <Sammi-Husky@live.com>
- MIT License: https://github.com/Sammi-Husky/Sm4sh-Tools/blob/master/SALT/License.txt

Project:
- [FITX](https://github.com/Sammi-Husky/Sm4sh-Tools)
- Copyright (c) 2014 - 2016 Sammi Husky <Sammi-Husky@live.com>
- FITD MIT License: https://github.com/Sammi-Husky/Sm4sh-Tools/blob/master/FITD/LICENSE.txt
- FITC MIT License: https://github.com/Sammi-Husky/Sm4sh-Tools/blob/master/FITC/LICENSE.txt
