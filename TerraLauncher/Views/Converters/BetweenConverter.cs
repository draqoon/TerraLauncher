using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TerraLauncher.Views.Converters {

	[ValueConversion( typeof( IComparable ), typeof( bool ), ParameterType = typeof( string ) )]
	class BetweenConverter: IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var comparableValue = (IComparable)value;
			var str = ( (string)parameter ).Split( ',' );

			if( !string.IsNullOrWhiteSpace( str[0] ) ) {
				var min = (IComparable)System.Convert.ChangeType( str[0], value.GetType() );
				if( comparableValue.CompareTo( min ) < 0 )
					return false;
			}

			if( 1 < str.Length && !string.IsNullOrWhiteSpace( str[1] ) ) {
				var max = (IComparable)System.Convert.ChangeType( str[1], value.GetType() );
				if( 0 < comparableValue.CompareTo( max ) )
					return false;
			}

			return true;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
			=> throw new NotImplementedException();
	}
}
