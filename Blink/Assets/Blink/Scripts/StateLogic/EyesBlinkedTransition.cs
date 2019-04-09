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
    /// Transition to the next state after detecting an eye blink.
    /// </summary>
    public class EyesBlinkedTransition : State
    {
        //----------- Private Members -----------

        [SerializeField] private EyeInput _eyeInput;
        [SerializeField] private int _numOfBlinks = 3;
        [SerializeField] private StateMachine _tutorialVisualSM;

        private int _blinkCount;

        //----------- MonoBehaviour Methods -----------

        private void OnEnable()
        {
            _blinkCount = 0;
            _eyeInput.OnBlink += HandleEyesBlinked;
        }

        private void OnDisable()
        {
            if(_eyeInput != null)
            {
                _eyeInput.OnBlink -= HandleEyesBlinked;
            }
            // Tell the tutorial visuals state machine to progress as well.
            if (_tutorialVisualSM != null)
            {
                _tutorialVisualSM.GoToNext();
            }
        }

        //----------- Event Handlers -----------

        private void HandleEyesBlinked()
        {
            _blinkCount++;
            if(_blinkCount >= _numOfBlinks)
            {
                parentStateMachine.GoToNext();
            }
        }
    }
}
