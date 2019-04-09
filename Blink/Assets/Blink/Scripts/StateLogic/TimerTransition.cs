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
    /// A state whose purpose is to wait a specified amount of time before transitioning to the next state.
    /// </summary>
    public class TimerTransition : State
    {
        //----------- Private Members -----------

        [SerializeField] private float _transitionDelay = 5f;
        [SerializeField] private StateMachine _tutorialVisualSM;

        //----------- MonoBehaviour Methods -----------

        private void OnEnable()
        {
            Invoke("GoToNextState", _transitionDelay);
        }

        private void OnDisable()
        {
            // Tell the tutorial visuals state machine to progress as well.
            if (_tutorialVisualSM != null)
            {
                _tutorialVisualSM.GoToNext();
            }
        }

        //----------- Private Methods -----------

        private void GoToNextState()
        {
            parentStateMachine.GoToNext();
        }
    }
}
