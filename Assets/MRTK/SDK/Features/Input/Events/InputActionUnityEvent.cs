// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Unity event for input action events. Contains the data of the input event that triggered the action.
    /// </summary>
    [Serializable]
    public class InputActionUnityEvent : UnityEvent<BaseInputEventData> { }
}
