using System;
using Gtk;
using GLib;
using System.Runtime.InteropServices;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	class OpacityWindow : Window
	{
		private readonly Gdk.Color BORDER_COLOR = new Gdk.Color (255, 0, 0);

		public OpacityWindow ()
			: base (WindowType.Popup)
		{
			ModifyBg (StateType.Normal, BORDER_COLOR);
			Decorated = false;
			KeepAbove = true;
			AcceptFocus = false;
			Sensitive = false;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
		}

#if WIN32
		public const int GWL_EXSTYLE = -20;
		public const int WS_EX_LAYERED = 0x80000;
		public const int LWA_ALPHA = 0x2;
		public const int LWA_COLORKEY = 0x1;

		[DllImport ("user32.dll", SetLastError = true)]
		static extern int GetWindowLong (IntPtr hWnd, int nIndex);

		[DllImport ("user32.dll")]
		static extern int SetWindowLong (IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport ("user32.dll")]
		static extern bool SetLayeredWindowAttributes (IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		[DllImport ("libgdk-win32-2.0-0.dll")]
		static extern IntPtr gdk_win32_drawable_get_handle (IntPtr handle);
#endif
		//set the window style to alpha appearance
		public void Show (byte alpha)
		{
			this.ShowAll ();
#if WIN32
			if (GdkWindow != null) {
				IntPtr nativeHandle = gdk_win32_drawable_get_handle (GdkWindow.Handle);
				if (nativeHandle != IntPtr.Zero) {
					SetWindowLong (nativeHandle, GWL_EXSTYLE,
						GetWindowLong (nativeHandle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
					SetLayeredWindowAttributes (nativeHandle, 0, alpha, LWA_ALPHA);
				}
			}
#else
#endif
		}
	}

	public class Highlighter
	{
		private OpacityWindow left;
		private OpacityWindow right;
		private OpacityWindow top;
		private OpacityWindow bottom;
		private bool outOfScreen = false;

		private readonly int BORDER_WIDTH = 6;

		public Highlighter (int x, int y, int w, int h)
		{
			int x2 = x + w;
			int y2 = y + h;
			outOfScreen = x < 0 && y < 0 && x2 < 0 && y2 < 0;
			if (!outOfScreen) {
				left = CreateBorderWindow (x, y, x, y2);
				right = CreateBorderWindow (x2, y, x2, y2);
				top = CreateBorderWindow (x, y, x2, y);
				bottom = CreateBorderWindow (x, y2, x2, y2);
			}
		}

		private OpacityWindow CreateBorderWindow (int x1, int y1, int x2, int y2)
		{
			int halfBorderWidth = BORDER_WIDTH / 2;
			Gdk.Rectangle r1 = new Gdk.Rectangle (
				x1 - halfBorderWidth, y1 - halfBorderWidth,
				BORDER_WIDTH, BORDER_WIDTH);
			Gdk.Rectangle r2 = new Gdk.Rectangle (
				x2 - halfBorderWidth, y2 - halfBorderWidth,
				BORDER_WIDTH, BORDER_WIDTH);
			r1 = r1.Union (r2);
			return CreateSolidWindow (r1.Left, r1.Top, r1.Width, r1.Height);
		}

		private OpacityWindow CreateSolidWindow (int x, int y, int w, int h)
		{
			var wnd = new OpacityWindow ();
			wnd.Move (x, y);
			wnd.Resize (w, h);
			return wnd;
		}

		public void Flash (uint milliseconds)
		{
			if (!outOfScreen) {
				GLib.Timeout.Add (milliseconds, () => {
					left.Destroy ();
					right.Destroy ();
					top.Destroy ();
					bottom.Destroy ();
					return true;
				});

				byte opacity = 0x80;
				left.Show (opacity);
				right.Show (opacity);
				top.Show (opacity);
				bottom.Show (opacity);
			}
		}
	}
}
