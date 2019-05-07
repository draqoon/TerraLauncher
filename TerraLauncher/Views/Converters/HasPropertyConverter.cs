using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TerraLauncher.Models;

namespace TerraLauncher.Views.Converters {

	[ValueConversion( typeof( object ), typeof( bool ), ParameterType = typeof( string ) )]
	class HasPropertyConverter: IValueConverter {
		object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			if( value != null && parameter is string s && s != null ) {
				var propertyNames = s.Split( ',' )
										.Select( x => x?.Trim() )
										.Where( x => !string.IsNullOrWhiteSpace( x ) );

				foreach( var propertyName in propertyNames ) {
					var property = value.GetType().GetProperty( propertyName );
					if( property != null && property.CanRead )
						return true;
				}
			}

			return false;
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => throw new NotImplementedException();
	}
}
