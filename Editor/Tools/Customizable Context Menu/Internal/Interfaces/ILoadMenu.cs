using System.Collections.Generic;

namespace Konfus.Tools.Customizable_Context_Menu.Internal.Interfaces
{
	public interface ILoadMenu
	{
		void OnModifyCollectedItems(List<MenuItemInfo> items);
	}
}