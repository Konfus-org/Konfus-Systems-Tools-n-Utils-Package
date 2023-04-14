using System.Collections.Generic;
using Konfus.Tools.Customizable_Context_Menu.Internal;
using Konfus.Tools.Customizable_Context_Menu.Internal.Interfaces;
using UnityEngine;

namespace Konfus.Tools.Customizable_Context_Menu
{
	internal class AddCopyGuid : ILoadMenu
	{
		public void OnModifyCollectedItems(List<MenuItemInfo> items)
		{
			var it = new MenuItemInfo("Add Copy Guid", new GUIContent("Copy Guid"));
			it.BeforeInvoke += guid =>
			{
				GUIUtility.systemCopyBuffer = guid;
				return false;
			};
			items.Add(it);
		}
	}
}