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
using UnityEngine.Events;
using MagicLeap.Utilities;

[System.Serializable]
public class StringEvent : UnityEvent<string> { }

[System.Serializable]
public class Vector2Event : UnityEvent<Vector2> { }

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

[System.Serializable]
public class BoolEvent : UnityEvent<bool> { }

[System.Serializable]
public class TransformEvent : UnityEvent<Transform> { }