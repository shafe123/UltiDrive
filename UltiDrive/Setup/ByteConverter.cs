using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UltiDrive.Setup
{
    class ByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
    System.Globalization.CultureInfo culture)
        {
            long numBytes = (long)value;

            //kiloBytes
            if (numBytes > 1024*.5 && numBytes <= 1024 * 512)
            {
                return (double)(numBytes / 1024.0) + " kB";
            }
            //megaBytes
            else if (numBytes > 1024 * 512 && numBytes <= 1024 * 1024 * 512)
            {
                return (double)(numBytes / 1024 / 1024.0) + " MB";
            }
            else if (numBytes > 1024 * 1024 * 512 && numBytes <= 1024.0 * 1024 * 1024 * 512)
            {
                return (double)(numBytes / 1024 / 1024 / 1024.0) + " GB";
            }
            else
            {
                return (double)numBytes + " Bytes";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
