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
    /// A state whose purpose is to wait for a state transition gesture before transitioning to the next state.
    /// </summary>
    public class GestureTransition : State
    {
        //----------- Private Members -----------

        [SerializeField] BlinkGestures _gestureInput;
        [SerializeField] private LoadingBar _loadingBar;
        [SerializeField] private StateMachine _cloudEnablerSM;
        [SerializeField] private StateMachine _tutorialVisualSM;
        [SerializeField] private float _duration = 1.5f;
        // Amount of time after the gesture is lost, that the gesture can regain itself before the system throws it away.
        [SerializeField] private float _gracePeriod = 0.25f;

        private float _timer;
        private bool _gestureDetected;
        private bool _gestureLost;
        private float _graceTimer;

        //----------- MonoBehaviour Members -----------

        private void OnEnable()
        {
            _timer = 0f;
            _graceTimer = 0f;
            _gestureDetected = false;
            _gestureLost = false;
            _loadingBar.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (_loadingBar != null)
            {
                _loadingBar.gameObject.SetActive(false);
            }
            // Tell the tutorial visuals state machine to progress as well.
            if(_tutorialVisualSM != null)
            {
                _tutorialVisualSM.GoToNext();
            }
            if(_cloudEnablerSM != null)
            {
                // Enable the cloud
                _cloudEnablerSM.GoToNext();
            }
        }

        private void Update()
        {
            HandleInput();
            // Update our internal state based on the input.
            UpdateTransitionState();
            UpdateLoadBar();
        }

        //----------- Private Methods -----------

        private void HandleInput()
        {
            // Poll the gesture system.
            if (_gestureInput.TransitionStateGestureDetected())
            {
                UpdateGestureStatus(true);
            }
            else
            {
                UpdateGestureStatus(false);
            }
        }

        private void UpdateLoadBar()
        {
            if (!_loadingBar.gameObject.activeInHierarchy)
            {
                return;
            }
            // Update the visual.
            float progress = Mathf.Clamp01(_timer / _duration);
            _loadingBar.UpdateProgress(progress);
            // Calculate new position.
            Vector3 handPos = Vector3.zero;
            if (_gestureInput.TryGetHandPosition(ref handPos))
            {
                _loadingBar.transform.position = handPos;
            }
        }

        private void UpdateGestureStatus(bool status)
        {
            if (status)
            {
                _gestureDetected = true;
                _gestureLost = false;
                _loadingBar.gameObject.SetActive(true);
            }
            else
            {
                _gestureLost = true;
            }
        }

        private void UpdateTransitionState()
        {
            if (_gestureDetected)
            {
                _timer += Time.deltaTime;
                if (_timer > _duration)
                {
                    parentStateMachine.GoToNext();
                }
                if (_gestureLost)
                {
                    _graceTimer += Time.deltaTime;
                    if (_graceTimer > _gracePeriod)
                    {
                        _loadingBar.gameObject.SetActive(false);
                        _gestureDetected = false;
                    }
                }
                else
                {
                    _graceTimer = 0f;
                }
            }
            else
            {
                _timer = 0f;
                _graceTimer = 0f;
            }
        }
    }
}