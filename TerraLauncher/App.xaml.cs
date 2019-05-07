using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Livet;
using TerraLauncher.Models;
using TerraLauncher.Services;
using TerraLauncher.ViewModels;
using TerraLauncher.Views;

namespace TerraLauncher {
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App: Application {

		[STAThread()]
		public static void Main( string[] args ) => new App().RunMain( args );

		private MainModel MainModel { get; set; }

		private void RunMain( string[] args ) {
			this.InitializeComponent();

			var configFile = Path.ChangeExtension( Assembly.GetEntryAssembly().Location, ".Config.json" );
			var config = ConfigModel.LoadFromFile( configFile );
			this.ChangeCulture( config.Culture );

			var dataFile = Path.ChangeExtension( Assembly.GetEntryAssembly().Location, ".Data.json" );
			var mainModel = MainModel.LoadFile( dataFile, config );
			this.MainModel = mainModel;

			var window = new MainWindow();
			var navigatorService = this.CreateNavigateService( window );
			window.DataContext = new MainWindowViewModel( navigatorService, mainModel );

			var mainPageViewModel = new MainPageViewModel( navigatorService, mainModel );
			navigatorService.Navigate( mainPageViewModel );

			this.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new SendOrPostCallback( ( w ) => ( (Window)w ).Show() ), window );

			this.Run();
		}

		private INavigatorService CreateNavigateService( NavigationWindow window ) {
			var navigator = new PageNavigatorService( () => window.NavigationService ); ;
			navigator.Register( typeof( MainPageViewModel ), typeof( MainPage ) );

			navigator.Register( typeof( EditListEnvironmentPageViewModel ), typeof( EditListPage ) );
			navigator.Register( typeof( EditListSaveDirectoryPageViewModel ), typeof( EditListPage ) );
			navigator.Register( typeof( EditListRunDatePageViewModel ), typeof( EditListPage ) );
			navigator.Register( typeof( EditListLanguagePageViewModel ), typeof( EditListPage ) );
			navigator.Register( typeof( EditListResolutionPageViewModel ), typeof( EditListPage ) );

			navigator.Register( typeof( EditEnvironmentViewModel ), typeof( EditDetailPage ) );
			navigator.Register( typeof( EditSaveDirectoryViewModel ), typeof( EditDetailPage ) );
			navigator.Register( typeof( EditRunDateViewModel ), typeof( EditDetailPage ) );
			navigator.Register( typeof( EditLanguageViewModel ), typeof( EditDetailPage ) );
			navigator.Register( typeof( EditResolutionViewModel ), typeof( EditDetailPage ) );

			return navigator;
		}

		public void ChangeCulture( CultureInfo culture ) {

			var cultureName = culture.Name;
			if( cultureName == "ja" )
				cultureName = "ja-JP";
			else if( cultureName != "ja-JP" )
				cultureName = "en-US";

			var dictionary = new ResourceDictionary() {
				Source = new Uri( $"/TerraLauncher;component/Resources/Languages/{cultureName}.xaml", UriKind.Relative )
			};
			this.Resources.MergedDictionaries[0] = dictionary;
		}

		protected override void OnStartup( StartupEventArgs e ) {
			DispatcherHelper.UIDispatcher = this.Dispatcher;
			AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
		}

		//集約エラーハンドラ
		private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e ) {
			//TODO:ロギング処理など

			MessageBox.Show(
				"不明なエラーが発生しました。アプリケーションを終了します。",
				"エラー",
				MessageBoxButton.OK,
				MessageBoxImage.Error );

			MessageBox.Show(
				e.ExceptionObject.ToString(),
				"エラー",
				MessageBoxButton.OK,
				MessageBoxImage.Error );

			Environment.Exit( 1 );
		}

	}
}
