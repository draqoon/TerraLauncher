using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reactive.Linq;
using System.Text;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using TerraLauncher.Models;
using TerraLauncher.Services;

namespace TerraLauncher.ViewModels {
	public class MainWindowViewModel: ViewModelBase {

		public ReactiveProperty<bool> CanCloseWindow { get; }

		public MainWindowViewModel( INavigatorService navigatorService, MainModel model ) : base( navigatorService ) {
			this.CompositeDisposable.Add( () => model.Save() );
			this.CanCloseWindow = this.NavigatorServie.ObserveProperty( m => m.CanGoBack ).Select( x => !x ).ToReactiveProperty();
		}

		public void OnClosing() {
			if( this.NavigatorServie.CanGoBack ) {
				this.NavigatorServie.GoBack();
			}
		}

	}
}
