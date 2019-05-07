using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using TerraLauncher.Models;
using TerraLauncher.Resources;
using TerraLauncher.Services;

namespace TerraLauncher.ViewModels {
    public class MainPageViewModel: ViewModelBase {

        private MainModel InnerModel { get; set; }

        public ReactiveProperty<ListModel<EnvironmentModel>> Environments { get; private set; }
        public ReactiveProperty<ListModel<SaveDirectoryModel>> SaveDirectories { get; private set; }
        public ReactiveProperty<ListModel<RunDateModel>> RunDates { get; private set; }
        public ReactiveProperty<ListModel<LanguageModel>> Languages { get; private set; }
        public ReactiveProperty<ListModel<ResolutionModel>> Resolutions { get; private set; }

        public bool RunDatesVisible => File.Exists( this.InnerModel.Config.RunAsDateExe );

        public ReadOnlyReactiveProperty<bool> CanRun { get; private set; }
        public ReadOnlyReactiveProperty<bool> Waiting { get; private set; }

        public ReadOnlyReactiveProperty<bool> Running { get; private set; }

        public ReactiveProperty<CultureInfo> AppLanguage { get; private set; }

        public MainPageViewModel( INavigatorService navigatorService, MainModel model ) : base( navigatorService ) {
            this.InnerModel = model;

            this.Environments = this.CreateReactiveProperty( this.InnerModel, m => m.Environments );
            this.SaveDirectories = this.CreateReactiveProperty( this.InnerModel, m => m.SaveDirectories );
            this.RunDates = this.CreateReactiveProperty( this.InnerModel, m => m.RunDates );
            this.Languages = this.CreateReactiveProperty( this.InnerModel, m => m.Languages );
            this.Resolutions = this.CreateReactiveProperty( this.InnerModel, m => m.Resolutions );

            this.CanRun = this.InnerModel
                                .ObserveProperty( m => m.CanRun )
                                .ToReadOnlyReactiveProperty();

            this.Waiting = this.InnerModel
                                .ObserveProperty( m => m.Waiting )
                                .ToReadOnlyReactiveProperty();

            this.Running = this.InnerModel
                                .ObserveProperty( m => m.Running )
                                .ToReadOnlyReactiveProperty();

            this.AppLanguage = this.InnerModel.Config.ToReactivePropertyAsSynchronized(
                    m => m.Culture,
                    ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
                    true
                );
            this.AppLanguage.Subscribe( culture => ( App.Current as App ).ChangeCulture( culture ) );

            this.InnerModel.Environments
                            .ObserveProperty( m => m.SelectedItem )
                            .Subscribe( m => {
                                this.InnerModel.SaveDirectories.Filter = item => item.ModLoader == m.ModLoader;
                                this.InnerModel.Languages.Filter = item => File.Exists( Path.Combine( m.InstallDirectory, item.Value ) );
                            } );
        }

        private ReactiveProperty<ListModel<V>> CreateReactiveProperty<M, V>( M model, Expression<Func<M, ListModel<V>>> propertySelector )
                                                                where M : INotifyPropertyChanged
                                                                where V : ItemModelBase {
            return model.ToReactivePropertyAsSynchronized(
                            propertySelector,
                            ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
                            true
                        )
                        .SetValidateNotifyError( value => {
                            var names = value.ValidateNotifyError();
                            var text = names.Aggregate( "", ( a, b ) => ( a == "" ? "" : a + Environment.NewLine ) + b );
                            return string.IsNullOrWhiteSpace( text ) ? null : text;
                        } );
        }

