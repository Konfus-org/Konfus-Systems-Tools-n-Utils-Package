using UnityEngine;

namespace Konfus.Tools.Graph_Editor.Editor.Utils
{
    public static class MouseUtils
    {
        public static bool IsNone(this EventModifiers modifiers)
        {
            return modifiers == EventModifiers.None;
        }

        public static bool IsShift(this EventModifiers modifiers)
        {
            return (modifiers & EventModifiers.Shift) != 0;
        }

        public static bool IsActionKey(this EventModifiers modifiers)
        {
            return PlatformUtils.IsMac
                ? (modifiers & EventModifiers.Command) != 0
                : (modifiers & EventModifiers.Control) != 0;
        }

        public static bool IsExclusiveShift(this EventModifiers modifiers)
        {
            return modifiers == EventModifiers.Shift;
        }

        public static bool IsExclusiveActionKey(this EventModifiers modifiers)
        {
            return PlatformUtils.IsMac
                ? modifiers == EventModifiers.Command
                : modifiers == EventModifiers.Control;
        }
    }
}