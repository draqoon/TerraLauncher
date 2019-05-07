using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;

using Livet;

using Newtonsoft.Json;

using TerraLauncher.Resources;

namespace TerraLauncher.Models {

	#region ItemModelBase

	public abstract class ItemModelBase: NotificationObject, ICloneable {

		[JsonProperty( Order = 1 )]
		public string ID {
			get => this._uuid;
			set => this.RaisePropertyChangedIfSet( ref this._uuid, value );
		}
		private string _uuid = Guid.NewGuid().ToString();

		[JsonProperty( Order = 2, DefaultValueHandling = DefaultValueHandling.Ignore )]
		[DefaultValue( "" )]
		public string Group {
			get => this._group;
			set => this.RaisePropertyChangedIfSet( ref this._group, value );
		}
		private string _group = "";

		[JsonProperty( Order = 3 )]
		public string Title {
			get => this._title;
			set {
				if( this.RaisePropertyChangedIfSet( ref this._title, value ) ) {
					this.RaisePropertyChanged( nameof( this.DisplayTitle ) );
				}
			}
		}
		private string _title = "";

		[JsonProperty( Order = 99, DefaultValueHandling = DefaultValueHandling.Ignore )]
		[DefaultValue( false )]
		public bool Hidden {
			get => this._hidden;
			set => this.RaisePropertyChangedIfSet( ref this._hidden, value );
		}
		private bool _hidden = false;

		[JsonIgnore()]
		public virtual string DisplayTitle => this.Title;

		[JsonIgnore()]
		public bool Locked { get; set; }

		[JsonIgnore()]
		public ItemModelBase Backup { get; set; }

		public override string ToString() => this.Title;

		#region ICloneable

		/// <summary>
		/// オブジェクトの複製を作成します。
		/// </summary>
		/// <returns>複製したオブジェクト</returns>
		public virtual object Clone() {
			var type = this.GetType();

			var other = Activator.CreateInstance( type, true );

			foreach( var p in this.GetType().GetProperties().Where( p => p.CanWrite && p.CanRead ) ) {
				var value = p.GetValue( this );
				p.SetValue( other, value );
			}

			return other;
		}

		public ItemModelBase Clone( bool newUUID ) {
			var newObject = (ItemModelBase)this.Clone();
			if( newUUID ) {
				newObject.ID = Guid.NewGuid().ToString();
				newObject.Locked = false;
				newObject.Backup = null;
			}
			return newObject;
		}

		#endregion

		#region CopyTo

		/// <summary>
		/// オブジェクトのプロパティ値をコピーします。
		/// </summary>
		/// <param name="other">コピー先のインスタンス</param>
		public virtual void CopyTo( ItemModelBase other ) {
			if( other == null )
				throw new ArgumentNullException( nameof( other ) );

			var type = this.GetType();
			var otherType = other.GetType();
			if( type != otherType && !otherType.IsSubclassOf( type ) )
				throw new ArgumentException( $"{nameof( other )}には{type}または{type}のサブクラスのインスタンスを指定してください。", nameof( other ) );   //todo:例外メッセージをリソース化

			foreach( var p in this.GetType().GetProperties().Where( p => p.CanWrite && p.CanRead ) ) {
				var value = p.GetValue( this );
				p.SetValue( other, value );
			}
		}

		#endregion

		public virtual IEnumerable<string> ValidateNotifyError( string propertyName, object value = null ) {
			if( propertyName == nameof( this.Title ) ) {
				var text = value as string ?? this.Title;
				if( string.IsNullOrWhiteSpace( text ) )
					yield return AppResource.Lang( "Require:" + propertyName );
			}
		}

		public void Restore() {
			if( this.Locked && this.Backup != null ) {
				var type = this.GetType();

				foreach( var p in this.GetType().GetProperties().Where( p => p.CanWrite && p.CanRead ) ) {
					var value = p.GetValue( this.Backup );
					p.SetValue( this, value );
				}

			}
		}

	}

	#endregion

	#region EnvironmentModel

	public class EnvironmentModel: ItemModelBase {

