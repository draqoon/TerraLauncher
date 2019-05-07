using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace TerraLauncher.Views.Converters {

	class AddCalcConverter: IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			if( parameter == null ) {
				throw new ArgumentNullException( nameof( parameter ) );
			}
			object addValue;
			try {
				addValue = System.Convert.ChangeType( parameter, value.GetType() );
			}
			catch( InvalidCastException ex ) {
				throw new ArgumentException( "パラメータのキャストに失敗しました。", nameof( parameter ), ex );
			}
			var result = (dynamic)value + (dynamic)addValue;
			return result;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			if( parameter == null ) {
				throw new ArgumentNullException( nameof( parameter ) );
			}
			object addValue;
			try {
				addValue = System.Convert.ChangeType( parameter, value.GetType() );
			}
			catch( InvalidCastException ex ) {
				throw new ArgumentException( "パラメータのキャストに失敗しました。", nameof( parameter ), ex );
			}
			var result = (dynamic)value - (dynamic)addValue;
			return result;
		}
	}

}
