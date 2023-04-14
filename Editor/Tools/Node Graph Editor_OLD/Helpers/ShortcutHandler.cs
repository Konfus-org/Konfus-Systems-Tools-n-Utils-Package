using System;
using System.Collections.Generic;
using Konfus.Tools.Graph_Editor.Editor.Enums;
using UnityEngine;

namespace Konfus.Tools.Graph_Editor.Editor.Helpers
{
    public class ShortcutHandler
    {
        public static readonly Dictionary<SpecialKey, string> specialKeyDisplayNameLookup = new()
        {
            {SpecialKey.CommandExclusive, "⌘"},
            {SpecialKey.Command, "⌘"},
            {SpecialKey.Control, "Ctrl"},
            {SpecialKey.ControlExclusive, "Ctrl"},
            {SpecialKey.Shift, "⇧ " + SpecialKey.Shift}
        };

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private readonly SpecialKey[] isSystemControlKeyExclusive = new SpecialKey[] { SpecialKey.CommandExclusive };
        private readonly SpecialKey[] isShiftAndCommand = new SpecialKey[] { SpecialKey.Shift, SpecialKey.Command };
        private readonly SpecialKey[] isCommandsAndFunctionKey =
 new SpecialKey[] { SpecialKey.Command, SpecialKey.Function };
#else
        private readonly SpecialKey[] isSystemControlKeyExclusive = new SpecialKey[] {SpecialKey.ControlExclusive};
#endif
        private readonly SpecialKey[] isUnmodified = new SpecialKey[] {SpecialKey.Unmodified};
        private readonly SpecialKey[] isFunctionKey = new SpecialKey[] {SpecialKey.Function};

        internal static readonly Dictionary<SpecialKey, Func<EventModifiers, bool>> specialKeyChecks = new()
        {
            {SpecialKey.Command, (modifiers) => (modifiers & EventModifiers.Command) != 0},
            {SpecialKey.CommandExclusive, (modifiers) => modifiers == EventModifiers.Command},
            {SpecialKey.Control, (modifiers) => (modifiers & EventModifiers.Control) != 0},
            {SpecialKey.ControlExclusive, (modifiers) => modifiers == EventModifiers.Control},
            {SpecialKey.Shift, (modifiers) => (modifiers & EventModifiers.Shift) != 0},
            {SpecialKey.Function, (modifiers) => (modifiers & EventModifiers.FunctionKey) != 0},
            {SpecialKey.Unmodified, (modifiers) => modifiers == EventModifiers.None}
        };

        private Dictionary<KeyCode, KeyAction> keyActions = new();

        public KeyAction GetKeyAction(Actions action)
        {
            foreach (KeyValuePair<KeyCode, KeyAction> keyValue in keyActions)
                if (keyValue.Value.handler.actionType == action)
                    return keyValue.Value;
            return null;
        }

        public ShortcutHandler()
        {
            void AddSimpleAction(KeyCode keyCode, Actions actionType, SpecialKey[] checks,
                KeyAction.Handler altHandler = null)
            {
                keyActions.Add(keyCode, new KeyAction(keyCode, new KeyAction.Handler(checks, actionType), altHandler));
            }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            AddSimpleAction(KeyCode.Z, Actions.Undo, isSystemControlKeyExclusive, new(isShiftAndCommand, Actions.Redo));
            AddSimpleAction(KeyCode.Backspace, Actions.Delete, isCommandsAndFunctionKey);
#else
            AddSimpleAction(KeyCode.Z, Actions.Undo, isSystemControlKeyExclusive);
            AddSimpleAction(KeyCode.Y, Actions.Redo, isSystemControlKeyExclusive);
#endif
            AddSimpleAction(KeyCode.C, Actions.Copy, isSystemControlKeyExclusive);
            AddSimpleAction(KeyCode.V, Actions.Paste, isSystemControlKeyExclusive);
            AddSimpleAction(KeyCode.X, Actions.Cut, isSystemControlKeyExclusive);
            AddSimpleAction(KeyCode.D, Actions.Duplicate, isSystemControlKeyExclusive);
            AddSimpleAction(KeyCode.Delete, Actions.Delete, isFunctionKey);
            AddSimpleAction(KeyCode.F, Actions.Frame, isUnmodified);
            AddSimpleAction(KeyCode.F2, Actions.Rename, isFunctionKey);
        }


        public Actions Execute(KeyCode keyCode, EventModifiers modifiers)
        {
            if (keyActions.ContainsKey(keyCode))
            {
                Actions actionType = keyActions[keyCode].Execute(modifiers);
                if (actionType != Actions.NoAction) return actionType;
            }

            return Actions.NoAction;
        }
    }
}