using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Models
{
    public class ImageItem
    {
        public string ImageSource { get; set; }
        public Color BgColor { get; set; }
        public Color BorderColor { get; set; }
        public bool IsPurchased { get; set; }

        /// <summary>
        /// 1.0 for purchased slots, 0.55 for empty slots (faded/muted look).
        /// Bound to the Border's Opacity in the CollectionView DataTemplate.
        /// </summary>
        public double Opacity { get; set; } = 1.0;
    }
}
