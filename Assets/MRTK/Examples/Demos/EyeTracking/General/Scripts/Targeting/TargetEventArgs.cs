// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.Input;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// Class specifying targeting event arguments.
    /// </summary>
    public class TargetEventArgs : EventArgs
    {
        public EyeTrackingTarget HitTarget { get; private set; }

        public TargetEventArgs(EyeTrackingTarget hitTarget)
        {
            HitTarget = hitTarget;
        }
    }
}
