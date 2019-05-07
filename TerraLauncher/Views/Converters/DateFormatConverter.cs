using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TerraLauncher.Views.Converters {

	[ValueConversion( typeof( string ), typeof( string ) )]
	class DateFormatConverter: IValueConverter {
		object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			if( !( value is string ) )
				return value;

			var formatText = (string)value;
			var text = DateTime.Now.ToString( formatText );

			return text;
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => throw new NotImplementedException();
	}
}
