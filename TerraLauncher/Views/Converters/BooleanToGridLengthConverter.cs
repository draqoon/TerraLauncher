using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TerraLauncher.Views.Converters {

	[ValueConversion( typeof( bool ), typeof( GridLength ) )]
	class BooleanToGridLengthConverter: IValueConverter {

		private readonly GridLength Zero = new GridLength( 0 );

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			if( value is bool b && !b ) {
				return Zero;
			}
			return GridLength.Auto;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
			=> throw new NotImplementedException();
	}
}
