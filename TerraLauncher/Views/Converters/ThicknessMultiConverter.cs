using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TerraLauncher.Views.Converters {

	[ValueConversion( typeof( double ), typeof( Thickness ) )]
	class ThicknessMultiConverter: IMultiValueConverter {

		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {

			var array = ( values?.Select( x => (double)x ) ?? Enumerable.Empty<double>() ).ToArray();

			var thicknes = new Thickness();
			if( array.Length == 1 ) {
				thicknes.Left = array[0];
				thicknes.Top = array[0];
				thicknes.Right = array[0];
				thicknes.Bottom = array[0];
			}
			else if( array.Length == 2 ) {
				thicknes.Left = array[0];
				thicknes.Top = array[1];
				thicknes.Right = array[0];
				thicknes.Bottom = array[1];
			}
			else if( array.Length == 4 ) {
				thicknes.Left = array[0];
				thicknes.Top = array[1];
				thicknes.Right = array[2];
				thicknes.Bottom = array[3];
			}

			return thicknes;
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
			if( value is Thickness thickness ) {
				return new object[] { thickness.Left, thickness.Top, thickness.Right, thickness.Bottom };
			}
			return new object[] { 0.0, 0.0, 0.0, 0.0 };
		}

	}
}
