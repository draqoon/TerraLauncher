using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using Microsoft.Win32;

namespace System.Windows {

	/// <summary>
	/// ディスプレイ デバイスまたは 1 つのシステム上の複数のディスプレイ デバイスを表します。
	/// </summary>
	public class Screen {

		#region Win32 API

		[DllImport( "user32.dll" )]
		private static extern bool EnumDisplayMonitors( IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData );

		private delegate bool MonitorEnumDelegate( IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData );

		[StructLayout( LayoutKind.Sequential )]
		private struct RECT {
			public int Left { get; set; }
			public int Top { get; set; }
			public int Right { get; set; }
			public int Bottom { get; set; }

			public Rect ToRect() => new Rect( this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top );
		}

		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		private static extern bool GetMonitorInfo( IntPtr hMonitor, ref MonitorInfoEx lpmi );

		// size of a device name string
		private const int CCHDEVICENAME = 32;

		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
		private struct MonitorInfoEx {
			public int Size;
			public RECT Monitor;
			public RECT WorkArea;
			public MonitorInfoFlags Flags;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME )] public string DeviceName;
		}

		[Flags()]
		private enum MonitorInfoFlags {
			None = 0,
			Primary = 1
		}

		// MonitorFromWindowが返したディスプレイの種類
		public enum MonitorDefaultTo { Null, Primary, Nearest }
		// GetDpiForMonitorが返したDPIの種類
		private enum MonitorDpiType { Effective, Angular, Raw, Default = Effective }
		// ディスプレイハンドルからDPIを取得
		[DllImport( "SHCore.dll", CharSet = CharSet.Unicode, PreserveSig = false )]
		private static extern void GetDpiForMonitor( IntPtr hmonitor, MonitorDpiType dpiType, ref uint dpiX, ref uint dpiY );

		// ウィンドウハンドルから、そのウィンドウが乗っているディスプレイハンドルを取得
		[DllImport( "user32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
		private static extern IntPtr MonitorFromWindow( IntPtr hwnd, MonitorDefaultTo dwFlags );

		#endregion

		#region static member

		/// <summary>
		/// プライマリ ディスプレイを取得します。
		/// </summary>
		public static Screen PrimaryScreen => AllScreens.FirstOrDefault( screen => screen.Primary );

		private static bool NeedUpdateAllScreens = true;

		/// <summary>
		/// システム上のすべてのディスプレイの配列を取得します。
		/// </summary>
		public static Screen[] AllScreens {
			get {
				if( NeedUpdateAllScreens ) {
					NeedUpdateAllScreens = false;
					UpdateScreens();
				}
				return _allScreens;
			}
			private set => _allScreens = value;
		}
		private static Screen[] _allScreens;

		static Screen() {
			SystemEvents.DisplaySettingsChanged += ( sender, e ) => NeedUpdateAllScreens = true;
		}

		private static void UpdateScreens() {
			var screens = new List<Screen>();
			EnumDisplayMonitors( IntPtr.Zero, IntPtr.Zero,
						( IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData ) => {
							var screen = new Screen() { MonitorHandle = hMonitor };

							var mi = new MonitorInfoEx();
							mi.Size = Marshal.SizeOf( mi );
							var success = GetMonitorInfo( hMonitor, ref mi );
							if( success ) {
								screen.DeviceName = mi.DeviceName;
								screen.Bounds = mi.Monitor.ToRect();
								screen.WorkingArea = mi.WorkArea.ToRect();
								screen.Primary = ( mi.Flags & MonitorInfoFlags.Primary ) != 0;
							}

							uint dpiX = 0;
							uint dpiY = 0;
							GetDpiForMonitor( hMonitor, MonitorDpiType.Default, ref dpiX, ref dpiY );

							screen.DpiX = (int)dpiX;
							screen.DpiY = (int)dpiY;

							screens.Add( screen );
							return true;
						}, IntPtr.Zero );
			AllScreens = screens.ToArray();
		}

		/// <summary>
		/// 指定したウインドウの最大を含むディスプレイを取得します。
		/// </summary>
		/// <param name="window">取得する対象のウインドウを指定します。</param>
		/// <param name="monitorDefaultTo"></param>
		/// <returns>取得するウインドウを含むディスプレイを返します。</returns>
		public static Screen FromWindow( Window window, MonitorDefaultTo monitorDefaultTo = MonitorDefaultTo.Nearest ) {
			var hWnd = new WindowInteropHelper( window ).Handle;
			var hMonitor = MonitorFromWindow( hWnd, monitorDefaultTo );
			return AllScreens.FirstOrDefault( screen => screen.MonitorHandle == hMonitor );
		}

		/// <summary>
		/// 指定した位置を含むディスプレイを取得します。
		/// </summary>
		/// <param name="point">取得する対象の位置を指定します。</param>
		/// <returns>取得する位置を含むディスプレイを返します。</returns>
		public static Screen FromPoint( Point point ) => AllScreens.FirstOrDefault( screen => screen.Bounds.Contains( point ) );

		#endregion

		#region instance member

		/// <summary>
		/// モニターハンドルを取得します。
		/// </summary>
		public IntPtr MonitorHandle { get; private set; }

		/// <summary>
		/// 画面に関連付けられているデバイス名を取得します。
		/// </summary>
		public string DeviceName { get; private set; }

		/// <summary>
		/// ディスプレイの範囲を取得します。
		/// </summary>
		public Rect Bounds { get; private set; }

		/// <summary>
		/// 画面の作業領域を取得します。 作業領域は、複数のタスク バー、ドッキング ウィンドウとドッキングされているツール バーを除く、ディスプレイのデスクトップの領域です。
		/// </summary>
		public Rect WorkingArea { get; private set; }

		/// <summary>
		/// 特定のディスプレイがプライマリ デバイスであるかどうかを示す値を取得します。
		/// </summary>
		public bool Primary { get; private set; }

		/// <summary>
		/// 横方向のDPIを取得します。
		/// ただし、dpiAwareが"True/PM"でない場合は、常にプライマリディスプレイのDPIを取得します。
		/// </summary>
		public int DpiX { get; private set; }

		/// <summary>
		/// 縦方向のDPIを取得します。
		/// ただし、dpiAwareが"True/PM"でない場合は、常にプライマリディスプレイのDPIを取得します。
		/// </summary>
		public int DpiY { get; private set; }

		#endregion

	}
}
