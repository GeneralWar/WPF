using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace General.WPF
{
    static public partial class Extension
    {
        public static Vector GetVisualOffset(this Visual visual)
        {
            PropertyInfo info = visual.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Vector)info.GetValue(visual);
        }
    }
}
