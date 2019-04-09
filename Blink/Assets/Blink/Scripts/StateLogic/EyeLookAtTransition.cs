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
    /// Transition to the next state after looking at the specified target for the specified time.
    /// </summary>
    public class EyeLookAtTransition : State
    {
        //----------- Private Members -----------

        [SerializeField] private EyeInput _eyeInput;
        [SerializeField] private LoadingBar _loadingBar;
        // The load bar's position is set at a bit of an offset from the target to avoid clipping.
        [SerializeField] private Vector3 _loadBarOffset;
        [SerializeField] private GameObject _target;
        // Amount of time that the target must be looked at before a transition is triggered.
        [SerializeField] private float _dwellDuration = 1f;
        // Amount of time after the gesture is lost, that the gesture can regain itself before the system throws it away.
        [SerializeField] private float _gracePeriod = 0.25f;
        [SerializeField] private StateMachine _tutorialVisualSM;

        private float _timer;
        private float _graceTimer;
        private bool _targetLost;
        private bool _updateDwellTimer;
        private Transform _headpose;
        bool _lookingAtTarget;

        //----------- MonoBehaviour Methods -----------

        private void Awake()
        {
            _headpose = Camera.main.transform;
        }

        private void OnEnable()
        {
            _loadingBar.gameObject.SetActive(true);
            _timer = 0f;
            _graceTimer = 0f;
            _updateDwellTimer = false;
            _targetLost = false;
        }

        private void OnDisable()
        {
            if (_loadingBar != null)
            {
                _loadingBar.gameObject.SetActive(false);
            }
            // Tell the tutorial visuals state machine to progress as well.
            if (_tutorialVisualSM != null)
            {
                _tutorialVisualSM.GoToNext();
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
            // Poll for eye gaze target.
            bool prevLookAtState = _lookingAtTarget;
            _lookingAtTarget = _eyeInput.gazeObject == _target;
            if (_lookingAtTarget != prevLookAtState)
            {
                // Eye gaze target has changed.
                if (_lookingAtTarget)
                {
                    _updateDwellTimer = true;
                    _targetLost = false;
                }
                else
                {
                    _targetLost = true;
                }
            }
        }

        private void UpdateTransitionState()
        {
            // Grace period to regain target once we lose it.
            if (_targetLost && _updateDwellTimer)
            {
                _graceTimer += Time.deltaTime;
                if (_graceTimer > _gracePeriod)
                {
                    _updateDwellTimer = false;
                }
            }
            else
            {
                _graceTimer = 0f;
            }
            // Check if we've hit the target long enough. 
            if (_updateDwellTimer)
            {
                _timer += Time.deltaTime;
                if (_timer > _dwellDuration)
                {
                    parentStateMachine.GoToNext();
                }
            }
            else
            {
                _timer = 0f;
            }
        }

        private void UpdateLoadBar()
        {
            if (!_loadingBar.gameObject.activeInHierarchy)
            {
                return;
            }
            // Update visual.
            float progress = Mathf.Clamp01(_timer / _dwellDuration);
            _loadingBar.UpdateProgress(progress);
            // Calculate new position.
            Vector3 offset = (_headpose.forward * _loadBarOffset.z) + (_headpose.up * _loadBarOffset.y) + (_headpose.right * _loadBarOffset.x);
            Vector3 loadBarPos = _target.transform.position + offset;
            _loadingBar.transform.position = loadBarPos;
        }
    }

}
