namespace Konfus.Tools.Customizable_Context_Menu.Internal.Interfaces
{
	public enum BeforeOpenMenuResponse
	{
		Continue = 0,
		Stop = 1,
	}
	
	public interface IBeforeOpenMenu
	{
		BeforeOpenMenuResponse OnOpenMenu(Context context);
	} 


}