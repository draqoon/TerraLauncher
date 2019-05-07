using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraLauncher.Resources {
    class AppResource {
        public static T Get<T>( object resourceKey ) {
            if( App.Current.Resources.Contains( resourceKey ) ) {
                if( App.Current.Resources[resourceKey] is T value ) {
                    return value;
                }
            }
            return default( T );
        }

        public static string Lang( object resourceKey, params object[] args ) {
            var text = Get<string>( $"Lang:{resourceKey}" );

            if( 0 < ( args?.Length ?? 0 ) )
                text = string.Format( text, args );

            return text;
        }

    }
}
