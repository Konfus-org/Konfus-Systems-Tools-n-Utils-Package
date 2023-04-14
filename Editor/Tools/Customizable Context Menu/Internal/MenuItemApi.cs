using UnityEditor;

namespace Konfus.Tools.Customizable_Context_Menu.Internal
{
	public static class MenuItemApi
	{
		public static string[] GetProjectMenuItems()
		{
			return Unsupported.GetSubmenus("Assets");
		}
	}
}