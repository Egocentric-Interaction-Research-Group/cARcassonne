// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Unity event for a pointer event. Contains the pointer event data.
    /// </summary>
    [Serializable]
    public class PointerUnityEvent : UnityEvent<MixedRealityPointerEventData> { }
}
