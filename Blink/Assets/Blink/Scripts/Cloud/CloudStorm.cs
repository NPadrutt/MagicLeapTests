// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicKit
{
    /// <summary>
    /// Handles the clouds storm visual state changes based upon eye input
    /// </summary>
    [RequireComponent(typeof(Cloud))]
    public class CloudStorm : MonoBehaviour
    {
        //----------- Private Members -----------

        [SerializeField] private EyeInput _eyeInput;

        private Cloud _cloud;

        //----------- MonoBehaviour Methods -----------

        private void Awake()
        {
            _cloud = GetComponent<Cloud>();
        }

        private void OnEnable()
        {
            _eyeInput.OnGazeEnter += HandleOnGazeEnter;
            _eyeInput.OnGazeExit += HandleOnGazeExit;
            _eyeInput.OnBlink += HandleOnEyesBlink;
        }

        private void OnDisable()
        {
            _eyeInput.OnGazeEnter -= HandleOnGazeEnter;
            _eyeInput.OnGazeExit -= HandleOnGazeExit;
            _eyeInput.OnBlink -= HandleOnEyesBlink;
        }

        //----------- Event Handlers -----------

        private void HandleOnGazeEnter(GameObject target)
        {
            if (gameObject == target)
            {
                _cloud.SetRaining(true);
            }
        }

        private void HandleOnGazeExit(GameObject target)
        {
            if (gameObject == target)
            {
                _cloud.SetRaining(false);
            }
        }

        private void HandleOnEyesBlink()
        {
            _cloud.DoThunder();
        }
    }
}
