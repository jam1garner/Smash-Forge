Smash 4 Skeletal Animation Tool
===========
[Bug Tracker](https://github.com/jam1garner/Smash-4-Bone-Animator/issues) | [Request a feature](https://github.com/jam1garner/Smash-4-Bone-Animator/issues) | [![Build status](https://ci.appveyor.com/api/projects/status/o73kaah41uewf1kx/branch/master?svg=true)](https://ci.appveyor.com/project/Sammi-Husky/smash-4-bone-animator/branch/master)
##Features##
 - Can open and preview Smash 4 boneset files (.vbn)
 - Can play/import/export Smash 4 animation files (.omo), maya anim files (.anim), and NW4R Animation files (CHR0)
 - Can open .pac archives containing multiple animations
 
##Building##
###Windows###
 - Open the solution file in Visual Studio and build


##Known Bugs/Issues##
- Viewport zooms out when window in minimized
This is due to the viewport still technically being "active" while the window is minimised, so then if you use your mouse wheel when your mouse is hovering over the area where the viewport would be, it will interact with it. We are working on a fix.

- Custom MDL0 to VBN bonesets crash the game when loading
I (Y2K) do not currently know what causes this issue, or if it even has been fixed, but I do know from experience it is an issue. We are looking for a fix.
