// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/MRTK/Examples/LogStructure")]
    public class LogStructure : MonoBehaviour
    {
        public virtual string[] GetHeaderColumns()
        {
            return Array.Empty<string>();
        }

        public virtual object[] GetData(string inputType, string inputStatus, EyeTrackingTarget intTarget)
        {
            return Array.Empty<object>();
        }
    }
}