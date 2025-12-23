using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Konfus.Editor.Context_Menu
{
    internal static class MenuItemReflection
    {
        public static IReadOnlyList<MenuItemInfo> GetAllMenuItems()
        {
            var results = new List<MenuItemInfo>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                // Skip obvious non-editor assemblies for sanity/perf
                if (!assembly.FullName!.Contains("Editor", StringComparison.OrdinalIgnoreCase) &&
                    !assembly.FullName!.Contains("UnityEditor", StringComparison.OrdinalIgnoreCase))
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray()!;
                }

                foreach (Type type in types)
                {
                    MethodInfo[] methods = type.GetMethods(
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);

                    foreach (MethodInfo method in methods)
                    {
                        IEnumerable<MenuItem> attributes = method.GetCustomAttributes<MenuItem>(false);
                        foreach (MenuItem? attr in attributes)
                        {
                            results.Add(new MenuItemInfo(
                                attr.menuItem,
                                attr.validate,
                                method));
                        }
                    }
                }
            }

            return results;
        }

        public static bool TryInvoke(MenuItemInfo item, out Exception? error)
        {
            error = null;

            if (item.Validate)
            {
                error = new InvalidOperationException(
                    $"Cannot invoke validation MenuItem: {item.MenuPath}");
                return false;
            }

            try
            {
                if (item.Method == null)
                    return EditorApplication.ExecuteMenuItem(item.MenuPath);
                item.Method.Invoke(null, null);
                return true;
            }
            catch (TargetInvocationException tie)
            {
                error = tie.InnerException ?? tie;
                return false;
            }
            catch (Exception e)
            {
                error = e;
                return false;
            }
        }

        // <summary>
        /// Removes Unity MenuItem shortcut tokens (% # & _) from the end of a menu path.
        /// </summary>
        private static string StripMenuShortcut(string menuPath)
        {
            if (string.IsNullOrWhiteSpace(menuPath))
                return menuPath;

            int lastSpace = menuPath.LastIndexOf(' ');
            if (lastSpace < 0)
                return menuPath;

            string suffix = menuPath.Substring(lastSpace + 1);

            // Valid shortcut suffixes:
            // %n, %#n, %#&n, _, etc.
            if (!IsShortcutSuffix(suffix))
                return menuPath;

            return menuPath.Substring(0, lastSpace);
        }

        private static bool IsShortcutSuffix(string token)
        {
            // Explicit "no shortcut"
            if (token == "_")
                return true;

            // One or more modifier chars followed by a single key char
            var i = 0;
            while (i < token.Length && IsModifier(token[i]))
            {
                i++;
            }

            // Must consume at least one modifier and exactly one key
            return i > 0 && i == token.Length - 1;
        }

        private static bool IsModifier(char c)
        {
            return c == '%' || c == '#' || c == '&';
        }

        public sealed record MenuItemInfo(
            string MenuPath,
            bool Validate,
            MethodInfo? Method)
        {
            public string MenuPath { get; } = StripMenuShortcut(MenuPath);
            public bool Validate { get; } = Validate;
            public MethodInfo? Method { get; } = Method;
        }
    }
}