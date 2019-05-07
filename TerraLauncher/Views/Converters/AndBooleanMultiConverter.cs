using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TerraLauncher.Views.Converters {

	class AndBooleanMultiConverter: IMultiValueConverter {
		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
			=> values.All( v => v is bool b && b );

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
			=> throw new NotImplementedException();

	}
}