        public async void RunTerraria() {
            if( this.InnerModel == null )
                return;

            var isDebug = ( Keyboard.GetKeyStates( Key.LeftCtrl ) & KeyStates.Down ) == KeyStates.Down;

            var screen = Screen.FromWindow( App.Current.MainWindow );

            if( !isDebug ) {
                await Task
                        .Run( () => this.InnerModel.Run( screen ) )
                        .ContinueWith( t => DispatcherHelper.UIDispatcher.Invoke( this.QuitApp ) );
            }
            else {
                await Task
                        .Run( () => this.InnerModel.Run( screen, ( r, env, saveDirectory, date, lang, resolution, command, commandArgs ) => {
                            Thread.Sleep( 1000 );
                            var sb = new StringBuilder();
                            sb.AppendLine( $"Environment\t:\t{env?.DisplayTitle}" );
                            sb.AppendLine( $"SaveDirectory\t:\t{saveDirectory?.DisplayTitle}" );
                            sb.AppendLine( $"RunDate\t\t:\t{date?.DisplayTitle}" );
                            sb.AppendLine( $"Language\t\t:\t{lang?.DisplayTitle}" );
                            sb.AppendLine( $"Resolution\t:\t{resolution?.DisplayTitle}" );
                            sb.AppendLine( "---" );
                            sb.AppendLine( $"exe : {command}" );
                            sb.AppendLine( $"args : {commandArgs}" );

                            var message = new InformationMessage( sb.ToString(), "Debug", MessageBoxImage.Information, "MessageBox" );
                            this.Messenger.Raise( message );
                        } )
                    );
            }
        }

        public void QuitApp() =>
            App.Current.Shutdown( 0 );

        public void Edit( string parameter ) {

            ViewModel vm;
            if( parameter == nameof( this.Environments ) ) {
                var model = (ListModel<EnvironmentModel>)this.InnerModel.Environments.Clone();
                vm = new EditListEnvironmentPageViewModel( this.NavigatorServie, model, AppResource.Lang( "Environment" ), newModel => {
                    newModel.CopyTo( this.InnerModel.Environments );
                    this.RaisePropertyChanged( nameof( this.Environments ) );
                } );
            }
            else if( parameter == nameof( this.SaveDirectories ) ) {
                var model = (ListModel<SaveDirectoryModel>)this.InnerModel.SaveDirectories.Clone();
                vm = new EditListSaveDirectoryPageViewModel( this.NavigatorServie, model, AppResource.Lang( "SaveDirectory" ), newModel => {
                    newModel.CopyTo( this.InnerModel.SaveDirectories );
                    this.RaisePropertyChanged( nameof( this.SaveDirectories ) );
                } );
            }
            else if( parameter == nameof( this.RunDates ) ) {
                var model = (ListModel<RunDateModel>)this.InnerModel.RunDates.Clone();
                vm = new EditListRunDatePageViewModel( this.NavigatorServie, model, AppResource.Lang( "RunDate" ), newModel => {
                    newModel.CopyTo( this.InnerModel.RunDates );
                    this.RaisePropertyChanged( nameof( this.RunDates ) );
                } );
            }
            else if( parameter == nameof( this.Languages ) ) {
                var model = (ListModel<LanguageModel>)this.InnerModel.Languages.Clone();
                vm = new EditListLanguagePageViewModel( this.NavigatorServie, model, AppResource.Lang( "Language" ), newModel => {
                    newModel.CopyTo( this.InnerModel.Languages );
                    this.RaisePropertyChanged( nameof( this.Languages ) );
                } );
            }
            else if( parameter == nameof( this.Resolutions ) ) {
                var model = (ListModel<ResolutionModel>)this.InnerModel.Resolutions.Clone();
                vm = new EditListResolutionPageViewModel( this.NavigatorServie, model, AppResource.Lang( "Resolution" ), newModel => {
                    newModel.CopyTo( this.InnerModel.Resolutions );
                    this.RaisePropertyChanged( nameof( this.Resolutions ) );
                } );
            }
            else {
                return;
            }
            this.NavigatorServie.Navigate( vm );
        }

        private ListCollectionView GetListView( IEnumerable<ItemModelBase> items ) {
            var array = items.ToArray();
            var view = new ListCollectionView( array );

            if( items.Any( x => !string.IsNullOrWhiteSpace( x.Group ) ) ) {
                view.GroupDescriptions.Add( new PropertyGroupDescription( nameof( ItemModelBase.Group ) ) );
            }

            return view;
        }

    }

}
