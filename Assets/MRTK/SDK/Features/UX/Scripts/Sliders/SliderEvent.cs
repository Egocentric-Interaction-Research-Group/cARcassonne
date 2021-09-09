// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// A UnityEvent callback containing a SliderEventData payload.
    /// </summary>
    [Serializable]
    public class SliderEvent : UnityEvent<SliderEventData> { }

}
