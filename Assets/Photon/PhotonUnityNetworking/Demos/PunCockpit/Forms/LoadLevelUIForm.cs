// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetRoomCustomPropertyUIForm.cs" company="Exit Games GmbH">
//   Part of: Pun Cockpit Demo
// </copyright>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Photon.Pun.Demo.Cockpit.Forms
{
    /// <summary>
    /// Level Loading UI Form.
    /// </summary>
	public class LoadLevelUIForm : MonoBehaviour
    {
        public InputField PropertyValueInput;

        [Serializable]
        public class OnSubmitEvent : UnityEvent<string> { }

        public OnSubmitEvent OnSubmit;

        public void Start()
        {

        }

        // new UI will fire "EndEdit" event also when loosing focus. So check "enter" key and only then StartChat.
        public void EndEditOnEnter()
        {
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
            {
                this.SubmitForm();
            }
        }

        public void SubmitForm()
        {
            OnSubmit.Invoke(PropertyValueInput.text);
        }
    }
}