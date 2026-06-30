using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using System.Collections.ObjectModel;

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {
        private readonly IMenuService _menuService;

        [ObservableProperty] private ObservableCollection<Mdl_MenuCategory> categories = new();
        [ObservableProperty] private bool   isLoading   = false;
        [ObservableProperty] private string branchName  = string.Empty;
        [ObservableProperty] private int    branchCode  = 0;

        public INavigation Navigation { get; set; }

        public MenuViewModel(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public void SetBranch(Mdl_Branch branch)
        {
            BranchCode = branch.BranchCode;
            BranchName = branch.BranchName ?? string.Empty;
        }

        public async Task LoadMenuAsync()
        {
            try
            {
                IsLoading = true;
                Categories.Clear();

                var result = await _menuService.GetMenuItemsAsync(new Mdl_MenuCriteria
                {
                    BranchCode = BranchCode,
                    IsActive   = true
                });

                if (result?.IsSuccess == true && result.Data != null)
                {
                    // Group items by category
                    var grouped = result.Data
                        .GroupBy(i => new { i.DispCategoryCode, i.DispCategoryName })
                        .Select(g =>
                        {
                            var cat = new Mdl_MenuCategory
                            {
                                CategoryCode = g.Key.DispCategoryCode,
                                CategoryName = g.Key.DispCategoryName
                            };
                            cat.AddRange(g);
                            return cat;
                        });

                    foreach (var cat in grouped)
                        Categories.Add(cat);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Menu] LoadMenu error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (Navigation != null)
                await Navigation.PopAsync();
        }
    }
}