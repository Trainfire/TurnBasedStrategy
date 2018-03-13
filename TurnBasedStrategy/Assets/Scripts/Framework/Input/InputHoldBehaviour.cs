using UnityEngine;
using System;

namespace Framework
{
    /// <summary>
    /// Adds a repeat behaviour for when a button is held down. When the button is held down and after an initial delay, OnTrigger will be invoked repeatedly with a delay between each call.
    /// </summary>
    public class InputHoldBehaviour : IInputHandler
    {
        public event Action OnTrigger;

        private string trigger;
        private bool buttonDown;
        private float buttonDownTimestamp;
        private float holdRepeatTimestamp;

        private const float HoldActivateDelay = 0.5f;
        private const float HoldRepeatDelay = 0.05f;

        public InputHoldBehaviour(string trigger)
        {
            this.trigger = trigger;
        }

        public void HandleInput(InputActionEvent action)
        {
            if (action.Type == InputActionType.Held && action.Action == trigger)
            {
                if (!buttonDown)
                {
                    buttonDown = true;
                    buttonDownTimestamp = Time.realtimeSinceStartup;
                }

                var time = Time.realtimeSinceStartup;

                if (time > buttonDownTimestamp + HoldActivateDelay)
                {
                    if (time > holdRepeatTimestamp + HoldRepeatDelay)
                    {
                        if (OnTrigger != null)
                            OnTrigger();

                        holdRepeatTimestamp = Time.realtimeSinceStartup;
                    }
                }
            }
            else
            {
                buttonDown = false;
            }
        }

        public void Destroy()
        {
            InputManager.UnregisterHandler(this);
            OnTrigger = null;
        }
    }
}
