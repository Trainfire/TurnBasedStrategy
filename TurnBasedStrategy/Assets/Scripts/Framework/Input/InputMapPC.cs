using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework
{
    public class InputMapPC : InputMap
    {
        private Dictionary<string, KeyCode> _bindings;

        protected override InputContext Context
        {
            get { return InputContext.PC; }
        }

        public void Awake()
        {
            _bindings = new Dictionary<string, KeyCode>();
        }

        public void AddBinding(string action, KeyCode key)
        {
            if (_bindings.ContainsKey(action))
            {
                Debug.LogErrorFormat("InputMapPC: '{0}' is already bound to '{1}'", action, key);
            }
            else
            {
                _bindings.Add(action, key);
            }
        }

        public void LateUpdate()
        {
            foreach (var kvp in _bindings)
            {
                if (Input.anyKey)
                {
                    if (Input.GetKeyDown(kvp.Value))
                        FireTrigger(kvp.Key, InputActionType.Down);

                    if (Input.GetKey(kvp.Value))
                        FireTrigger(kvp.Key, InputActionType.Held);
                }

                if (Input.GetKeyUp(kvp.Value))
                    FireTrigger(kvp.Key, InputActionType.Up);
            }

            GetAxis("Mouse X", Horizontal);
            GetAxis("Mouse Y", Vertical);

            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                FireTrigger(ScrollUp, InputActionType.Down);

            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                FireTrigger(ScrollDown, InputActionType.Down);

            if (Input.GetMouseButtonDown(0))
                FireTrigger(LeftClick, InputActionType.Down);

            if (Input.GetMouseButtonDown(1))
                FireTrigger(RightClick, InputActionType.Down);
        }
    }
}
