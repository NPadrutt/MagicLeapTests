// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace MagicKit
{
    /// <summary>
    /// State machine for cloud movement. Each state has an enter, update, fixedupdate, oncollisionenter, and exit method. 
    /// Enter and exit are called during a state change.
    /// Update, fixedupdate, and oncollisionenter are called via their corresponding MonoBehaviour methods for the current state.
    /// </summary>
    [RequireComponent(typeof(Cloud))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class CloudMovement : MonoBehaviour
    {
        //----------- Private Members -----------

        [SerializeField] private BlinkTuning _blinkTuning;
        [SerializeField] private EyeInput _eyeInput;
        [SerializeField] private BlinkGestures _gestureInput;
        // The layers that are raycasted against when deciding valid points for the cloud to wander to.
        [SerializeField] private LayerMask _environmentLayer;
        // Used in the calculation to determine whether the cloud is currently considered to be in view of the user.
        [SerializeField][Range(0f, 1f)] private float _viewVecDotThresh = 0.85f;

        private Transform _headpose;
        private Rigidbody _rigidbody;
        private SphereCollider _collider;
        private Cloud _cloud;
        private State _state;
        private float _handToWanderTimer;
        private float _cloudOrbitSpeed;
        private bool _moveToPos;
        // The amount of time after reaching a wander target the cloud will wait before choosing another target.
        private float _wanderWait;
        // The current wander point that the cloud is traveling to.
        private Vector3 _wanderPoint;
        // We internally calculate the velocity for use when the cloud becomes kinematic and we want to apply force when we make it not kinematic.
        private Vector3 _velocity;
        // The position of the cloud on the previous frame (used to calculate velocity when the cloud becomes kinematic).
        private Vector3 _prevPosition;
        // The maximum speed the cloud can move at (velocity will be clamped to this value).
        private float _maxSpeed;
        // The distance away from the current wander point that the cloud needs to be in order to choose a new wander point.
        private const float StopDist = 0.05f;

        private enum State
        {
            Wander,
            HandHover,
            HeadOrbit,
        }

        //----------- MonoBehaviour Methods -----------

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<SphereCollider>();
            _cloud = GetComponent<Cloud>();
            _headpose = Camera.main.transform;
        }

        private void Start()
        {
            Vector3 handPos = transform.position;
            if (_gestureInput.TryGetHandPosition(ref handPos))
            {
                transform.position = handPos;
            }
            // Begin in the wander environment state.
            GoToState(State.Wander);
        }

        private void Update()
        {
            UpdateStateMachine();
            _velocity = (transform.position - _prevPosition) / Time.deltaTime;
            _prevPosition = transform.position;
        }

        private void FixedUpdate()
        {
            FixedUpdateStateMachine();
            // Clamp speed.
            _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, _maxSpeed);
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnCollisionEnterStateMachine(collision);
        }

        //----------- State Machine Methods -----------

        private void GoToState(State newState)
        {
            switch (_state)
            {
                case State.Wander:
                    ExitWander();
                    break;
                case State.HandHover:
                    ExitHandHover();
                    break;
                case State.HeadOrbit:
                    ExitHeadOrbit();
                    break;
            }
            _state = newState;
            switch (_state)
            {
                case State.Wander:
                    EnterWander();
                    break;
                case State.HandHover:
                    EnterHandHover();
                    break;
                case State.HeadOrbit:
                    EnterHeadOrbit();
                    break;
            }
        }

        private void UpdateStateMachine()
        {
            switch (_state)
            {
                case State.Wander:
                    UpdateWander();
                    break;
                case State.HandHover:
                    UpdateHandHover();
                    break;
                case State.HeadOrbit:
                    UpdateHeadOrbit();
                    break;
            }
        }

        private void FixedUpdateStateMachine()
        {
            switch (_state)
            {
                case State.Wander:
                    FixedUpdateWander();
                    break;
                case State.HandHover:
                    FixedUpdateHandHover();
                    break;
                case State.HeadOrbit:
                    FixedUpdateHeadOrbit();
                    break;
            }
        }

        private void OnCollisionEnterStateMachine(Collision collision)
        {
            switch (_state)
            {
                case State.Wander:
                    OnCollisionEnterWander(collision);
                    break;
            }
        }

        //----------- Wander State Methods -----------
        /// <summary>
        /// This is the default state. We remain in this state until a hand detected or an eyes closed event occurs.  
        /// The cloud will choose a random point in front of the user to travel to. 
        /// Upon reaching that point, the cloud will idle for a time before repeating the process.
        /// </summary>

        private void EnterWander()
        {
            _maxSpeed = _blinkTuning.maxWanderSpeed;
            ChooseNewTargetPos();
        }

        private void UpdateWander()
        {
            // Check if we should transition to different state.
            if (_eyeInput.eyesClosed)
            {
                GoToState(State.HeadOrbit);
            }
            else if (_gestureInput.HandPositionGestureDetected())
            {
                GoToState(State.HandHover);
            }
        }

        private void FixedUpdateWander()
        {
            if (_moveToPos)
            {
                float dist = Vector3.Distance(_wanderPoint, transform.position);
                if (dist > _blinkTuning.maxWanderDist)
                {
                    // We have wandered too far away from our current target, abort and pick a new point.
                    ChooseNewTargetPos();
                }
                if (dist > _blinkTuning.wanderDeadzone)
                {
                    // Move towards target position.
                    MoveTowardPositionWithForce(_wanderPoint, _blinkTuning.moveSpeedWander, false);
                }
                else
                {
                    // Stop and wait for a bit.
                    _moveToPos = false;
                    _wanderWait = Time.time + Random.Range(_blinkTuning.minWanderWait, _blinkTuning.maxWanderWait);
                }
            }
            else
            {
                // Check if wait is over so we can move again.
                if (Time.time > _wanderWait)
                {
                    ChooseNewTargetPos();
                }
            }
        }

        private void OnCollisionEnterWander(Collision collision)
        {
            ChooseNewTargetPos();
        }

        private void ExitWander()
        {
            // Logic for cleaning up the Wander state would go here.
        }

        //----------- HandHover State Methods -----------
        /// <summary>
        /// We enter this state when the user's hand is detected and their eyes are not closed.
        /// The cloud will move towards the user's hand and follow it around for as long as it is detected,
        /// or until an eyes closed event occurs.
        /// </summary>

        private void EnterHandHover()
        {
            _maxSpeed = _blinkTuning.maxHandSpeed;
        }

        private void UpdateHandHover()
        {
            // Check if we should transition to different state.
            if (_eyeInput.eyesClosed)
            {
                GoToState(State.HeadOrbit);
            }
            else if (!_gestureInput.HandPositionGestureDetected())
            {
                _handToWanderTimer += Time.deltaTime;
                if (_handToWanderTimer > _blinkTuning.handToWanderDelay)
                {
                    GoToState(State.Wander);
                }
            }
            else
            {
                _handToWanderTimer = 0f;
            }
        }

        private void FixedUpdateHandHover()
        {
            if (_gestureInput.HandPositionGestureDetected())
            {
                Vector3 handPos = transform.position;
                if (_gestureInput.TryGetHandPosition(ref handPos))
                {
                    Vector3 newOffset = Vector3.up * _blinkTuning.handHoverOffset.y;
                    Vector3 newPos = handPos + newOffset;
                    MoveTowardPositionWithForce(newPos, _blinkTuning.moveSpeedHand, true);
                }
            }
        }

        private void ExitHandHover()
        {
            // Logic for cleaning up the HandHover state would go here.
        }

        //----------- HeadOrbit State Methods -----------
        /// <summary>
        /// We enter this state whenever the user's eyes are detected as being closed.
        /// The cloud will move towards headpose and begin orbiting it, playing a spatialized
        /// audio track on loop.
        /// </summary>

        private void EnterHeadOrbit()
        {
            _maxSpeed = _blinkTuning.maxHeadSpeed;
            _rigidbody.isKinematic = true;
            _cloud.SetOrbit(true);
        }

        private void UpdateHeadOrbit()
        {
            // Check if we should transition to different state.
            if (!_eyeInput.eyesClosed)
            {
                // Wait until the cloud has orbited back in view of the user before changing states & speed up orbit speed.
                _cloudOrbitSpeed = _blinkTuning.orbitSpeedEyesOpen;
                if (CloudInViewOfHeadpose())
                {
                    if (_gestureInput.HandPositionGestureDetected())
                    {
                        GoToState(State.HandHover);
                    }
                    else
                    {
                        GoToState(State.Wander);
                    }
                }
            }
            else
            {
                // Set the orbit speed to the default.
                _cloudOrbitSpeed = _blinkTuning.orbitSpeed;
            }
        }

        private void FixedUpdateHeadOrbit()
        {
            float dist = Vector3.Distance(_headpose.position, transform.position);
            float delta = Mathf.Abs(dist - _blinkTuning.orbitDist);
            Vector3 pos;
            if (delta > _blinkTuning.orbitDeadzone)
            {
                // Move towards headpose.
                if (dist > _blinkTuning.orbitDist)
                {
                    pos = _headpose.position;
                }
                else
                {
                    pos = _headpose.position + transform.forward * _blinkTuning.orbitDist;
                }
                MoveTowardPosition(pos, _blinkTuning.moveSpeedHead);
            }
            else
            {
                // Orbit around headpose.
                transform.RotateAround(_headpose.position, Vector3.up, _cloudOrbitSpeed * Time.deltaTime);
                // Always orbit at head level.
                pos = transform.position;
                pos.y = _headpose.position.y;
                MoveTowardPosition(pos, _blinkTuning.moveSpeedHead);
            }
        }

        private void ExitHeadOrbit()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = _velocity;
            _cloud.SetOrbit(false);
        }

        //----------- Private Methods -----------

        private void MoveTowardPositionWithForce(Vector3 movePos, float speed, bool opposingForce)
        {
            float mult = 1f;
            float dist = Vector3.Distance(movePos, transform.position);
            Vector3 moveDir = (movePos - transform.position).normalized;
            float dot = Vector3.Dot(moveDir, _rigidbody.velocity.normalized);
            if (dot < 0 && opposingForce)
            {
                mult = Mathf.Abs(dot) * _blinkTuning.opposingForceMult;
            }
            if (dist > StopDist)
            {
                _rigidbody.AddForce(moveDir * _rigidbody.mass * speed * mult);
            }
        }

        private void MoveTowardPosition(Vector3 movePos, float speed)
        {
            float dist = Vector3.Distance(movePos, transform.position);
            float distRatio = dist / StopDist;
            Vector3 moveDir = (movePos - transform.position).normalized;
            _rigidbody.MovePosition(transform.position + moveDir * distRatio * speed * Time.deltaTime);
        }

        private void ChooseNewTargetPos()
        {
            _wanderPoint = CalculateNewTargetPos();
            _moveToPos = true;
        }

        private Vector3 CalculateNewTargetPos()
        {
            float wanderDist = Random.Range(_blinkTuning.minWanderDist, _blinkTuning.maxWanderDist);
            // Raycast origin point is slightly above headpose.
            Vector3 origin = _headpose.position + (Vector3.up * _blinkTuning.heightAboveHeadpose);
            // Perform several raycasts in a frontal cone.
            float maxAngle = _blinkTuning.wanderZoneConeAngle /2f;
            float angle = -maxAngle;
            List<Vector3> validPoints = new List<Vector3>();
            float angleStep = _blinkTuning.wanderZoneConeAngle / _blinkTuning.wanderZoneConeResolution;
            while (angle < maxAngle) 
            {
                RaycastHit hit;
                Vector3 castDir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.ProjectOnPlane(_headpose.forward, Vector3.up);
                Debug.DrawRay(origin, castDir, Color.yellow, 1f);
                angle += angleStep;
                if (!CastDirection(castDir, origin, wanderDist, out hit))
                {
                    // Nothing is in the way.
                    Vector3 newPos = origin + castDir;
                    float distFromCurrentPos = Vector3.Distance(transform.position, newPos);
                    if(distFromCurrentPos > _blinkTuning.minDistBetweenWanderPoints)
                    {
                        validPoints.Add(newPos);
                    }
                }
            }
            if(validPoints.Count > 0)
            {
                // Choose a valid position at random.
                int randomIndex = Random.Range(0, validPoints.Count);
                return validPoints[randomIndex];
            }
            else
                // Fallback to current position if no valid position was found.
                return transform.position;
        }

        private bool CastDirection(Vector3 dir, Vector3 origin, float maxDist, out RaycastHit hit)
        {
            return Physics.SphereCast(origin, _collider.radius, dir, out hit, maxDist, _environmentLayer);
        }

        private bool CloudInViewOfHeadpose()
        {
            Vector3 headposeToCloudDir = (transform.position - _headpose.position).normalized;
            float dot = Vector3.Dot(_headpose.forward, headposeToCloudDir);
            return dot >= _viewVecDotThresh;
        }
    }
}