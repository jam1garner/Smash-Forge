Smash Forge
===========
<a href="url"><img src="https://github.com/jam1garner/Smash-Forge/wiki/Images/Application Main.png" align="top" height="auto" width="auto" ></a>

[Bug Tracker](https://github.com/jam1garner/Smash-4-Bone-Animator/issues) | [Request a feature](https://github.com/jam1garner/Smash-4-Bone-Animator/issues) | [Forge Wiki](https://github.com/jam1garner/Smash-Forge/wiki) | [![Build status](https://ci.appveyor.com/api/projects/status/o73kaah41uewf1kx/branch/master?svg=true)](https://ci.appveyor.com/project/Sammi-Husky/smash-4-bone-animator/branch/master)

## Installation
Download the latest commit from the [releases page](https://github.com/jam1garner/Smash-Forge/releases). See the wiki for more information and basic usage. **Do not accidentally download the source code (contains a .sln).**

## Features
* **Smash 4**
    * Accurate renders of Namco models (.nud), Namco textures (.nut), and Smash 4 level data (.lvd)
    * Animations
        * Play, import, and export animation files (.omo), maya anim files (.anim), and NW4R
        * animation files (CHR0)
        * Open .pac archives containing multiple animations
    * Bones
        * Open, preview, and edit boneset files (.vbn)
        * Import bones from .mdl0 models
    * Movesets
        * Edit ACMD files, file -> open then select a .mtable
        * Edit and preview movesets in real time (ACMD)
    * Materials
        * Edit materials for Namco models, including exporting or importing materials
        * Edit and preview Smash 4 material animations in real time (.mta)
        * Includes material presets to help get started with editing materials  
    * Preview stage camera animation (path.bin and CMR0 types)
    * Edit parameter files (certain .bins)
    * Create, edit, and delete parts of LVDs
    * Melee DAT  
        * Import Melee DAT models
        * Convert Melee level data (DAT) into Smash 4 level data (LVD)
* **Bfres (Wii U and Switch)**  
    * Can open and preview BFRES models, skeleton animations, texture pattern animations, bone
    visual animations, and textures.
    * SARC support.
* **Shared Features**
    * Import/export textures from DDS and PNG
    * Model and texture rendering
    * Debug shading for normals, tangents, bitangents, vertex color, skin weights, and more.
    * DAE model importing.  
    * Export models as DAE
    * Exportable models, animations, and textures  

## Building (Windows)
* Open the solution file in Visual Studio 2015 or later and build.  
* Other platforms are not supported.

## Known Bugs/Issues
This program is still a work in progress. There may be bugs or incomplete features. Please
stick with us as we're working on it! For known bugs and issues, check out the [Bug Tracker](https://github.com/jam1garner/Smash-4-Bone-Animator/issues)

## Credits
This application uses Open Source components. You can find the source code of their open source
projects along with license information below. We acknowledge and are grateful to these developers
for their contributions to open source.

Project:
* [Dock Panel Suite](https://github.com/dockpanelsuite/dockpanelsuite)
* Copyright (c) 2007 Weifen Luo (email: weifenluo@yahoo.com)
* MIT License: https://github.com/dockpanelsuite/dockpanelsuite/blob/master/license.txt

Project:
* [OpenTK](https://github.com/opentk/opentk)
* Copyright (c) 2006 - 2014 Stefanos Apostolopoulos <stapostol@gmail.com>
* MIT/X11 License: https://github.com/opentk/opentk/blob/develop/License.txt

Project:
* [SALT](https://github.com/Sammi-Husky/Sm4sh-Tools)
* Copyright (c) 2014 - 2016 Sammi Husky <Sammi-Husky@live.com>
* MIT License: https://github.com/Sammi-Husky/Sm4sh-Tools/blob/master/SALT/License.txt

Project:
* [FITX](https://github.com/Sammi-Husky/Sm4sh-Tools)
* Copyright (c) 2014 - 2016 Sammi Husky <Sammi-Husky@live.com>
* FITD MIT License: https://github.com/Sammi-Husky/Sm4sh-Tools/blob/master/FITD/LICENSE.txt
* FITC MIT License: https://github.com/Sammi-Husky/Sm4sh-Tools/blob/master/FITC/LICENSE.txt

Project:
* [SFGraphics](https://github.com/ScanMountGoat/SFGraphics)
* Copyright (c) 2018 SMG
* MIT License: https://github.com/ScanMountGoat/SFGraphics/blob/master/LICENSE.txt

Project:
* [EveryFileExplorer](https://github.com/Gericom/EveryFileExplorer)
* Gericom

Project:
* [NintenTools.Bfres](https://github.com/Syroot/NintenTools.Bfres)
* Copyright (c) 2017 Syroot
* MIT License: https://github.com/Syroot/NintenTools.Bfres/blob/master/LICENSE
