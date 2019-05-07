using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using TerraLauncher.Models;
using TerraLauncher.Services;

namespace TerraLauncher.ViewModels {
	public abstract class ViewModelBase: ViewModel {

		protected INavigatorService NavigatorServie { get; }

		protected ViewModelBase( INavigatorService navigatorService ) {
			this.NavigatorServie = navigatorService;
		}

		public virtual void Initialize() {
		}

	}
}
