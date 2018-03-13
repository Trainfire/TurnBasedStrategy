using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
    public enum InputContext
    {
        PC,
        Xbox,
    }

    // The type of action
    public enum InputActionType
    {
        None,
        Down,
        Up,
        Held,
        Axis,
    }

    public interface IInputHandler
    {
        void HandleInput(InputActionEvent action);
    }

    public interface IInputContextHandler
    {
        void HandleContextChange(InputContext context);
    }

    public class InputActionEvent : EventArgs
    {
        public string Action { get; private set; }
        public InputContext Context { get; private set; }
        public InputActionType Type { get; private set; }
        public float Delta { get; private set; }

        public InputActionEvent(string action, InputContext context, InputActionType type, float delta = 0f)
        {
            Action = action;
            Context = context;
            Type = type;
            Delta = delta;
        }
    }

    // Handles input from an input map and relays to a handler
    public static class InputManager
    {
        private static List<IInputHandler> inputHandlers;
        private static List<IInputContextHandler> contextHandlers;
        private static List<InputMap> maps;
        private static InputContext context;

        static InputManager()
        {
            inputHandlers = new List<IInputHandler>();
            contextHandlers = new List<IInputContextHandler>();
            maps = new List<InputMap>();
        }

        public static void RegisterHandler(IInputHandler handler)
        {
            if (!inputHandlers.Contains(handler))
                inputHandlers.Add(handler);
        }

        public static void UnregisterHandler(IInputHandler handler)
        {
            if (inputHandlers.Contains(handler))
                inputHandlers.Remove(handler);
        }

        public static void RegisterHandler(IInputContextHandler handler)
        {
            if (!contextHandlers.Contains(handler))
                contextHandlers.Add(handler);
        }

        public static void UnregisterHandler(IInputContextHandler handler)
        {
            if (contextHandlers.Contains(handler))
                contextHandlers.Remove(handler);
        }

        public static void RegisterMap(InputMap inputMap)
        {
            if (!maps.Contains(inputMap))
            {
                maps.Add(inputMap);
                inputMap.Trigger += Relay;
            }
        }

        public static void UnregisterMap(InputMap inputMap)
        {
            if (maps.Contains(inputMap))
            {
                maps.Remove(inputMap);
                inputMap.Trigger -= Relay;
            }
        }

        private static void Relay(object sender, InputActionEvent action)
        {
            if (action.Context != context)
            {
                context = action.Context;
                contextHandlers.ForEach(x => x.HandleContextChange(context));
            }

            inputHandlers.ForEach(x => x.HandleInput(action));
        }
    }
}
