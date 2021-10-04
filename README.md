# cARcassonne
Augmented Reality Carcassonne using the HoloLens2 and MRTK in Unity

## Dependencies

cARcassonne has recently moved to the Unity OpenXR Plugin system and Microsoft's Mixed Reality OpenXR plugin.

* Unity OpenXR plugin: version 1.2 or later
* Mixed Reality OpenXR plugin: version 1.0.0 or later
* MRTK: version 2.7.2 or later
* AR Foundation: version 4.1.1 or later
* Universal Render Pipeline (URP): version 10.5.1 or later
* Azure Spatial Anchors: version 2.10 or later
* Azure Remote Rendering: version 1.0.15 or later

## Installation

Installation lists software versions that have been tested with the setup. Other versions may or may not work!

### Prerequisites

1. Windows 10 Education (21H1)
1. Unity Hub (v2.4.5)
1. Microsoft Visual Studio Community 2019 (16.11.3) OR JetBrains Rider (2021.1+)
2. Github Client
    * Terminal Client (any one will do)
    * Github Desktop (https://desktop.github.com/)
    * Gitkraken (https://www.gitkraken.com/)

### Installing

1. Install Unity 20.3.19f1 from Unity Hub (v2.4.5)
    * Make sure `Universal Windows Platform Build Support` is checked.
3. Clone the project from Github.
    * Check out the `develop` branch and fork from there.
    * Actually, I'd suggest each team make a main feature branch (e.g. `feature/gameplayai` and `feature/situationrecognition` that you all share and fork individial work branches from there.
5. Add the project to Unity Hub (`Projects > Add`)
6. Make sure the project has the right Unity version and open it.
    * This could take a while as Unity finds and downloads packages, etc.
1. The MRTK Project Configurator should pop up. `Apply` the default settings. The click `Next` and then `Done`
2. Add the missing Scenes to the Project Hierarchy:
    1. Select `GameScene` and `StartmenuScene` from `Assets > Scenes`
    2. Drag them to the Hierarchy window
    3. `Remove` the default Untitled Scene
    1. `Unload` the `StartmenuScene` so that it is in the Hierarchy, but greyed out
11. Press play and the project should run!

### Building the documentation (windows only)
See [Documentation index](Documentation/index.md)

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
