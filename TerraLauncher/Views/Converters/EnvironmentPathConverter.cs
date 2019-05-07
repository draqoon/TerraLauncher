using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TerraLauncher.Views.Converters {

	[ValueConversion( typeof( string ), typeof( string ) )]
	class EnvironmentPathConverter: IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			if( value is string s ) {
				value = Environment.ExpandEnvironmentVariables( s );
			}
			return value;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			if( value is string s ) {
				var userProfile = Environment.GetEnvironmentVariable( "USERPROFILE" );
				if( s.StartsWith( userProfile ) ) {
					s = s.Replace( userProfile, "%USERPROFILE%" );
				}
				value = s;
			}
			return value;
		}

	}
}
