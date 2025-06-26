using System.Globalization;
using System.Windows.Data;

namespace WpfApp.Converter
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            return value?.ToString()?.Equals(parameter?.ToString()) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || targetType == null)
            {
                return Binding.DoNothing;
            }

            // Fix for CS8600: Ensure parameter.ToString() is not null and targetType is an Enum type  
            if (value is bool booleanValue && booleanValue && parameter != null && targetType.IsEnum)
            {
                string? parameterString = parameter?.ToString();
                if (!string.IsNullOrEmpty(parameterString))
                {
                    return Enum.Parse(targetType, parameterString);
                }
            }

            return Binding.DoNothing;
        }
    }
}
