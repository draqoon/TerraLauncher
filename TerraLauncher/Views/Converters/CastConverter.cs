using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TerraLauncher.Views.Converters {

	[ValueConversion( typeof( string ), typeof( object ), ParameterType = typeof( Type ) )]
	class CastConverter: IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			try {
				if( parameter is Type type ) {
					return System.Convert.ChangeType( value, type );
				}
			}
			catch {
			}
			return value;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
			=> value?.ToString();

	}
}
