// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// A UnityEvent callback containing a ManipulationEventData payload.
    /// </summary>
    [Serializable]
    public class ManipulationEvent : UnityEvent<ManipulationEventData> { }
}
