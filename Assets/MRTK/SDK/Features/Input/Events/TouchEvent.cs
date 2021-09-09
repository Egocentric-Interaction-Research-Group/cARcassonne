// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// A UnityEvent callback containing a TouchEventData payload.
    /// </summary>
    [Serializable]
    public class TouchEvent : UnityEvent<HandTrackingInputEventData> { }
}
