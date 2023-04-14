using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Konfus.Tools.Customizable_Context_Menu.Internal
{
	public class Context
	{
		public string Guid;
		public Rect Rect;
		public EditorWindow Window;
		public List<MenuItemInfo> Items;
	}
}