		public static IEnumerable<EnvironmentModel> InitializeList => new EnvironmentModel[] {
					new EnvironmentModel() {
						ID = "06cc2387-917c-40f5-8875-aafdb2031d03",
						Group = "Vanilla",
						Title = "Vanilla",
						InstallDirectory = Environment.Is64BitOperatingSystem ?
									@"C:\Program Files (x86)\Steam\steamapps\common\Terraria" :
									@"C:\Program Files\Steam\steamapps\common\Terraria",
						Locked = true
					}
		};

		[JsonProperty( Order = 11 )]
		public string InstallDirectory {
			get => this._installDirectory;
			set => this.RaisePropertyChangedIfSet( ref this._installDirectory, value );
		}
		private string _installDirectory = "";

		[JsonProperty( Order = 12, DefaultValueHandling = DefaultValueHandling.Ignore )]
		[DefaultValue( false )]
		public bool ModLoader {
			get => this._ModLoader;
			set => this.RaisePropertyChangedIfSet( ref this._ModLoader, value );
		}
		private bool _ModLoader = false;

		public override IEnumerable<string> ValidateNotifyError( string propertyName, object value = null ) {
			var baseErrors = base.ValidateNotifyError( propertyName, value );
			if( baseErrors != null )
				foreach( var error in baseErrors )
					yield return error;

			if( propertyName == nameof( this.InstallDirectory ) ) {
				var text = value as string ?? this.InstallDirectory;
				if( string.IsNullOrWhiteSpace( text ) )
					yield return AppResource.Lang( "Require:" + propertyName );
				else if( !Directory.Exists( text ) )
					yield return AppResource.Lang( "NotFound:" + propertyName );
			}
		}

	}

	#endregion

	#region SaveDirectoryModel

	public class SaveDirectoryModel: ItemModelBase {

		public static IEnumerable<SaveDirectoryModel> InitializeList => new SaveDirectoryModel[] {
					new SaveDirectoryModel() {
						ID = "73eb5ea8-ce2f-4cbf-96ac-c88a387a1550",
						Title = "Vanilla",
						SaveDirectory = @"%USERPROFILE%\Documents\My Games\Terraria",
						Locked = true
					}
		};

		[JsonProperty( Order = 11 )]
		public string SaveDirectory {
			get => this._SaveDirectory;
			set => this.RaisePropertyChangedIfSet( ref this._SaveDirectory, value );
		}
		private string _SaveDirectory = "";

		[JsonProperty( Order = 12, DefaultValueHandling = DefaultValueHandling.Ignore )]
		[DefaultValue( false )]
		public bool ModLoader {
			get => this._ModLoader;
			set => this.RaisePropertyChangedIfSet( ref this._ModLoader, value );
		}
		private bool _ModLoader = false;

		public override IEnumerable<string> ValidateNotifyError( string propertyName, object value = null ) {
			var baseErrors = base.ValidateNotifyError( propertyName, value );
			if( baseErrors != null )
				foreach( var error in baseErrors )
					yield return error;

			if( propertyName == nameof( this.SaveDirectory ) ) {
				var text = value as string ?? this.SaveDirectory;
				if( string.IsNullOrWhiteSpace( text ) )
					yield return AppResource.Lang( "Require:" + propertyName );
				else {
					text = Environment.ExpandEnvironmentVariables( text );
					if( !Directory.Exists( text ) )
						yield return AppResource.Lang( "NotFound:" + propertyName );
				}
			}
		}

	}

	#endregion

	#region RunDateModel

	public class RunDateModel: ItemModelBase {

		public static IEnumerable<RunDateModel> InitializeList => new RunDateModel[] {
					new RunDateModel() {
						ID = "386a9666-4322-494b-83c4-88b39f60166d",
						Title = AppResource.Lang( "Realtime" ),
						Locked = true
					},
					new RunDateModel() {
						ID = "506b0a80-b51b-4f4b-b87c-ad217534161e",
						Title = AppResource.Lang( "Halloween" ),
						Value = @"yyyy-10-30",
						Locked = true
					},
					new RunDateModel() {
						ID = "27a76917-62a1-4f49-83b8-80264559a6aa",
						Title = AppResource.Lang( "Christmas" ),
						Value = @"yyyy-12-25",
						Locked = true
					}
		};

		[JsonProperty( Order = 11 )]
		public string Value {
			get => this._Value;
			set => this.RaisePropertyChangedIfSet( ref this._Value, value );
		}
		private string _Value = "";

