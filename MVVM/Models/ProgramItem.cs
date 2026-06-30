using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Models
{
    public class ProgramItem
    {
        public string LoyaltyName { get; set; }
        public string ItemName { get; set; }
        public string DealLabel { get; set; }  // "Buy 10 Get 1 Free"
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public Color EndDateColor { get; set; }  // red if expiring soon
        public string StatusLabel { get; set; }  // "ACTIVE" / "ENDING SOON"
        public Color StatusBgColor { get; set; }
        public Color StatusTextColor { get; set; }
    }
}
