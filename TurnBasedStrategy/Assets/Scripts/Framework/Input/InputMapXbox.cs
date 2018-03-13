using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework
{
    public enum InputXbox
    {
        ButtonA,
        ButtonB,
        ButtonX,
        ButtonY,
        LBumper,
        RBumper,
        Back,
        Start,
        LStickClick,
        RStickClick,
    }

    public class InputMapXbox : InputMap
    {
        private Dictionary<InputXbox, string> _actionBindings;
        private Dictionary<InputXbox, string> _buttonBindings;

        protected override InputContext Context
        {
            get { return InputContext.Xbox; }
        }

        public void Awake()
        {
            _actionBindings = new Dictionary<InputXbox, string>();

            // Default bindings for face buttons.
            // Note: These are Windows-specific.
            _buttonBindings = new Dictionary<InputXbox, string>();
            _buttonBindings.Add(InputXbox.ButtonA, "joystick button 0");
            _buttonBindings.Add(InputXbox.ButtonB, "joystick button 1");
            _buttonBindings.Add(InputXbox.ButtonX, "joystick button 2");
            _buttonBindings.Add(InputXbox.ButtonY, "joystick button 3");
            _buttonBindings.Add(InputXbox.LBumper, "joystick button 4");
            _buttonBindings.Add(InputXbox.RBumper, "joystick button 5");
            _buttonBindings.Add(InputXbox.Back, "joystick button 6");
            _buttonBindings.Add(InputXbox.Start, "joystick button 7");
            _buttonBindings.Add(InputXbox.LStickClick, "joystick button 8");
            _buttonBindings.Add(InputXbox.RStickClick, "joystick button 9");
        }

        public void AddBinding(InputXbox trigger, string action)
        {
            if (_actionBindings.ContainsKey(trigger))
            {
                Debug.LogErrorFormat("InputMapXbox: '{0}' is already bound to '{1}'", action, trigger);
            }
            else
            {
                _actionBindings.Add(trigger, action);
            }
        }

        public void LateUpdate()
        {
            foreach (var kvp in _buttonBindings)
            {
                if (Input.anyKey)
                {
                    if (Input.GetKeyDown(kvp.Value) && _actionBindings.ContainsKey(kvp.Key))
                        FireTrigger(_actionBindings[kvp.Key], InputActionType.Down);

                    if (Input.GetKey(kvp.Value) && _actionBindings.ContainsKey(kvp.Key))
                        FireTrigger(_actionBindings[kvp.Key], InputActionType.Held);
                }

                if (Input.GetKeyUp(kvp.Value) && _actionBindings.ContainsKey(kvp.Key))
                    FireTrigger(_actionBindings[kvp.Key], InputActionType.Up);
            }

            // Sticks
            GetAxis("Horizontal", Horizontal);
            GetAxis("Vertical", Vertical);
            GetAxis("Horizontal2", Horizontal2);
            GetAxis("Vertical2", Vertical2);

            // D-Pad
            var _dpadX = Input.GetAxis("D Pad X");
            var _dpadY = Input.GetAxis("D Pad Y");

            if (_dpadX > 0f)
            {
                FireTrigger(Right, InputActionType.Held);
            }
            else if (_dpadX < 0f)
            {
                FireTrigger(Left, InputActionType.Held);
            }

            if (_dpadY > 0f)
            {
                FireTrigger(Up, InputActionType.Held);
            }
            else if (_dpadY < 0f)
            {
                FireTrigger(Down, InputActionType.Held);
            }

            // Triggers
            GetAxis("Left Trigger", LeftTrigger);
            GetAxis("Right Trigger", RightTrigger);
        }
    }
}
