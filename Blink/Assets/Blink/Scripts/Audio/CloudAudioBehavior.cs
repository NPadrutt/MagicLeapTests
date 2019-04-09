// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeap.Utilities;

namespace MagicKit
{
	/// <summary>
	/// Cloud audio handler.
	/// </summary>
	public class CloudAudioBehavior : AudioBehavior
	{
        //----------- Private Members -----------

        private const string SpawnEventName = "spawn";
        private const string AmbientEventName = "ambient";
        private const string OrbitEventName = "orbit";
        private const string OrbitAmbienceEventName = "orbit_ambience";
        private const string OrbitAccentEventName = "orbit_accent";
        private const string OrbitStart = "orbit_start";
        private const string RainEventName = "rain";
        private const string ThunderEventName = "thunder";
        private const string HandEventName = "hand";

        [SerializeField] private AudioSource _ambientAudioSource;

        [SerializeField] private AudioSource _handAudioSource;
        [SerializeField] private float _handDetectedSoundDelay = 1.0f;

        [SerializeField] private AudioSource _rainAudioSource;
        [SerializeField] private float _fadeInRainDuration = 2.0f;
        [SerializeField] private float _fadeOutRainDuration = 2.0f;

        [SerializeField] private float _fadeInPadSoundDuration = 0.5f;
        [SerializeField] private float _fadeOutPadSoundDuration = 1.5f;

        [SerializeField] private AudioSource _orbitAudioSource;
        [SerializeField] private AudioSource _orbitAmbienceAudioSource;
        [SerializeField] private float _delayOrbit = 3.0f;
        [SerializeField] private float _fadeInOrbitDuration = 2.0f;
        [SerializeField] private float _fadeOutOrbitDuation = 2.0f;
        [SerializeField] private float _orbitAccentsDelay = 2.0f;
        [SerializeField] private float _orbitAccentsVariance = 2.0f;

        [SerializeField] private BlinkGestures _gestureInput;

        //----------- Private Members -----------

        private bool _handDetected = false;
        private Vector3? _handPosition;

        //----------- MonoBehaviour Methods -----------

        private void FixedUpdate()
        {
            bool handDetected = _gestureInput.HandPositionGestureDetected();

            if (handDetected == _handDetected)
            {
                if (_handDetected == true)
                {
                    TryGetHandPosition();
                }
                return;
            }

            if (handDetected)
            {
                TryGetHandPosition();
                StopCoroutine("StopHand");
                StartCoroutine("Hand");
            }
            else
            {
                StopCoroutine("Hand");
                StartCoroutine("StopHand");
            }

            _handDetected = handDetected;
        }

        //----------- Public Methods -----------

        /// <summary>
        /// Cloud is spawned.
        /// </summary>
        public void Spawn()
        {
            PlaySound(SpawnEventName);
            PlaySound(AmbientEventName, _ambientAudioSource);
        }

        /// <summary>
        /// Plays/Stops the orbit sfx on the orbit channel.
        /// </summary>
        public void PlayOrbit(bool status)
        {
            if (status)
            {
                StartCoroutine("Orbit");
            }
            else
            {
                FadeOutAudio(_orbitAudioSource, _fadeOutOrbitDuation, false);
                FadeOutAudio(_orbitAmbienceAudioSource, _fadeOutOrbitDuation, false);
                StopCoroutine("Orbit");
            }
        }

        /// <summary>
        /// Plays/Stops the rain sfx on the rain channel.
        /// </summary>
        public void PlayRain(bool status)
        {
            if (status)
            {
                if (_rainAudioSource.isPlaying)
                {
                    FadeInAudio(_rainAudioSource, _fadeInRainDuration);
                }
                else
                {
                    PlayAndFadeInSound(RainEventName, _fadeInRainDuration, _rainAudioSource);
                }
            }
            else
            {
                if (_rainAudioSource.isPlaying)
                {
                    FadeOutAudio(_rainAudioSource, _fadeOutRainDuration, false);
                }
            }
        }

        /// <summary>
        /// Plays the thunder sfx.
        /// </summary>
        public void PlayThunder()
        {
            PlaySound(ThunderEventName);
        }

        //----------- Private Methods -----------

        private void TryGetHandPosition()
        {
            Vector3 handPosition = new Vector3();
            if (_gestureInput.TryGetHandPosition(ref handPosition))
            {
                if (_handPosition == null)
                {
                    _handPosition = handPosition;
                }
                else
                {
                    _handPosition = Vector3.Lerp((Vector3)_handPosition, handPosition, 0.5f);
                }
            }
            else if (_handPosition == null)
            {
                _handPosition = transform.position;
            }

            _handAudioSource.transform.position = (Vector3)_handPosition;
        }

        //----------- Coroutines -----------

        private IEnumerator Orbit()
        {
            // Don't start the orbit loop right away, just in case there's a false positive.
            yield return new WaitForSeconds(_delayOrbit);

            PlaySound(OrbitStart);

            // Start orbit loop.
            if (_orbitAudioSource.isPlaying)
            {
                FadeInAudio(_orbitAudioSource, _fadeInOrbitDuration);
                FadeInAudio(_orbitAmbienceAudioSource, _fadeInOrbitDuration);
            }
            else
            {
                PlayAndFadeInSound(OrbitEventName, _fadeInOrbitDuration, _orbitAudioSource);
                PlayAndFadeInSound(OrbitAmbienceEventName, _fadeInOrbitDuration, _orbitAmbienceAudioSource);
            }

            // Start playing orbit accents.
            float waitFor = 0.0f;
            while (true)
            {
                waitFor = Random.Range(_orbitAccentsDelay - _orbitAccentsVariance, _orbitAccentsDelay + _orbitAccentsVariance);

                yield return new WaitForSeconds(waitFor);

                PlaySound(OrbitAccentEventName);
            }
        }

        private IEnumerator Hand()
        {
            yield return new WaitForSeconds(_handDetectedSoundDelay);
            _handAudioSource.transform.position = (Vector3)_handPosition;
            if (_handAudioSource.isPlaying)
            {
                FadeInAudio(_handAudioSource, _fadeInPadSoundDuration);
            }
            else
            {
                PlayAndFadeInSound(HandEventName, _fadeInPadSoundDuration, _handAudioSource);
            }
        }

        private IEnumerator StopHand()
        {
            yield return new WaitForSeconds(_handDetectedSoundDelay);
            FadeOutAudio(_handAudioSource, _fadeOutPadSoundDuration, true);
        }
    }
}
