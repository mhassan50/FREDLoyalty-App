using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Views
{
    public static class AnimationExtensions
    {
        public static Task<bool> BackgroundColorTo(this View view, Color toColor, uint duration = 500)
        {
            var tcs = new TaskCompletionSource<bool>();
            var fromColor = view.BackgroundColor ?? Colors.Transparent;

            var animation = new Animation(t =>
            {
                view.BackgroundColor = new Color(
                    (float)(fromColor.Red + (toColor.Red - fromColor.Red) * t),
                    (float)(fromColor.Green + (toColor.Green - fromColor.Green) * t),
                    (float)(fromColor.Blue + (toColor.Blue - fromColor.Blue) * t),
                    (float)(fromColor.Alpha + (toColor.Alpha - fromColor.Alpha) * t));
            });

            animation.Commit(view, "BgColorAnim", 16, duration,
                Easing.SinInOut, (_, cancelled) => tcs.SetResult(!cancelled));

            return tcs.Task;
        }
    }
}
