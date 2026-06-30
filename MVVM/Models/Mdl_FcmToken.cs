using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Models
{
    public class Mdl_FcmToken
    {
        public string CustomerCode { get; set; }
        public string FcmToken { get; set; }
        public string Platform { get; set; } = "Android";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Mdl_FcmTokenResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
