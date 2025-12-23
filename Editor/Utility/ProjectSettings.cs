using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Editor.Context_Menu;
using UnityEditor;

namespace Konfus.Editor.Utility
{
    internal static class ProjectSettings
    {
        public const string DoNotAskAgainId = "Konfus.DoNotAskAgainDialog.";
        public const string PinnedMenuItemsId = "Konfus.PinnedItems";

        public static List<MenuItemReflection.MenuItemInfo> LoadPinnedItems(
            IEnumerable<MenuItemReflection.MenuItemInfo> allItems)
        {
            var pinnedItems = new List<MenuItemReflection.MenuItemInfo>();
            string? savedPinnedItemStr = EditorPrefs.GetString(PinnedMenuItemsId);
            List<string> savedPinnedItemsList = (savedPinnedItemStr?.Split(',') ?? Array.Empty<string>()).ToList();
            foreach (string? item in savedPinnedItemsList)
            {
                MenuItemReflection.MenuItemInfo? menuItem = allItems.FirstOrDefault(i => i.MenuPath == item);
                pinnedItems.Add(menuItem ?? new MenuItemReflection.MenuItemInfo(item, false, null));
            }

            return pinnedItems;
        }

        public static void SavePinnedItems(IEnumerable<MenuItemReflection.MenuItemInfo> pinned)
        {
            string serializedItems = string.Join(", ", pinned.Select(i => i.MenuPath).ToArray());
            EditorPrefs.SetString(PinnedMenuItemsId, serializedItems);
        }
    }
}