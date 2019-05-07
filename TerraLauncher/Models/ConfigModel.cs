using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;

using Livet;

using Newtonsoft.Json;

namespace TerraLauncher.Models {

	[JsonObject()]
	public class ConfigModel: NotificationObject {

		#region JsonProperty

		[JsonProperty( Order = 1, DefaultValueHandling = DefaultValueHandling.Ignore )]
		[DefaultValue( true )]
		public bool CultureDetect {
			get => this._cultureDetect;
			set => this.RaisePropertyChangedIfSet( ref this._cultureDetect, value );
		}
		private bool _cultureDetect = true;

		[JsonProperty( PropertyName = "Culture", Order = 2 )]
		public string CultureName {
			get => this.CultureDetect ? "" : this.Culture?.Name;
			set => this.Culture = string.IsNullOrWhiteSpace( value ) ? null : CultureInfo.GetCultureInfo( value );
		}

		[JsonProperty( Order = 3 )]
		public string RunAsDateExe {
			get => this._runAsDateExe;
			set => this.RaisePropertyChangedIfSet( ref this._runAsDateExe, value );
		}
		private string _runAsDateExe = "";

		[JsonProperty( Order = 4 )]
		public string RunAsDateParamFormat {
			get => this._runAsDateParam;
			set => this.RaisePropertyChangedIfSet( ref this._runAsDateParam, value );
		}
		private string _runAsDateParam = "";

		#endregion

		#region JsonIgnore Property

		[JsonIgnore]
		public CultureInfo Culture {
			get => this._culture;
			set => this.RaisePropertyChangedIfSet( ref this._culture, value );
		}
		private CultureInfo _culture = null;

		[JsonIgnore]
		public string ConfigFile { get; private set; }

		#endregion

		public ConfigModel() { }

		public static ConfigModel LoadFromFile( string configFile ) {
			ConfigModel model;
			try {
				string json;
				using( var reader = new StreamReader( configFile, Encoding.UTF8 ) ) {
					json = reader.ReadToEnd();
				}
				model = JsonConvert.DeserializeObject<ConfigModel>( json );
			}
			catch {
				model = new ConfigModel();
			}

			if( string.IsNullOrWhiteSpace( model.RunAsDateParamFormat ) ) {
				model.RunAsDateParamFormat = "/immediate /movetime";
			}

			if( model.CultureDetect || model.Culture == null ) {
				model.Culture = CultureInfo.CurrentUICulture;
			}

			model.ConfigFile = configFile;
			return model;
		}

	}
}
