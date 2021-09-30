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

1. Install Unity 20.3.19f1 from Unity Hub (v2.4.5)
1. Pull and check out the project.
1. Add the project to Unity Hub
1. Start the project
1. Bypass the error message (`An error has occurred while resolving packages...`) by pressing continue.
1. Once inside the project add the downloaded packages to the project using https://docs.unity3d.com/Manual/upm-ui-tarball.html
1. Press play and the project should run!

### Troubleshooting

#### Reference rewriter: Error: type `System.Web.HttpUtility` doesn't exist in target framework. It is referenced from RestSharp.dll at System.String RestSharp.Extensions.StringExtensions::HtmlAttributeEncode(System.String).

1. Select "..\Assets\MRTK.Tutorials.AzureSpatialAnchors\Plugins\RestSharp.dll"
Change the settings of the inspector panel
1.  Platform settings->SDK->UWP
    1. Check "Don't process"
    1. Uncheck “Select platforms for plugin->Any Platform”, and Check ALL platforms below
    1. P.S. I'm not sure which step took effect. For steps 2.1 to 2.3, I did it together. According to experience, I think step 2.1 is the key, but I still did 2.2 and 2.3 to ensure that it is correct.
