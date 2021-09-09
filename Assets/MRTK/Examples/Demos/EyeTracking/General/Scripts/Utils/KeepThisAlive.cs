// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// Enforces to keep this GameObject alive across different scenes.
    /// </summary>
    [Obsolete("This component is no longer supported", true)]
    [AddComponentMenu("Scripts/MRTK/Obsolete/KeepThisAlive")]
    public class KeepThisAlive : MonoBehaviour
    {
        public static KeepThisAlive Instance { get; private set; }

        private void Awake()
        {
            Debug.LogError(this.GetType().Name + " is deprecated");
        }

        private void Start()
        {
            if (Instance != null)
            {
                gameObject.SetActive(false);
                Instance.gameObject.SetActive(true);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(Instance);
            }
        }
    }
}