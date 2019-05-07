using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Livet;
using System.Runtime.InteropServices;
using System.Windows;
using System.ComponentModel;

namespace TerraLauncher.Models {

    [JsonObject()]
    public class MainModel: NotificationObject {

        public MainModel() {
        }

        #region JsonProperty

        [JsonProperty( Order = 1 )]
        public ListModel<EnvironmentModel> Environments {
            get => this._environments;
            set {
                if( this.RaisePropertyChangedIfSet( ref this._environments, value ) )
                    this.RaisePropertyChanged( nameof( this.CanRun ) );
            }
        }
        private ListModel<EnvironmentModel> _environments = new ListModel<EnvironmentModel>( "Environment" );

        [JsonProperty( Order = 2 )]
        public ListModel<SaveDirectoryModel> SaveDirectories {
            get => this._saveDirectories;
            set {
                if( this.RaisePropertyChangedIfSet( ref this._saveDirectories, value ) )
                    this.RaisePropertyChanged( nameof( this.CanRun ) );
            }
        }
        private ListModel<SaveDirectoryModel> _saveDirectories = new ListModel<SaveDirectoryModel>( "SaveDirectory" );

        [JsonProperty( Order = 3 )]
        public ListModel<RunDateModel> RunDates {
            get => this._runDates;
            set {
                if( this.RaisePropertyChangedIfSet( ref this._runDates, value ) )
                    this.RaisePropertyChanged( nameof( this.CanRun ) );
            }
        }
        private ListModel<RunDateModel> _runDates = new ListModel<RunDateModel>( "RunDate" );

        [JsonProperty( Order = 4 )]
        public ListModel<LanguageModel> Languages {
            get => this._languages;
            set {
                if( this.RaisePropertyChangedIfSet( ref this._languages, value ) )
                    this.RaisePropertyChanged( nameof( this.CanRun ) );
            }
        }
        private ListModel<LanguageModel> _languages = new ListModel<LanguageModel>( "Language" );

        [JsonProperty( Order = 5 )]
        public ListModel<ResolutionModel> Resolutions {
            get => this._resolutions;
            set {
                if( this.RaisePropertyChangedIfSet( ref this._resolutions, value ) )
                    this.RaisePropertyChanged( nameof( this.CanRun ) );
            }
        }
        private ListModel<ResolutionModel> _resolutions = new ListModel<ResolutionModel>( "Resolution" );

        #endregion

        #region JsonIgnore Property

        [JsonIgnore]
        public bool CanRun {
            get {
                if( !this.Environments.IsValid )
                    return false;
                if( !this.SaveDirectories.IsValid )
                    return false;
                if( File.Exists( this.Config.RunAsDateExe ) && !this.RunDates.IsValid )
                    return false;
                if( !this.Languages.IsValid )
                    return false;
                if( !File.Exists( Path.Combine( this.Environments.SelectedItem.InstallDirectory, this.Languages.SelectedItem.Value ) ) )
                    return false;
                if( !this.Resolutions.IsValid )
                    return false;
                return true;
            }
        }

        [JsonIgnore()]
        public bool Waiting {
            get => this._waiting;
            private set => this.RaisePropertyChangedIfSet( ref this._waiting, value );
        }
        private bool _waiting = false;

        [JsonIgnore()]
        public bool Running {
            get => this._running;
            private set => this.RaisePropertyChangedIfSet( ref this._running, value );
        }
        private bool _running = false;

        [JsonIgnore]
        public string ConfigFile { get; private set; }

        [JsonIgnore]
        public ConfigModel Config { get; private set; }

        #endregion

        public static MainModel LoadFile( string configFile, ConfigModel config ) {
            MainModel model;
            try {
                string json;
                using( var reader = new StreamReader( configFile, Encoding.UTF8 ) ) {
                    json = reader.ReadToEnd();
                }
                model = JsonConvert.DeserializeObject<MainModel>( json );
            }
            catch {
                model = new MainModel();
            }
            model.ConfigFile = configFile;
            model.Config = config;

            model.Environments.Initialize( EnvironmentModel.InitializeList );
            model.SaveDirectories.Initialize( SaveDirectoryModel.InitializeList );
            model.RunDates.Initialize( RunDateModel.InitializeList );
            model.Languages.Initialize( LanguageModel.InitializeList );
            model.Resolutions.Initialize( ResolutionModel.InitializeList );

            model.BeginWatchAsync();

            return model;
        }

        public async void BeginWatchAsync() {
            await Task.Run( () => {
                while( true ) {
                    this.Running = !this.Waiting && this.GetIsRunning();
                    System.Threading.Thread.Sleep( 1000 );
                }
            } );
        }

        private bool GetIsRunning() {
            var processNames = this.Languages.Items.Select( x => Path.GetFileNameWithoutExtension( x.Value ) ).ToArray();
            return processNames.Any( processName => 0 < Process.GetProcessesByName( processName ).Length );
        }

        public void Save() {
            var json = JsonConvert.SerializeObject( this, Formatting.Indented );
            using( var writer = new StreamWriter( this.ConfigFile, false, Encoding.UTF8 ) ) {
                writer.Write( json );
            }
        }

