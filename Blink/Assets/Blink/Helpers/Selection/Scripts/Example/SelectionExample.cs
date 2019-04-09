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

namespace MagicLeap.Utilities
{
    ///<summary>
    /// 
    ///</summary>
    public class SelectionExample : MonoBehaviour, ISelectable
    {
        //----------- Private Members -----------
        Renderer _renderer;
        Color _initialColor;
        Vector3 _initialScale;

		//----------- MonoBehaviour Methods -----------
		private void Awake()
		{
            _renderer = GetComponent<Renderer>();
            _initialColor = _renderer.material.color;
            _initialScale = transform.localScale;
		}

		//----------- Event Handlers -----------
		public void OnDeselected()
        {
            Tween.Add(_renderer.material.color, _initialColor, .25f, 0, Tween.EaseLinear, Tween.LoopType.None, (value) => _renderer.material.color = value);
            Tween.Add(transform.localScale, _initialScale, .25f, 0, Tween.EaseOutBack, Tween.LoopType.None, (value) => transform.localScale = value);
        }

        public void OnSelected()
        {
            Tween.Add(_renderer.material.color, Color.green, .15f, 0, Tween.EaseLinear, Tween.LoopType.None, (value) => _renderer.material.color = value);
            Tween.Add(transform.localScale, _initialScale * 1.25f, .15f, 0, Tween.EaseOutBack, Tween.LoopType.None, (value) => transform.localScale = value);
        }
    }
}