using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Livet;

namespace TerraLauncher.Services {

	#region INavigatorService

	public interface INavigatorService: INotifyPropertyChanged {
		/// <summary>
		/// ViewModel を指定して遷移します。
		/// </summary>
		/// <param name="viewModel">遷移する対象の ViewModel インスタンス。</param>
		/// <returns>ナビゲーションがキャンセルされない場合は true、それ以外の場合は false を返します。</returns>
		bool Navigate( ViewModel viewModel );

		/// <summary>
		/// ナビゲーション履歴の後方に、少なくとも 1 つ以上のエントリがあるかどうかを示す値を取得します。
		/// </summary>
		bool CanGoBack { get; }

		/// <summary>
		/// ナビゲーション履歴の前方に、少なくとも 1 つ以上のエントリがあるかどうかを示す値を取得します。
		/// </summary>
		bool CanGoForward { get; }

		/// <summary>
		/// ナビゲーション履歴が存在する場合、直前のエントリに移動します。
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		void GoBack();

		/// <summary>
		/// ナビゲーション履歴が存在する場合、直後のエントリに移動します。
		/// </summary>
		void GoForward();
	}

	#endregion

	#region NavigatorService<T>

	public abstract class NavigatorService<T>: INavigatorService where T : FrameworkElement {

		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropertyChanged( [CallerMemberName] string propertyName = "" )
			=> this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

		private Dictionary<Type, Type> NavigateToDictionary { get; } = new Dictionary<Type, Type>();

		/// <summary>
		/// ナビゲーション履歴の後方に、少なくとも 1 つ以上のエントリがあるかどうかを示す値を取得します。
		/// </summary>
		public virtual bool CanGoBack => false;

		/// <summary>
		/// ナビゲーション履歴の前方に、少なくとも 1 つ以上のエントリがあるかどうかを示す値を取得します。
		/// </summary>
		public virtual bool CanGoForward => false;

		/// <summary>
		/// ViewModel と対応する 遷移先 の型のセットを登録します。
		/// </summary>
		/// <param name="viewModelType">ViewModel の Type</param>
		/// <param name="navigateToType">遷移先 の Type</param>
		/// <exception cref="ArgumentException">ViewModel,Page 以外の Type を指定した場合に発生します。</exception>
		public void Register( Type viewModelType, Type navigateToType ) {

			if( viewModelType == null || !viewModelType.IsSubclassOf( typeof( ViewModel ) ) )
				throw new ArgumentException( $"Please set type of ViewModel to {nameof( viewModelType )}." );   //todo:例外メッセージをリソース化

			if( navigateToType == null || !navigateToType.IsSubclassOf( typeof( T ) ) || navigateToType.IsAbstract )
				throw new ArgumentException( $"Please set type of {nameof( T )} to {nameof( navigateToType )}." );  //todo:例外メッセージをリソース化

			if( !this.NavigateToDictionary.ContainsKey( viewModelType ) ) {
				this.NavigateToDictionary.Add( viewModelType, navigateToType );
			}
			else {
				this.NavigateToDictionary[viewModelType] = navigateToType;
			}
		}

		/// <summary>
		/// ViewModel を指定してページ遷移します。
		/// </summary>
		/// <param name="viewModel">ページ遷移する対象の ViewModel インスタンス。</param>
		/// <returns>ナビゲーションがキャンセルされない場合は true、それ以外の場合は false を返します。</returns>
		/// <exception cref="ArgumentException">未登録の ViewModel を指定した場合に発生します。</exception>
		public bool Navigate( ViewModel viewModel ) {
			var navigateToInstance = this.CreatePage( viewModel );
			if( navigateToInstance == null )
				throw new ArgumentException( $"{viewModel.GetType()} is not registered. Please register to {nameof( NavigatorService<T> )}.", nameof( viewModel ) );   //todo:例外メッセージをリソース化

			return this.OnNavigate( navigateToInstance );
		}

		protected abstract bool OnNavigate( T navigateToInstance );

		/// <summary>
		/// ViewModel を指定して、対応する T インスタンスを作成します。
		/// </summary>
		/// <param name="vm">ViewModel</param>
		/// <returns>作成された T インスタンス</returns>
		private T CreatePage( ViewModel vm ) {
			if( this.NavigateToDictionary.TryGetValue( vm.GetType(), out var type ) ) {
				var navigateToInstance = (T)Activator.CreateInstance( type );
				navigateToInstance.DataContext = vm;
				return navigateToInstance;
			}
			return null;
		}

		/// <summary>
		/// ナビゲーション履歴が存在する場合、直前のエントリに移動します。
		/// </summary>
		public void GoBack() {
			if( this.CanGoBack )
				this.OnGoBack();
		}
		protected virtual void OnGoBack() { }

		/// <summary>
		/// ナビゲーション履歴が存在する場合、直後のエントリに移動します。
		/// </summary>
		public void GoForward() {
			if( this.CanGoForward )
				this.GoForward();
		}
		protected virtual void OnGoForward() { }
	}

	#endregion

	#region PageNavigatorService

	/// <summary>
	/// ViewModel と Page のセットを管理し、ページ遷移を行います。（要 Livet）
	/// </summary>
	public class PageNavigatorService: NavigatorService<Page> {

		private NavigationService NavigationService => this._navigationService ?? ( this._navigationService = this.GetNavigationService?.Invoke() );
		private NavigationService _navigationService = null;
		private Func<NavigationService> GetNavigationService;

		/// <summary>
		/// ページ遷移を操作する NavigationService を指定して、インスタンスを初期化します。
		/// </summary>
		/// <param name="navigationService">ページ遷移を操作する NavigationService</param>
		public PageNavigatorService( NavigationService navigationService ) {
			this._navigationService = navigationService;
			this._navigationService.Navigated += this.OnNavigated;
		}

		/// <summary>
		/// ページ遷移を操作する NavigationService を取得するデリゲートを指定して、インスタンスを初期化します。
		/// </summary>
		/// <param name="getNavigationService">ページ遷移を操作する NavigationService を取得するデリゲートを</param>
		public PageNavigatorService( Func<NavigationService> getNavigationService ) {
			this.GetNavigationService = () => {
				var nav = getNavigationService();
				nav.Navigated += this.OnNavigated;
				return nav;
			};
		}

		private void OnNavigated( object sender, NavigationEventArgs e ) {
			this.RaisePropertyChanged( nameof( this.CanGoBack ) );
			this.RaisePropertyChanged( nameof( this.CanGoForward ) );
		}

		/// <returns>ナビゲーションがキャンセルされない場合は true、それ以外の場合は false を返します。</returns>
		/// <exception cref="ArgumentException">未登録の ViewModel を指定した場合に発生します。</exception>
		protected override bool OnNavigate( Page navigateToInstance ) => this.NavigationService.Navigate( navigateToInstance );

		/// <summary>
		/// ナビゲーション履歴が存在する場合、直前のエントリに移動します。
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		protected override void OnGoBack() => this.NavigationService.GoBack();

		/// <summary>
		/// ナビゲーション履歴が存在する場合、直後のエントリに移動します。
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		protected override void OnGoForward() => this.NavigationService.GoForward();

		/// <summary>
		/// ナビゲーション履歴の後方に、少なくとも 1 つ以上のエントリがあるかどうかを示す値を取得します。
		/// </summary>
		public override bool CanGoBack => this.NavigationService.CanGoBack;

		/// <summary>
		/// ナビゲーション履歴の前方に、少なくとも 1 つ以上のエントリがあるかどうかを示す値を取得します。
		/// </summary>
		public override bool CanGoForward => this.NavigationService.CanGoForward;

	}

	#endregion
}
