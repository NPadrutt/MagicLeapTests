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
    public class CloudAnimStateMachine : StateMachineBehaviour
    {
        //----------- Public events -----------

        public static event System.Action<Cloud.CloudAnimStates> OnStateEntered;

        //----------- Public Members -----------

        public Cloud.CloudAnimStates cloudAnimState;

        //----------- Event Handlers -----------

        /// <summary>
        /// When entering a state, fire off an event.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="stateInfo">State info.</param>
        /// <param name="layerIndex">Layer index.</param>
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (OnStateEntered != null)
            {
                OnStateEntered(cloudAnimState);
            }
        }
    }
}
