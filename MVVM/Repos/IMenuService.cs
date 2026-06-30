using FREDLoyalty_App.MVVM.Models;

namespace FREDLoyalty_App.MVVM.Repos
{
    public interface IMenuService
    {
        Task<Mdl_MenuResult> GetMenuItemsAsync(Mdl_MenuCriteria criteria);
    }
}