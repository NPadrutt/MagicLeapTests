// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.Utilities
{
    public interface ISelectable
    {
        void OnSelected();
        void OnDeselected();
    }
}