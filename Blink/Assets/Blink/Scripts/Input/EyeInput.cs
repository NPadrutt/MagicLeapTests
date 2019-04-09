// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System;

namespace MagicKit
{
    /// <summary>
    /// Input wrapper for eye tracking.
    /// </summary>
    public class EyeInput : MonoBehaviour
    {
        //----------- Public Events -----------

        public event Action OnClose;
        public event Action OnOpen;
        public event Action OnBlink;
        public event Action<GameObject> OnGazeEnter;
        public event Action<GameObject> OnGazeExit;

        //----------- Public Members -----------

        public float sphereCastRadius = 0.01f;
        [HideInInspector] public bool eyesClosed;
        [HideInInspector] public Vector3 fixationPoint;
        [HideInInspector] public GameObject gazeObject;

        //----------- Private Members -----------

        [SerializeField]
        private float _rayCastDistance = 100;

        [SerializeField]
        // Amount of time the eyes must be closed before the system registers it as an eye closed event.
        private float _closedDuration = 1f;

        private Camera _camera;
        private float _closedTimer;
        private bool _insideBlink;

        //----------- MonoBehaviour Methods -----------

        private void Start()
        {
            _camera = Camera.main;
            MLResult result = MLEyes.Start();
            if (!result.IsOk)
            {
                Debug.LogError("Error starting MLEyes, disabling script.");
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (!MLEyes.IsStarted)
            {
                return;
            }
            fixationPoint = MLEyes.FixationPoint;
            DetectBlink();
            DetectEyesClosed();
        }

        private void FixedUpdate()
        {
            if (!MLEyes.IsStarted)
            {
                return;
            }
            DetectGazeTarget();
        }

        private void OnDestroy()
        {
            if (!MLEyes.IsStarted)
            {
                return;
            }
            MLEyes.Stop();
        }

        //----------- Private Methods -----------

        private void DetectBlink()
        {
            if (MLEyes.LeftEye.IsBlinking && MLEyes.RightEye.IsBlinking)
            {
                if (!_insideBlink)
                {
                    _insideBlink = true;
                    var handler = OnBlink;
                    if (handler != null && !eyesClosed)
                    {
                        handler();
                    }
                }
            }
            else
            {
                _insideBlink = false;
            }
        }

        private void DetectEyesClosed()
        {
            if (MLEyes.LeftEye.CenterConfidence == 0 && MLEyes.RightEye.CenterConfidence == 0)
            {
                if (!eyesClosed)
                {
                    _closedTimer += Time.deltaTime;
                    if (_closedTimer > _closedDuration)
                    {
                        // Eyes are recognized as being closed.
                        eyesClosed = true;
                        var handler = OnClose;
                        if (handler != null)
                        {
                            handler();
                        }
                    }
                }
            }
            else
            {
                if (eyesClosed)
                {
                    _closedTimer = 0f;
                    eyesClosed = false;
                    var handler = OnOpen;
                    if (handler != null)
                    {
                        handler();
                    }
                }
            }
        }

        private void DetectGazeTarget()
        {
            // Performs an eye raycast.
            Vector3 pos = _camera.transform.position;
            Vector3 dir;
            dir = MLEyes.FixationPoint - pos;
            dir = dir.normalized;
            RaycastHit hit;
            GameObject temp = null;
            if (Physics.SphereCast(pos, sphereCastRadius, dir, out hit, _rayCastDistance))
            {
                temp = hit.transform.gameObject;
            }
            if (temp != gazeObject)
            {
                var handler = OnGazeExit;
                if (handler != null && !eyesClosed)
                {
                    handler(gazeObject);
                }
                gazeObject = temp;
                if (temp != null)
                {
                    handler = OnGazeEnter;
                    if (handler != null && !eyesClosed)
                    {
                        handler(gazeObject);
                    }
                }
            }
        }
    }
}