        #region Win32

        [DllImport( "user32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool IsWindow( IntPtr hWnd );

        [DllImport( "user32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool IsWindowVisible( IntPtr hWnd );

        [DllImport( "user32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool IsWindowEnabled( IntPtr hWnd );

        [StructLayout( LayoutKind.Sequential )]
        private struct RECT {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport( "user32.dll", SetLastError = true )]
        private static extern bool GetWindowRect( IntPtr hwnd, out RECT lpRect );

        [DllImport( "user32.dll", SetLastError = true )]
        private static extern bool MoveWindow( IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint );

        #endregion

        public void Run( Screen screen ) => this.Run( screen, ( _screen, env, saveDirectory, date, lang, resolution, command, commandArgs ) => {

            if( this.GetIsRunning() ) {
                this.Running = true;
                return;
            }

            this.PrepareForRun( saveDirectory );

            Process.Start( command, commandArgs ).WaitForInputIdle();

            var processes = Process.GetProcessesByName( Path.GetFileNameWithoutExtension( lang.Value ) );

            var p = processes[0];
            while( true ) {
                if( p.MainWindowHandle != IntPtr.Zero && IsWindow( p.MainWindowHandle ) && IsWindowVisible( p.MainWindowHandle ) && IsWindowEnabled( p.MainWindowHandle ) ) {
                    if( !_screen.Primary ) {
                        System.Threading.Thread.Sleep( 200 );
                        GetWindowRect( p.MainWindowHandle, out var r );
                        var area = _screen.WorkingArea;
                        var x = area.X + ( area.Width - ( r.Right - r.Left ) ) / 2;
                        var y = area.Y + ( area.Height - ( r.Bottom - r.Top ) ) / 2;
                        MoveWindow( p.MainWindowHandle, (int)x, (int)y, r.Right - r.Left, r.Bottom - r.Top, true );
                    }
                    break;
                }
            }
        } );

        public void Run( Screen screen, Action<Screen, EnvironmentModel, SaveDirectoryModel, RunDateModel, LanguageModel, ResolutionModel, string, string> action ) {
            this.Save();
            this.UpdateTerrariaConfig();

            var env = this.Environments.SelectedItem;

            var terrariaDir = env.InstallDirectory;
            var terrariaExe = !this.Languages.IsSelected ? "Terraria.exe" : this.Languages.SelectedItem.Value;

            var terrariaPath = "\"" + Path.Combine( terrariaDir, terrariaExe ) + "\"";

            var terrariaArgs = "";
            var saveDirectory = Environment.ExpandEnvironmentVariables( this.SaveDirectories.SelectedItem.SaveDirectory );
            if( env.ModLoader ) {
                terrariaArgs = $"-tmlsavedirectory \"{saveDirectory}\"";
            }
            else {
                terrariaArgs = $"-savedirectory \"{saveDirectory}\"";
            }

            var date = DateTime.Now;
            if( this.RunDates.IsSelected && !string.IsNullOrWhiteSpace( this.RunDates.SelectedItem.Value ) ) {
                if( DateTime.TryParse( DateTime.Now.ToString( this.RunDates.SelectedItem.Value ), out var d ) ) {
                    date = d;
                }
            }
            var runAsDateParam = date.ToString( this.Config.RunAsDateParamFormat );
            var command = this.Config.RunAsDateExe;
            var commandArgs = $"{runAsDateParam} {terrariaPath} {terrariaArgs}";

            this.Waiting = true;
            try {
                action( screen, env, this.SaveDirectories.SelectedItem, this.RunDates.SelectedItem, this.Languages.SelectedItem, this.Resolutions.SelectedItem, command, commandArgs );
            }
            finally {
                this.Waiting = false;
            }
        }

        private void PrepareForRun( SaveDirectoryModel env ) {
            var saveDirectory = Environment.ExpandEnvironmentVariables( env.SaveDirectory );

            if( !Directory.Exists( saveDirectory ) )
                Directory.CreateDirectory( saveDirectory );
        }

        private void UpdateTerrariaConfig() {
            var path = Path.Combine( this.SaveDirectories.SelectedItem.SaveDirectory, "config.json" );
            path = Environment.ExpandEnvironmentVariables( path );

            JObject config;
            if( File.Exists( path ) ) {
                var json = "";
                using( var reader = new StreamReader( path ) ) {
                    json = reader.ReadToEnd();
                }
                config = JObject.Parse( json );
            }
            else {
                config = new JObject();
            }

            if( this.Resolutions.IsSelected ) {
                var resolution = this.Resolutions.SelectedItem;

                config["DisplayWidth"] = resolution.Width;
                config["DisplayHeight"] = resolution.Height;
                config["Support4K"] = resolution.Support4K;
                config["Fullscreen"] = false;
                config["WindowMaximized"] = false;
                config["WindowBorderless"] = false;
            }

            using( var writer = new StreamWriter( path ) ) {
                writer.Write( config );
            }
        }
    }
}