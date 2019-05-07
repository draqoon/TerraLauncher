using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Livet.Behaviors;

namespace TerraLauncher.Views.Controls {
    /// <summary>
    /// LabelCombobox.xaml の相互作用ロジック
    /// </summary>
    public partial class LabelCombobox : UserControl {
        public LabelCombobox() {
            this.InitializeComponent();
        }

        #region CanSelect

        public static readonly DependencyProperty CanSelectProperty
                = DependencyProperty.Register(
                        nameof( CanSelect ),
                        typeof( bool ),
                        typeof( LabelCombobox ),
                        new PropertyMetadata( true ) );

        [Bindable( true, BindingDirection.TwoWay )]
        public bool CanSelect {
            get => (bool)this.GetValue( CanSelectProperty );
            set => this.SetValue( CanSelectProperty, value );
        }

        #endregion

        #region CanEdit

        public static readonly DependencyProperty CanEditProperty
                = DependencyProperty.Register(
                        nameof( CanEdit ),
                        typeof( bool ),
                        typeof( LabelCombobox ),
                        new PropertyMetadata( true ) );

        [Bindable( true, BindingDirection.TwoWay )]
        public bool CanEdit {
            get => (bool)this.GetValue( CanEditProperty );
            set => this.SetValue( CanEditProperty, value );
        }

        #endregion

        #region ItemsSource

        public static readonly DependencyProperty ItemsSourceProperty
                = DependencyProperty.Register(
                        nameof( ItemsSource ),
                        typeof( IEnumerable ),
                        typeof( LabelCombobox ) );

        [Bindable( true, BindingDirection.OneWay )]
        public IEnumerable ItemsSource {
            get => (IEnumerable)this.GetValue( ItemsSourceProperty );
            set => this.SetValue( ItemsSourceProperty, value );
        }

        #endregion

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty
                = DependencyProperty.Register(
                        nameof( SelectedItem ),
                        typeof( object ),
                        typeof( LabelCombobox ),
                        new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        [Bindable( true, BindingDirection.TwoWay )]
        public object SelectedItem {
            get => this.GetValue( SelectedItemProperty );
            set => this.SetValue( SelectedItemProperty, value );
        }

        #endregion

        #region ComboboxMinWidth

        public static readonly DependencyProperty ComboboxMinWidthProperty
                = DependencyProperty.Register(
                        nameof( ComboboxMinWidth ),
                        typeof( double ),
                        typeof( LabelCombobox ),
                        new PropertyMetadata( 0.0 ) );

        [Bindable( true, BindingDirection.OneWay )]
        public double ComboboxMinWidth {
            get => (double)this.GetValue( ComboboxMinWidthProperty );
            set => this.SetValue( ComboboxMinWidthProperty, value );
        }

        #endregion

        #region LabelContent

        public static readonly DependencyProperty LabelContentProperty
                = DependencyProperty.Register(
                        nameof( LabelContent ),
                        typeof( object ),
                        typeof( LabelCombobox ),
                        new PropertyMetadata( "Label" ) );

        [Bindable( true, BindingDirection.OneWay )]
        public object LabelContent {
            get => this.GetValue( LabelContentProperty );
            set => this.SetValue( LabelContentProperty, value );
        }

        #endregion

        #region LabelMinWidth

        public static readonly DependencyProperty LabelMinWidthProperty
                = DependencyProperty.Register(
                        nameof( LabelMinWidth ),
                        typeof( double ),
                        typeof( LabelCombobox ),
                        new PropertyMetadata( 0.0 ) );

        [Bindable( true )]
        public double LabelMinWidth {
            get => (double)this.GetValue( LabelMinWidthProperty );
            set => this.SetValue( LabelMinWidthProperty, value );
        }

        #endregion

        #region EditMethodTarget

        public static readonly DependencyProperty EditMethodTargetProperty
                = DependencyProperty.Register(
                        nameof( EditMethodTarget ),
                        typeof( object ),
                        typeof( LabelCombobox ),
                        new PropertyMetadata( null ) );

        [Bindable( true )]
        public object EditMethodTarget {
            get => this.GetValue( EditMethodTargetProperty );
            set => this.SetValue( EditMethodTargetProperty, value );
        }

        #endregion

        #region EditMethodName

        public static readonly DependencyProperty EditMethodNameProperty
                = DependencyProperty.Register(
                        nameof( EditMethodName ),
                        typeof( string ),
                        typeof( LabelCombobox ),
                        new PropertyMetadata( null ) );

        [Bindable( true )]
        public string EditMethodName {
            get => (string)this.GetValue( EditMethodNameProperty );
            set => this.SetValue( EditMethodNameProperty, value );
        }

        #endregion

        #region EditMethodParameter

        public static readonly DependencyProperty EditMethodParameterProperty
                = DependencyProperty.Register(
                        nameof( EditMethodParameter ),
                        typeof( object ),
                        typeof( LabelCombobox ),
                        new PropertyMetadata( null ) );

        [Bindable( true )]
        public object EditMethodParameter {
            get => this.GetValue( EditMethodParameterProperty );
            set => this.SetValue( EditMethodParameterProperty, value );
        }

        #endregion

    }
}
