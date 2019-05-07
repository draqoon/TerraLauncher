using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Livet;
using Newtonsoft.Json;
using TerraLauncher.Resources;

namespace TerraLauncher.Models {

	[JsonObject()]
	public class ListModel<T>: NotificationObject, IDisposable, ICloneable where T : ItemModelBase {

		[JsonProperty( Order = 1 )]
		public ObservableCollection<T> Items { get; private set; }

		[JsonIgnore]
		private IEnumerable<T> FilteredItems => this.Items.Where( x => !x.Hidden ).Where( this.Filter );

		[JsonIgnore()]
		public ListCollectionView ViewItems {
			get {
				var array = this.FilteredItems.ToArray();
				var view = new ListCollectionView( array );

				if( this.Items.Any( x => !string.IsNullOrWhiteSpace( x.Group ) ) ) {
					view.GroupDescriptions.Add( new PropertyGroupDescription( nameof( ItemModelBase.Group ) ) );
				}

				return view;
			}
		}

		[JsonIgnore()]
		public T SelectedItem {
			get => this._selectedItem;
			set {
				if( this.RaisePropertyChangedIfSet( ref this._selectedItem, value ) ) {
					this.RaisePropertyChanged( nameof( this.CanDelete ) );
					this.RaisePropertyChanged( nameof( this.CanMoveUp ) );
					this.RaisePropertyChanged( nameof( this.CanMoveDown ) );
					this.RaisePropertyChanged( nameof( this.IsSelected ) );
				}
			}
		}
		private T _selectedItem = null;

		[JsonProperty( Order = 2 )]
		public string SelectedKey {
			get => this.IsSelected ? ( this.SelectedItem?.ID ?? "" ) : "";
			set => this.SelectedItem = this.FilteredItems?.FirstOrDefault( x => x.ID == value );
		}

		[JsonIgnore()]
		public bool IsSelected => this.FilteredItems.Any() && this.SelectedItem != null && this.FilteredItems.Contains( this.SelectedItem );

		[JsonIgnore()]
		public bool CanDelete => this.IsSelected && !this.SelectedItem.Locked;
		[JsonIgnore()]
		public bool CanMoveUp => this.IsSelected && !this.FilteredItems.First().Equals( this.SelectedItem );
		[JsonIgnore()]
		public bool CanMoveDown => this.IsSelected && !this.FilteredItems.Last().Equals( this.SelectedItem );

		[JsonIgnore()]
		public Func<T, bool> Filter {
			get => this._filter;
			set {
				if( value == null )
					value = item => true;
				if( this.RaisePropertyChangedIfSet( ref this._filter, value ) ) {
					var selectedItemBackup = this.SelectedItem;
					this.RaisePropertyChanged( nameof( this.ViewItems ) );
					if( this.ViewItems.Contains( selectedItemBackup ) )
						this.SelectedItem = selectedItemBackup;
				}
			}
		}
		private Func<T, bool> _filter = item => true;

		[JsonIgnore()]
		private string Name { get; }

		public ListModel( string name ) : this( name, Enumerable.Empty<T>(), null ) {
		}

		public ListModel( string name, IEnumerable<T> items, string selectedItemID ) {
			this.Name = name;
			this.Items = new ObservableCollection<T>( items );
			this.Items.CollectionChanged += this.CollectionChanged;

			if( !string.IsNullOrWhiteSpace( selectedItemID ) ) {
				this.SelectedItem = this.Items.FirstOrDefault( x => x.ID == selectedItemID );
			}
		}

		private void CollectionChanged( object sender, NotifyCollectionChangedEventArgs e ) {
			this.RaisePropertyChanged( nameof( this.CanDelete ) );
			this.RaisePropertyChanged( nameof( this.CanMoveUp ) );
			this.RaisePropertyChanged( nameof( this.CanMoveDown ) );
			this.RaisePropertyChanged( nameof( this.ViewItems ) );

			if( !this.Items.Any( x => x.ID == this.SelectedItem?.ID ) )
				this.SelectedItem = null;
		}

		public void Initialize( IEnumerable<T> initializeList ) {
			if( !this.Items.Any() ) {
				this.Items = new ObservableCollection<T>( initializeList );
			}
			foreach( var initialItem in initializeList ) {
				var item = this.Items.FirstOrDefault( x => x.ID == initialItem.ID );
				if( item == null ) {
					var clone = (T)initialItem.Clone();
					clone.Backup = initialItem;
					this.Items.Add( clone );
				}
				else {
					item.Locked = initialItem.Locked;
					item.Backup = initialItem;
				}
			}
			if( this.SelectedItem == null && this.Items.Any() ) {
				this.SelectedItem = this.Items[0];
			}
			foreach( var item in this.Items ) {
				if( string.IsNullOrWhiteSpace( item.ID ) ) {
					item.ID = new Guid().ToString();
				}
			}
		}

		public void MoveItem( bool down ) {
			if( this.SelectedItem == null )
				return;

			var index = this.Items.IndexOf( this.SelectedItem );
			if( index == -1 )
				return;

			if( down ) {
				if( !this.CanMoveDown )
					return;
				var next = this.Items[index + 1];
				this.Items.RemoveAt( index + 1 );
				this.Items.Insert( index, next );
			}
			else {
				if( !this.CanMoveUp )
					return;
				var prev = this.Items[index - 1];
				this.Items.RemoveAt( index - 1 );
				this.Items.Insert( index, prev );
			}
		}

		public void DeleteItem( T model ) {
			var index = this.Items.IndexOf( model );
			if( -1 < index ) {
				this.Items.RemoveAt( index );
				if( this.SelectedItem == model )
					this.SelectedItem = null;
			}
		}

		public void ToggleItemVisible( T model ) {
			model.Hidden = !model.Hidden;
		}

		#region implements for IDisposable

		private bool disposed = false;

		~ListModel() => this.Dispose( false );

		public void Dispose() {
			this.Dispose( true );
			GC.SuppressFinalize( this );
		}

		private void Dispose( bool disposing ) {
			if( this.disposed )
				return;

			if( disposing ) {
				if( this.Items != null )
					this.Items.CollectionChanged -= this.CollectionChanged;
			}

			this.OnDispose( disposing );

			this.disposed = true;
		}

		protected virtual void OnDispose( bool disposing ) {
		}

		#endregion

		#region implements for IClonable

		public object Clone() {
			var cloneObject = new ListModel<T>( this.Name, this.Items?.Select( x => (T)x.Clone() ) ?? Enumerable.Empty<T>(), this.SelectedItem?.ID );
			return cloneObject;
		}

		#endregion

		public void CopyTo( ListModel<T> other ) {
			other.Items = new ObservableCollection<T>( this.Items.Select( x => (T)x.Clone() ) );
			other.SelectedItem = other.Items.FirstOrDefault( x => x.ID == this.SelectedItem?.ID );
		}

		public IEnumerable<string> ValidateNotifyError() {
			if( this.SelectedItem == null || !this.Items.Any( x => x.ID == this.SelectedItem.ID ) ) {
				yield return AppResource.Lang( "RequireSelect", AppResource.Lang( this.Name ) );
			}
			else {
				foreach( var p in typeof( T ).GetProperties().Where( p => p.CanRead ) ) {
					var list = this.SelectedItem.ValidateNotifyError( p.Name );
					foreach( var item in list )
						yield return item;
				}
			}
		}

		[JsonIgnore()]
		public bool IsValid => !this.ValidateNotifyError().Any();

	}
}
