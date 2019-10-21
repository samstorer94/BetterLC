# BetterLC
A simple solution to solve LC "arguments too long" exceptions that can occur in large projects

## What is BetterLC?
BetterLC is a wrapper exectulable for Microsoft's LC.exe, which is called during compilation

## Why do I need BetterLC?
As well as personally going through hell with large projects, I have seen many forum posts talking about a common issue. 
The issue is that the input to LC.exe is compiled of _every_ reference. 
This is even more of an issue in .NET Core 3.0 projects, where the compiled input is recursive, meaning _every_ reference of _every_ referenced project is also added to the input.

Apart from the benefit of not hitting the "argument too long" exception, there is also a real potential for a speed improvement in the build process. 
I have not timed LC against BetterLC, but it stands to reason that a smaller input to the LC task would yeild faster compilation times. 

## Great! How do I get started?
Simple!

### Include BetterLC
I *strongly* recommend including BetterLC in your repository if you use CI/CD. 
If you do, BetterLC is preconfigured to run from "YOUR_REPO/Build Tools". 
Just copy the BetterLC.exe and BetterLC.targets file into your Build Tools folder. 
If you do want to use a different path, you will need to edit the targets file. Look for "BLCPath".

### Edit the .targets file
There are a few things that are configurable, but the first thing you will want to do is edit "BLCResolvePath1". 
Add your dependency folders here (comma delimited!), and BetterLC will automatically resolve the required assemblies!

### Move & Rename your licx files
For BetterLC to work, you need to move all of your licx files out of your project folders. 
It is preconfigured to look in "YOUR_REPO/Licenses" for licx files (editable, see "BLCLicxFile"). 
The licx files need to be named as "ASSEMBLY_NAME.licx", where assembly name is the _Assembly Name_ of your project, _not the project name_.

### Add References to BetterLC.targets 
The final step is to add references to BetterLC.targets in each .csproj file that used to have a licx file. 
Anywhere inside the Project node of the .csproj, add:
```xml
  <Import Project="..\Build Tools\BetterLC.targets"/>
```
You will need to adjust the relative path according to how deep your project is in your folder structure.

### That's it!
BetterLC is a framework-dependent "R2R" .NET Core 3.0 application, meaning you will need the .NET Core 3.0 SDK installed. 
If you follow the instructions above, you _should_ have no issues!


### Sidenotes
You can build a self-contained version if you want, just "publish" the BetterLC project and choose "Release (x86) SC". Be warned, the size is large if you do this, and I'd assume that it might be quite sluggish.
