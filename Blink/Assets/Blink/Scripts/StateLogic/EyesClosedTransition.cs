// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using MagicLeap.Utilities;

namespace MagicKit
{
    /// <summary>
    /// Transition to the next state after detecting the eyes have closed.
    /// </summary>
    public class EyesClosedTransition : State
    {
        //----------- Private Members -----------

        [SerializeField] private EyeInput _eyeInput;
        [SerializeField] private StateMachine _tutorialVisualSM;

        //----------- MonoBehaviour Methods -----------

        private void OnEnable()
        {
            _eyeInput.OnClose += HandleEyesClosed;
        }

        private void OnDisable()
        {
            if(_eyeInput != null)
            {
                _eyeInput.OnClose -= HandleEyesClosed;
            }
            // Tell the tutorial visuals state machine to progress as well.
            if (_tutorialVisualSM != null)
            {
                _tutorialVisualSM.GoToNext();
            }
        }

        //----------- Event Handlers -----------

        private void HandleEyesClosed()
        {
            parentStateMachine.GoToNext();
        }
    }
}
