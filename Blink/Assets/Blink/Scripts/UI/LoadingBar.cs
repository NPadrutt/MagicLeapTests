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
    /// Handles visual representation of a loading bar.
    /// </summary>
    public class LoadingBar : MonoBehaviour
    {
        //----------- Private Members -----------

        [SerializeField] private Animator _animator;
        // The name of the parameter in the animator that updates the progress.
        [SerializeField] private string _animParamName = "progress";

        //----------- MonoBehaviour Methods -----------

        /// <summary>
        /// Update the animator with the current progress amount (value between 0-1)
        /// </summary>
        public void UpdateProgress(float progress)
        {
            _animator.SetFloat(_animParamName, progress);
        }
    }
}