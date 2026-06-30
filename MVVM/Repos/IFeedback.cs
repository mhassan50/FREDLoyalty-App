using FREDLoyalty_App.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Repos
{
    public interface IFeedback
    {
        Task<Mdl_CustomerFeedbackResult> InsertFeedbackAsync(Mdl_CustomerFeedback model);
        Task<Mdl_CustomerSuggestionResult> InsertSuggestionAsync(Mdl_CustomerSuggestion model);
    }
}
