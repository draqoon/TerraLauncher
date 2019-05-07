using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace TerraLauncher.Views.Converters {

	[ContentProperty( nameof( Converters ) )]
	public class ConverterPipe: IValueConverter {

		public Collection<IValueConverter> Converters { get; } = new Collection<IValueConverter>();

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) =>
			this.Converters.Aggregate( value, ( v, c ) => c.Convert( v, targetType, parameter, culture ) );

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) =>
			this.Converters.Reverse().Aggregate( value, ( v, c ) => c.ConvertBack( v, targetType, parameter, culture ) );

	}

}
