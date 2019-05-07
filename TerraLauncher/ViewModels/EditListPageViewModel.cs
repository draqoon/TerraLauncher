using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using TerraLauncher.Models;
using TerraLauncher.Properties;
using TerraLauncher.Resources;
using TerraLauncher.Services;

namespace TerraLauncher.ViewModels {

	#region EditListPageViewModel

	public abstract class EditListPageViewModel<M, VM>: ViewModelBase
		where M : ItemModelBase, new()
		where VM : EditDetailViewModelBase<M> {

		#region properties

		private ListModel<M> InnerModel { get; set; }
		public string PageTitle { get; private set; }
		private Action<ListModel<M>> ApplyCallback { get; }

		public ReadOnlyReactiveCollection<M> Items { get; private set; }
		public ReactiveProperty<M> SelectedItem { get; }
		public ReadOnlyReactiveProperty<bool> CanDelete { get; }
		public ReadOnlyReactiveProperty<bool> CanMoveUp { get; }
		public ReadOnlyReactiveProperty<bool> CanMoveDown { get; }
		public ReadOnlyReactiveProperty<bool> IsSelected { get; }

		#endregion

		#region constructor

		protected EditListPageViewModel( INavigatorService navigatorService, ListModel<M> model, string pageTitle, Action<ListModel<M>> applyCallback )
			: base( navigatorService ) {
			this.InnerModel = model;
			this.PageTitle = pageTitle;
			this.ApplyCallback = applyCallback;

			this.Items = this.InnerModel.Items.ToReadOnlyReactiveCollection().AddTo( this.CompositeDisposable );
			this.SelectedItem = this.InnerModel.ToReactivePropertyAsSynchronized( m => m.SelectedItem ).AddTo( this.CompositeDisposable );
			this.CanDelete = this.InnerModel.ObserveProperty( m => m.CanDelete ).ToReadOnlyReactiveProperty().AddTo( this.CompositeDisposable );
			this.CanMoveUp = this.InnerModel.ObserveProperty( m => m.CanMoveUp ).ToReadOnlyReactiveProperty().AddTo( this.CompositeDisposable );
			this.CanMoveDown = this.InnerModel.ObserveProperty( m => m.CanMoveDown ).ToReadOnlyReactiveProperty().AddTo( this.CompositeDisposable );
			this.IsSelected = this.InnerModel.ObserveProperty( m => m.IsSelected ).ToReadOnlyReactiveProperty().AddTo( this.CompositeDisposable );
		}

		#endregion

		#region Command Methods

		public void AddItem() => this.AddItem( null );

		public void AddItem( M template ) {
			M model;
			if( template == null ) {
				model = new M();
			}
			else {
				model = (M)template.Clone( true );
			}
			var vm = (ViewModel)Activator.CreateInstance( typeof( VM ), this.NavigatorServie, model, this.PageTitle, new Action<M>( applyModel => {
				this.InnerModel.Items.Add( applyModel );
				this.InnerModel.SelectedItem = applyModel;
			} ) );
			this.NavigatorServie.Navigate( vm );
		}

		public void EditItem( M model ) {
			var modelWork = (M)model.Clone();
			var vm = (ViewModel)Activator.CreateInstance( typeof( VM ), this.NavigatorServie, modelWork, this.PageTitle, new Action<M>( applyModel => {
				applyModel.CopyTo( model );
			} ) );
			this.NavigatorServie.Navigate( vm );
		}

		public void DeleteItem( M model ) {
			var message = new ConfirmationMessage( AppResource.Lang( "ConfirmRemove" ), AppResource.Lang( "Remove" ), MessageBoxImage.Question, MessageBoxButton.OKCancel, MessageBoxResult.Cancel, "Confirm;RemoveItem" );
			this.Messenger.Raise( message );

			if( message.Response ?? false )
				this.InnerModel.DeleteItem( model );
		}

		public void MoveItem( bool down ) => this.InnerModel.MoveItem( down );

		public void Apply() {
			this.ApplyCallback?.Invoke( this.InnerModel );
			this.Cancel();
		}

		public void Cancel() {
			this.InnerModel.Dispose();
			this.NavigatorServie.GoBack();
		}

		public void ToggleItemVisible( M model ) {
			this.InnerModel.ToggleItemVisible( model );
		}

		#endregion

	}

	#endregion

	#region EditListEnvironmentPageViewModel

	public class EditListEnvironmentPageViewModel: EditListPageViewModel<EnvironmentModel, EditEnvironmentViewModel> {
		public EditListEnvironmentPageViewModel( INavigatorService navigatorService, ListModel<EnvironmentModel> model, string pageTitle, Action<ListModel<EnvironmentModel>> applyAction )
			: base( navigatorService, model, pageTitle, applyAction ) { }
	}

	#endregion

	#region EditListSaveDirectoryPageViewModel

	public class EditListSaveDirectoryPageViewModel: EditListPageViewModel<SaveDirectoryModel, EditSaveDirectoryViewModel> {
		public EditListSaveDirectoryPageViewModel( INavigatorService navigatorService, ListModel<SaveDirectoryModel> model, string pageTitle, Action<ListModel<SaveDirectoryModel>> applyAction )
			: base( navigatorService, model, pageTitle, applyAction ) { }
	}

	#endregion

	#region EditListRunDatePageViewModel

	public class EditListRunDatePageViewModel: EditListPageViewModel<RunDateModel, EditRunDateViewModel> {
		public EditListRunDatePageViewModel( INavigatorService navigatorService, ListModel<RunDateModel> model, string pageTitle, Action<ListModel<RunDateModel>> applyAction )
			: base( navigatorService, model, pageTitle, applyAction ) { }
	}

	#endregion

	#region EditListLanguagePageViewModel

	public class EditListLanguagePageViewModel: EditListPageViewModel<LanguageModel, EditLanguageViewModel> {
		public EditListLanguagePageViewModel( INavigatorService navigatorService, ListModel<LanguageModel> model, string pageTitle, Action<ListModel<LanguageModel>> applyAction )
			: base( navigatorService, model, pageTitle, applyAction ) { }
	}

	#endregion

	#region EditListResolutionPageViewModel

	public class EditListResolutionPageViewModel: EditListPageViewModel<ResolutionModel, EditResolutionViewModel> {
		public EditListResolutionPageViewModel( INavigatorService navigatorService, ListModel<ResolutionModel> model, string pageTitle, Action<ListModel<ResolutionModel>> applyAction )
			: base( navigatorService, model, pageTitle, applyAction ) { }
	}

	#endregion

}
