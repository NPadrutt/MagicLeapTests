// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using MagicLeap.Utilities;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicKit
{
    /// <summary>
    /// Provides Gestures for the application layer.
    /// </summary>
    public class BlinkGestures : Singleton<BlinkGestures>
    {
        //----------- Private Members -----------

        [SerializeField, MagicLeapBitMask(typeof(KeyPoseTypes))]
        private KeyPoseTypes _spawnPoses;
        [SerializeField, MagicLeapBitMask(typeof(KeyPoseTypes))]
        private KeyPoseTypes _summonPoses;

        private Vector3 _leftHandPos;
        private Vector3 _rightHandPos;
        private bool _leftHandDetected;
        private bool _rightHandDetected;

        //----------- MonoBehaviour Methods -----------


        private void Update()
        {
            UpdateHandPositionTracking();
        }

        //----------- Public Methods -----------

        /// <summary>
        /// Checks if any of the gestures for valid state transitions are detected
        /// </summary>
        public bool TransitionStateGestureDetected()
        {
            if (GestureManager.Matches(GestureManager.LeftHand.KeyPose, _spawnPoses))
            {
                return true;
            }
            if (GestureManager.Matches(GestureManager.RightHand.KeyPose, _spawnPoses))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if any of the gestures for valid hand position are detected
        /// </summary>
        public bool HandPositionGestureDetected()
        {
            return _leftHandDetected || _rightHandDetected;
        }

        /// <summary>
        /// Tries to get a current hand position 
        /// </summary>
        public bool TryGetHandPosition(ref Vector3 pos)
        {
            if (_leftHandDetected)
            {
                pos = _leftHandPos;
                return true;
            }
            if (_rightHandDetected)
            {
                pos = _rightHandPos;
                return true;
            }
            return false;
        }

        //----------- Private Methods -----------

        private void UpdateHandPositionTracking()
        {
            _leftHandDetected = GestureManager.Matches(GestureManager.LeftHand.KeyPose, _summonPoses);
            _rightHandDetected = GestureManager.Matches(GestureManager.RightHand.KeyPose, _summonPoses);

            if (_leftHandDetected)
            {
                _leftHandPos = GestureManager.LeftHand.Center;
            }
            if (_rightHandDetected)
            {
                _rightHandPos = GestureManager.RightHand.Center;
            }
        }
    }
}