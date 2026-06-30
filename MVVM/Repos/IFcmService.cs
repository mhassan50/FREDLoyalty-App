using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Repos
{
    public interface IFcmService
    {
        Task<string> GetTokenAsync();
        Task SaveTokenToApiAsync(string customerCode, string token);
    }
}
