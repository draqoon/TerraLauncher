using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;

using Livet;
using Livet.Messaging;
using Livet.Messaging.IO;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using TerraLauncher.Models;
using TerraLauncher.Resources;
using TerraLauncher.Services;

namespace TerraLauncher.ViewModels {

	#region EditDetailViewModelBase

	public abstract class EditDetailViewModelBase<M>: ViewModelBase where M : ItemModelBase, new() {

		private M InnerModel { get; set; }
		private Action<M> ApplyCallback { get; set; }

		public string PageTitle { get; set; }
		public ReactiveProperty<string> Group { get; private set; }

		public ReactiveProperty<string> Title { get; private set; }

		public ReadOnlyReactiveProperty<bool> CanApply {
			get {
				if( this._canApply == null ) {
					var validations = new List<IObservable<bool>>() { this.Group.ObserveHasErrors, this.Title.ObserveHasErrors };
					validations.AddRange( this.ApplyValidation );

					this._canApply = validations.CombineLatestValuesAreAllFalse().ToReadOnlyReactiveProperty();
				}
				return this._canApply;
			}
		}
		private ReadOnlyReactiveProperty<bool> _canApply = null;

		public ReadOnlyReactiveProperty<bool> CanRestore { get; private set; }

		public EditDetailViewModelBase( INavigatorService navigatorService, M model, string pageTitle, Action<M> applyCallback )
			: base( navigatorService ) {
			this.InnerModel = model;
			this.ApplyCallback = applyCallback;
			this.PageTitle = pageTitle;
			this.Group = this.CreateReactiveProperty( model, m => m.Group );
			this.Title = this.CreateReactiveProperty( model, m => m.Title );
			this.CanRestore = this.InnerModel.ObserveProperty( m => m.Locked, true ).ToReadOnlyReactiveProperty();
		}

		protected ReactiveProperty<T> CreateReactiveProperty<T>( M synchronizedModel, Expression<Func<M, T>> propertySelector ) {
			var propertyName = ( propertySelector.Body as MemberExpression )?.Member.Name;

			var rp = synchronizedModel.ToReactivePropertyAsSynchronized(
							propertySelector,
							ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
							true
						)
						.SetValidateNotifyError( value => {
							var names = synchronizedModel.ValidateNotifyError( propertyName, value );
							return ( names == null || !names.Any() ) ? null : names;
						} )
						.AddTo( this.CompositeDisposable );
			return rp;
		}

		protected virtual IEnumerable<IObservable<bool>> ApplyValidation => Enumerable.Empty<IObservable<bool>>();

		public void Apply() {
			this.ApplyCallback?.Invoke( this.InnerModel );
			this.NavigatorServie.GoBack();
		}

		public void Cancel() {
			this.NavigatorServie.GoBack();
		}

		public void Restore() {
			var message = new ConfirmationMessage( AppResource.Lang( "ConfirmRestore" ), AppResource.Lang( "Restore" ), MessageBoxImage.Question, MessageBoxButton.OKCancel, MessageBoxResult.Cancel, "Confirm;Restore" );
			this.Messenger.Raise( message );

			if( message.Response ?? false )
				this.InnerModel.Restore();
		}
	}

	#endregion

	#region EditEnvironmentViewModel

	public class EditEnvironmentViewModel: EditDetailViewModelBase<EnvironmentModel> {

		public ReactiveProperty<string> InstallDirectory { get; private set; }

		public ReactiveProperty<bool> ModLoader { get; private set; }

		public EditEnvironmentViewModel( INavigatorService navigatorService, EnvironmentModel model, string pageTitle, Action<EnvironmentModel> applyAction ) : base( navigatorService, model, pageTitle, applyAction ) {
			this.InstallDirectory = this.CreateReactiveProperty( model, m => m.InstallDirectory );
			this.ModLoader = this.CreateReactiveProperty( model, m => m.ModLoader );
		}

		protected override IEnumerable<IObservable<bool>> ApplyValidation => new[] { this.InstallDirectory.ObserveHasErrors, this.ModLoader.ObserveHasErrors };

		public void SetPath( FolderSelectionMessage m ) {
			if( !string.IsNullOrWhiteSpace( m.Response ) ) {
				this.InstallDirectory.Value = m.Response;
			}
		}
	}

	#endregion

	#region EditSaveDirectoryViewModel

	public class EditSaveDirectoryViewModel: EditDetailViewModelBase<SaveDirectoryModel> {

		public ReactiveProperty<string> SaveDirectory { get; private set; }

		public ReactiveProperty<bool> ModLoader { get; private set; }

		public EditSaveDirectoryViewModel( INavigatorService navigatorService, SaveDirectoryModel model, string pageTitle, Action<SaveDirectoryModel> applyAction ) : base( navigatorService, model, pageTitle, applyAction ) {
			this.SaveDirectory = this.CreateReactiveProperty( model, m => m.SaveDirectory );
			this.ModLoader = this.CreateReactiveProperty( model, m => m.ModLoader );
		}

		protected override IEnumerable<IObservable<bool>> ApplyValidation => new[] { this.SaveDirectory.ObserveHasErrors, this.ModLoader.ObserveHasErrors };

		public void SetSaveDirectory( FolderSelectionMessage m ) {
			if( !string.IsNullOrWhiteSpace( m.Response ) ) {
				this.SaveDirectory.Value = m.Response;
			}
		}

	}

	#endregion

	#region EditRunDateViewModel

	public class EditRunDateViewModel: EditDetailViewModelBase<RunDateModel> {

		public ReactiveProperty<string> Value { get; private set; }

		public EditRunDateViewModel( INavigatorService navigatorService, RunDateModel model, string pageTitle, Action<RunDateModel> applyAction ) : base( navigatorService, model, pageTitle, applyAction ) {
			this.Value = this.CreateReactiveProperty( model, m => m.Value );
		}

		protected override IEnumerable<IObservable<bool>> ApplyValidation => new[] { this.Value.ObserveHasErrors };
	}

	#endregion

	#region EditLanguageViewModel

	public class EditLanguageViewModel: EditDetailViewModelBase<LanguageModel> {

		public ReactiveProperty<string> Value { get; private set; }

		public EditLanguageViewModel( INavigatorService navigatorService, LanguageModel model, string pageTitle, Action<LanguageModel> applyAction ) : base( navigatorService, model, pageTitle, applyAction ) {
			this.Value = this.CreateReactiveProperty( model, m => m.Value );
		}

		protected override IEnumerable<IObservable<bool>> ApplyValidation => new[] { this.Value.ObserveHasErrors };
	}

	#endregion

	#region EditResolutionViewModel

	public class EditResolutionViewModel: EditDetailViewModelBase<ResolutionModel> {

		public ReactiveProperty<int?> Height { get; private set; }

		public ReactiveProperty<int?> Width { get; private set; }

		public ReactiveProperty<bool> Support4K { get; private set; }

		public EditResolutionViewModel( INavigatorService navigatorService, ResolutionModel model, string pageTitle, Action<ResolutionModel> applyAction ) : base( navigatorService, model, pageTitle, applyAction ) {
			this.Height = this.CreateReactiveProperty( model, m => m.Height );
			this.Width = this.CreateReactiveProperty( model, m => m.Width );
			this.Support4K = this.CreateReactiveProperty( model, m => m.Support4K );
		}

		protected override IEnumerable<IObservable<bool>> ApplyValidation => new[] { this.Width.ObserveHasErrors, this.Height.ObserveHasErrors };
	}

	#endregion

}
