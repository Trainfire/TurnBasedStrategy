using UnityEngine;
using System;

namespace Framework
{
    // Maps bindings to an input
    public abstract class InputMap : MonoBehaviour
    {
        #region Core Generic Bindings
        public const string Horizontal = "Horizontal";
        public const string Vertical = "Vertical";
        public const string Horizontal2 = "Horizontal2";
        public const string Vertical2 = "Vertical2";
        public const string Up = "Up";
        public const string UpRight = "UpRight";
        public const string Right = "Right";
        public const string DownRight = "DownRight";
        public const string Down = "Down";
        public const string DownLeft = "DownLeft";
        public const string Left = "Left";
        public const string UpLeft = "UpLeft";
        public const string ScrollUp = "ScrollUp";
        public const string ScrollDown = "ScrollDown";
        public const string Back = "Back";
        public const string Start = "Pause";
        public const string LeftClick = "LeftClick";
        public const string RightClick = "RightClick";
        public const string MiddleClick = "MiddleClick";
        public const string LeftTrigger = "LeftTrigger";
        public const string RightTrigger = "RightTrigger";
        #endregion

        public event EventHandler<InputActionEvent> Trigger;

        protected abstract InputContext Context { get; }

        protected void FireTrigger(InputActionEvent actionEvent)
        {
            if (Trigger != null)
                Trigger(this, actionEvent);
        }

        protected void FireTrigger(string actionName, InputActionType actionType, float delta = 0f)
        {
            FireTrigger(new InputActionEvent(actionName, Context, actionType, delta));
        }

        /// <summary>
        /// Maps a Unity InputManager axis to an action.
        /// </summary>
        /// <param name="axisName">The name of the axis in the InputManager.</param>
        /// <param name="action">The action to trigger.</param>
        /// <returns></returns>
        protected float GetAxis(string axisName, string action)
        {
            var axisValue = Input.GetAxis(axisName);

            if (Mathf.Abs(axisValue) > 0f)
            {
                FireTrigger(action, InputActionType.Axis, axisValue);
                return axisValue;
            }

            return 0f;
        }
    }
}
