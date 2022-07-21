using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TodoSynchronizer.Converters
{
    public class BoolToAppearanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool islogin = (bool)value;
            if (value == null)
                return Wpf.Ui.Common.ControlAppearance.Caution;
            if (islogin)
                return Wpf.Ui.Common.ControlAppearance.Primary;
            else
                return Wpf.Ui.Common.ControlAppearance.Caution;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
