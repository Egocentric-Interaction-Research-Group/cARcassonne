# cARcassonne
Augmented Reality Carcassonne using the HoloLens2 and MRTK in Unity

## Installation

1. Install Unity 2019.4.9f1 from Unity Hub
1. Pull and check out the project.
1. Download Azure Spatial Anchors (version 2.8.1)
   1. Core: https://dev.azure.com/aipmr/MixedReality-Unity-Packages/_packaging?_a=package&feed=Unity-packages&package=com.microsoft.azure.spatial-anchors-sdk.core&version=2.8.1&protocolType=Npm
   1. Windows: https://dev.azure.com/aipmr/MixedReality-Unity-Packages/_packaging?_a=package&feed=Unity-packages&package=com.microsoft.azure.spatial-anchors-sdk.windows&protocolType=Npm&version=2.10.0-preview.1&view=versions
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
