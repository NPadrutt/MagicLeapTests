// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections;
using UnityEngine;

namespace MagicKit
{
    /// <summary>
    /// Handles the visual and audio components for the cloud.
    /// </summary>
    public class Cloud : MonoBehaviour
    {
        [System.Serializable]
        private struct StormColor
        {
            public MatColorProp[] colors;
        }

        [System.Serializable]
        private struct MatColorProp
        {
            public Color color;
            public string propName;
        }

        public enum CloudAnimStates
        {
            Undefined,
            Spawn,
            Inactive,
            Active
        }

        //----------- Private Members -----------

        [SerializeField] private CloudAudioBehavior _audioBehavior;
        [SerializeField] private Animator _animator;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private StormColor[] _stormColors;
        [SerializeField] private string _activeParamName = "active";
        // Multiplier applied to distance to generate an adjustment to add to the minimum spherecast radius.
        [SerializeField] private float _distanceScaleFactor = 0.5f;
        // Object used to detect distance from for scaling.
        [SerializeField] private GameObject _originObject;
        [SerializeField] private float _delayRain = 0.5f;

        private bool _raining = false;
        private bool _orbiting = false;
        private int _stormColorIndex;
        private float _scaleAdjustment;
        private CloudAnimStates _animState = CloudAnimStates.Undefined;

        //----------- MonoBehaviour Methods -----------

        private void OnEnable()
        {
            CloudAnimStateMachine.OnStateEntered += OnAnimStateEntered;
        }

        private void OnDisable()
        {
            CloudAnimStateMachine.OnStateEntered -= OnAnimStateEntered;
        }

        private void LateUpdate()
        {
            //Applies scale in late update to allow for updating scale in animator.
            // Calculates new scale adjustment.
            float dist = Vector3.Distance(_originObject.transform.position, transform.position);
            _scaleAdjustment = dist * dist * _distanceScaleFactor;

            // Calculates new scale.
            Vector3 scale = transform.localScale + (Vector3.one * _scaleAdjustment);

            // Applies scale.
            transform.localScale = scale;
        }

        //----------- Public Methods -----------

        /// <summary>
        /// Update the audio/animator to match the current raining state.
        /// </summary>
        public void SetRaining(bool raining)
        {
            if (raining == _raining)
            {
                return;
            }

            if (raining)
            {
                // Do not start raining, until cloud animation is done initializing.
                if (_animState == CloudAnimStates.Undefined || _animState == CloudAnimStates.Spawn)
                {
                    return;
                }

                StopCoroutine("StopRain");
                StartCoroutine("Rain");
            }
            else
            {
                StopCoroutine("Rain");
                StartCoroutine("StopRain");
            }

            _raining = raining;
        }

        /// <summary>
        /// Update the audio to match the current orbiting state.
        /// </summary>
        public void SetOrbit(bool orbiting)
        {
            if (orbiting == _orbiting)
            {
                return;
            }

            _audioBehavior.PlayOrbit(orbiting);

            _orbiting = orbiting;
        }

        /// <summary>
        /// Tell audio handler to play thunder sfx and change the storm color of the cloud.
        /// </summary>
        public void DoThunder()
        {
            _audioBehavior.PlayThunder();
            SetStormColor();
        }

        //----------- Private Methods -----------

        private void SetStormColor()
        {
            if(_stormColors.Length == 0)
            {
                return;
            }
            StormColor newColors = _stormColors[_stormColorIndex];
            foreach(MatColorProp mp in newColors.colors)
            {
                if (_renderer.material.HasProperty(mp.propName))
                {
                    _renderer.material.SetColor(mp.propName, mp.color);
                }
            }
            _stormColorIndex++;
            if(_stormColorIndex >= _stormColors.Length)
            {
                _stormColorIndex = 0;
            }
        }

        /// <summary>
        /// Cloud entered a new animation state.
        /// </summary>
        /// <param name="animState">New animation state entered.</param>
        private void OnAnimStateEntered(CloudAnimStates animState)
        {
            _animState = animState;

            if (_animState == CloudAnimStates.Spawn)
            {
                _audioBehavior.Spawn();
            }
            else if (_animState == CloudAnimStates.Active)
            {
                _audioBehavior.PlayRain(true);
            }
            else if (_animState == CloudAnimStates.Inactive)
            {
                _audioBehavior.PlayRain(false);
            }
        }

        //----------- Coroutines -----------

        private IEnumerator Rain()
        {
            // Don't start raining right away.
            yield return new WaitForSeconds(_delayRain);

            // Trigger visuals.
            _animator.SetBool(_activeParamName, true);
        }

        private IEnumerator StopRain()
        {
            // Don't stop raining right away.
            yield return new WaitForSeconds(_delayRain);

            // Trigger visuals.
            _animator.SetBool(_activeParamName, false);
        }
    }
}
