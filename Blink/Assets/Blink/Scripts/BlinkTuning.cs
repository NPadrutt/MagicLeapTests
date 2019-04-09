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
    /// Collection of tuning variables specific to the Blink application.
    /// </summary>
    [CreateAssetMenu(menuName = "BlinkTuning")]
    public class BlinkTuning : ScriptableObject
    {
        //----------- Public Members -----------

        // Speed that the cloud moves at when moving to the head.
        public float moveSpeedHead = 0.05f;
        // Speed that the cloud moves at when moving to the hand.
        public float moveSpeedHand = 0.3f;
        // Speed that the cloud moves at when wandering.
        public float moveSpeedWander = 0.03f;
        // Max speed the cloud can travel at when wandering.
        public float maxWanderSpeed = 0.1f;
        // Max speed the cloud can travel at when moving to the head.
        public float maxHeadSpeed = 0.1f;
        // Max speed the cloud can travel at when moving to the hand.
        public float maxHandSpeed = 0.4f;
        // Speed that the cloud orbits around the head.
        public float orbitSpeed = 90f;
        // Speed that the clound orbits around the head after the eyes have been opened.
        public float orbitSpeedEyesOpen = 90f;
        // Distance that the cloud will orbit around the head at.
        public float orbitDist = 0.5f;
        // Deadzone for cloud orbit.
        public float orbitDeadzone = 0.1f;
        // Mult force to slow down cloud when it's current velocity is opposing it's desired velocity.
        public float opposingForceMult = 10f;
        // The height above headpose that the cloud will choose to float at.
        public float heightAboveHeadpose = 0.6f;
        // Deadzone for wander to choose a new point to travel to.
        public float wanderDeadzone = 0.1f;
        // Min time the cloud will wait to pick a new position after reaching it's target.
        public float minWanderWait = 1f;
        // Max time the cloud will wait to pick a new position after reaching it's target.
        public float maxWanderWait = 5f;
        // Min dist the cloud will wander away from headpose.
        public float minWanderDist = 0.5f;
        // Max dist the cloud will wander away from headpose.
        public float maxWanderDist = 2f;
        // Amount of time in the hand hover state, after a hand is no longer detected, that the cloud will wait before transitioning to the wander state.
        public float handToWanderDelay = 0.33f;
        // When the cloud picks a wander point it picks a point from a cone defined by this angle and the headpose forward vector.
        public float wanderZoneConeAngle = 60f;
        // Defines how many rays within the cone are raycasted out. Increasing this number means more rays.
        public float wanderZoneConeResolution = 15;
        // The minimum distance between the new wander point that is being chosen and the current position of the cloud.
        // Points less than this distance are considered invalid.
        public float minDistBetweenWanderPoints = 0.2f;
        // The offset above the hand that the cloud will choose to hover at when it's been summoned.
        public Vector3 handHoverOffset = new Vector3(0f, 0.1f, 0f);
    }
}
