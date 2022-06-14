# cARcassonne
Augmented Reality Carcassonne using the HoloLens2 and MRTK in Unity.

This document details the project dependencies and installation proceedures.
To get started with development, you can also browse the [Manual](Documentation/manual/Overview.md) and [API Docs] and look through associated [Articles](Documentation/articles/intro.md). 

Code at https://github.com/Egocentric-Interaction-Research-Group/cARcassonne

### Dependencies

cARcassonne has recently moved to the Unity OpenXR Plugin system and Microsoft's Mixed Reality OpenXR plugin. *These should be installed automatically, but they are listed here for reference.*

* Mixed Reality OpenXR plugin: version 1.0.3
* MRTK: version 2.7.3
* ML Agents: version 2.0.1
* AR Foundation: version 4.2.2
* Universal Render Pipeline (URP): version 12.1.6

## Installation

Installation lists software versions that have been tested with the setup. Other versions may or may not work!

### Prerequisites

1. Windows 10 Education (21H1)
   * *Tested on this particular build. Should work on many others as well.*
   * > **OS X**: This also seems to work on OS X 11.6 Big Sur and 12.2 Monterey, tested on a Mac M1. Note that there are limitations here. At the time of writing (May 2022), Unity and Jetbrains Rider have Native M1 apps, but they are slow. It is also not possible to build for Hololens on the M1.
3. Unity Hub (v3.1.2)
4. Microsoft Visual Studio Community 2019 (16.11.3) OR JetBrains Rider (2022.1+)
5. Github Client
    * Terminal Client (any one will do)
    * Github Desktop (https://desktop.github.com/)
    * Gitkraken (https://www.gitkraken.com/)

### Installing

1. Install Unity 2021.3.1f1 from Unity Hub
    * Make sure `Universal Windows Platform Build Support` is checked.
    * > **OS X**: UWP isn't an option, so I've just installed Mac and Windows build support
3. Clone the project from Github.
    * Check out the `develop` branch and fork from there.individial work branches from there.
5. Add the project to Unity Hub (`Projects > Add`)
6. Make sure the project has the right Unity version and open it.
    * This could take a while as Unity finds and downloads packages, etc.
1. The MRTK Project Configurator should pop up. `Apply` the default settings. The click `Next` and then `Done`
2. Add the missing Scenes to the Project Hierarchy:
    1. Open `GameScene` from `Assets > Scenes`
11. Press play and the project should run!

### Troubleshooting

#### Reference rewriter: Error: type `System.Web.HttpUtility` doesn't exist in target framework. It is referenced from RestSharp.dll at System.String RestSharp.Extensions.StringExtensions::HtmlAttributeEncode(System.String).

1. Select "..\Assets\MRTK.Tutorials.AzureSpatialAnchors\Plugins\RestSharp.dll"
Change the settings of the inspector panel
1.  Platform settings->SDK->UWP
    1. Check "Don't process"
    1. Uncheck “Select platforms for plugin->Any Platform”, and Check ALL platforms below
    1. P.S. I'm not sure which step took effect. For steps 2.1 to 2.3, I did it together. According to experience, I think step 2.1 is the key, but I still did 2.2 and 2.3 to ensure that it is correct.

#### Unity crashes while loading, "Importing Assets: Compiling Assembly Definition Files scripts"

1. Delete Library and Temp folders
1. Try again
