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
using UnityEngine.Events;

namespace MagicLeap.Utilities
{
    public class Selection : Singleton<Selection>
    {
        #region Public Events
        public TransformEvent OnSelected;
        public TransformEvent OnDeselected;
        #endregion

        #region Public Properties
        public Transform[] Current
        {
            get
            {
                List<Transform> validSelections = new List<Transform>();

                //validate all entries in _currentSelections to ensure we do not provide any nulls:
                foreach (var item in _currentSelections)
                {
                    if (item != null)
                    {
                        validSelections.Add(item);
                    }
                }

                return validSelections.ToArray();
            }
        }
        #endregion

        #region Private Variables
        [SerializeField] float _maxDistance;
        [SerializeField] Transform[] _sources;
        [SerializeField] GameObject _cursor;
        [SerializeField] float _cursorOffset;
        GameObject[] _cursors;
        List<Transform> _activeSelections;
        Transform[] _currentSelections;
        #endregion

        #region Init
        private void Reset()
        {
            _maxDistance = 4.572f;
            _cursorOffset = 0.0127f;
        }

        void Awake ()
        {
            if (_cursor == null)
            {
                Debug.LogError("Please provide a cursor object.");
                enabled = false;
            }

            //clean up cursor template:
            _cursor.SetActive(false);
            foreach (var item in _cursor.GetComponentsInChildren<Collider>())
            {
                item.enabled = false;
            }

            //selections:
            _activeSelections = new List<Transform>();
            _currentSelections = new Transform[0];

            //create cursors:
            _cursors = new GameObject[_sources.Length];
            for (int i = 0; i < _sources.Length; i++)
            {
                _cursors[i] = GameObject.Instantiate<GameObject>(_cursor);
                _cursors[i].name = "(Cursor)";
                _cursors[i].transform.parent = transform;
                _cursors[i].SetActive(false);
            }
        }
        #endregion

        #region Loops
        private void Update()
        {
            _activeSelections.Clear();

            //see if any of our sources are pointing at anything:
            for (int i = 0; i < _sources.Length; i++)
            {
                RaycastHit hit;
                if (Physics.Raycast (_sources[i].position, _sources[i].forward, out hit, _maxDistance))
                {
                    _cursors[i].SetActive(true);
                    if (!_activeSelections.Contains(hit.transform)) _activeSelections.Add(hit.transform);
                    Debug.DrawLine(_sources[i].position, hit.point, Color.green);
                    _cursors[i].transform.position = hit.point + (hit.normal * _cursorOffset);
                    _cursors[i].transform.rotation = Quaternion.LookRotation(hit.normal);
                }
                else
                {
                    _cursors[i].SetActive(false);
                    Debug.DrawRay(_sources[i].position, _sources[i].forward * _maxDistance, Color.red);
                }
            }
            
            //if a current selection was not found again then it was deselected:
            foreach (var item in _currentSelections)
            {
                //the item was likely destroyed:
                if (item == null)
                {
                    continue;
                }

                if (!_activeSelections.Contains(item))
                {
                    if (OnDeselected != null) OnDeselected.Invoke(item);
                    ISelectable selectable = item.GetComponent(typeof(ISelectable)) as ISelectable;
                    if (selectable != null)
                    {
                        selectable.OnDeselected();
                    }
                }
            }

            //newly selected objects:
            foreach (var item in _activeSelections)
            {
                if (System.Array.IndexOf(_currentSelections, item) == -1)
                {
                    if (OnSelected != null) OnSelected.Invoke(item);
                    ISelectable selectable = item.GetComponent(typeof(ISelectable)) as ISelectable;
                    if (selectable != null)
                    {
                        selectable.OnSelected();
                    }
                }
            }

            //catalog current selections:
            _currentSelections = _activeSelections.ToArray();
        }
        #endregion
    }
}