		[JsonIgnore()]
		public override string DisplayTitle => string.Format( CultureInfo.CurrentCulture, this.Title, DateTime.Now );

		public override IEnumerable<string> ValidateNotifyError( string propertyName, object value = null ) {
			var baseErrors = base.ValidateNotifyError( propertyName, value );
			if( baseErrors != null )
				foreach( var error in baseErrors )
					yield return error;

			if( propertyName == nameof( this.Value ) ) {
				var text = value as string ?? this.Value;
				if( !string.IsNullOrWhiteSpace( text ) ) {
					var isError = false;
					try {
						var date = DateTime.Now.ToString( text );
						isError = !DateTime.TryParse( date, out var _ );
					}
					catch( FormatException ) {
						isError = true;
					}
					if( isError )
						yield return AppResource.Lang( "Invalid:Date" );
				}
			}
		}

	}

	#endregion

	#region LanguageModel

	public class LanguageModel: ItemModelBase {

		public static IEnumerable<LanguageModel> InitializeList => new LanguageModel[] {
					new LanguageModel() {
						ID = "5d8fd542-81e4-48de-a5aa-08586d8b8a8a",
						Title = AppResource.Lang( "English" ),
						Value = "Terraria.exe",
						Locked = true
					},
					new LanguageModel() {
						ID = "631214cb-cf52-4836-8b28-c225551d8fc2",
						Title = AppResource.Lang( "Japanese" ),
						Value = "Terraria_jp.exe",
						Locked = true
					}
		};

		[JsonProperty( Order = 11 )]
		public string Value {
			get => this._Value;
			set => this.RaisePropertyChangedIfSet( ref this._Value, value );
		}
		private string _Value = "";

		public override IEnumerable<string> ValidateNotifyError( string propertyName, object value = null ) {
			var baseErrors = base.ValidateNotifyError( propertyName, value );
			if( baseErrors != null )
				foreach( var error in baseErrors )
					yield return error;

			if( propertyName == nameof( this.Value ) ) {
				var text = value as string ?? this.Value;
				if( string.IsNullOrWhiteSpace( text ) )
					yield return AppResource.Lang( "Require:" + propertyName );
			}
		}

	}

	#endregion

	#region ResolutionModel

	public class ResolutionModel: ItemModelBase {

		public static IEnumerable<ResolutionModel> InitializeList => new ResolutionModel[] {
					new ResolutionModel() {
						ID = "99bc9032-6e15-49a7-abbf-3fe0130dc924",
						Title = "XGA (1024x768)",
						Width = 1024,
						Height = 768,
						Support4K = false,
						Locked = true
					}
		};

		[JsonProperty( Order = 11 )]
		public int? Width {
			get => this._Width;
			set => this.RaisePropertyChangedIfSet( ref this._Width, value );
		}
		private int? _Width = null;

		[JsonProperty( Order = 12 )]
		public int? Height {
			get => this._Height;
			set => this.RaisePropertyChangedIfSet( ref this._Height, value );
		}
		private int? _Height = null;

		[JsonProperty( Order = 13, DefaultValueHandling = DefaultValueHandling.Ignore )]
		[DefaultValue( false )]
		public bool Support4K {
			get => this._support4K;
			set => this.RaisePropertyChangedIfSet( ref this._support4K, value );
		}
		private bool _support4K = false;

		[JsonIgnore()]
		public override string DisplayTitle => string.Format( this.Title, this.Width, this.Height, this.Support4K );

		public override IEnumerable<string> ValidateNotifyError( string propertyName, object value = null ) {
			var baseErrors = base.ValidateNotifyError( propertyName, value );
			if( baseErrors != null )
				foreach( var error in baseErrors )
					yield return error;

			if( propertyName == nameof( this.Width ) ) {
				var text = value as string ?? ( this.Width.HasValue ? this.Width.Value.ToString() : "" );
				if( !int.TryParse( text, out var num ) || num < 1024 ) {
					yield return AppResource.Lang( "Require:" + propertyName );
				}
			}
			else if( propertyName == nameof( this.Height ) ) {
				var text = value as string ?? ( this.Height.HasValue ? this.Height.Value.ToString() : "" );
				if( !int.TryParse( text, out var num ) || num < 768 ) {
					yield return AppResource.Lang( "Require:" + propertyName );
				}
			}
		}

	}

	#endregion

}
