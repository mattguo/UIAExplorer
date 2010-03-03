using System;
using Gtk;
using GLib;
using System.Runtime.InteropServices;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	class OpacityWindow : Window
	{
		private readonly Gdk.Color BORDER_COLOR = new Gdk.Color (255, 0, 0);
		private byte alpha = 255;

		public OpacityWindow ()
			: base (WindowType.Popup)
		{
			ModifyBg (StateType.Normal, BORDER_COLOR);
			Decorated = false;
			KeepAbove = true;
			AcceptFocus = false;
			Sensitive = false;
			FocusOnMap = false;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			AppPaintable = true;
			DoubleBuffered = false;
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			using (Cairo.Context cc = Gdk.CairoHelper.Create (evnt.Window)) {
				SetSourceColor (cc);
				int w, h;
				GetSize (out w, out h);
	            cc.Rectangle (0, 0, w, h);
				cc.Fill ();
			}
			return true;
		}

		public void Show (byte alpha)
		{
			this.ShowAll ();
			if(GdkWindow != null) {
				this.alpha = alpha;
			}
		}

		protected void SetSourceColor (Cairo.Context cc)
		{
			double colorR = (double) BORDER_COLOR.Red / (double) ushort.MaxValue;
			double colorG = (double) BORDER_COLOR.Green / (double) ushort.MaxValue;
			double colorB = (double) BORDER_COLOR.Blue / (double) ushort.MaxValue;
			double colorA = (double) alpha / (double) byte.MaxValue;
			cc.SetSourceRGBA (colorR, colorG, colorG, colorA);
		}
	}

	class OffScreenindicator : OpacityWindow
	{
		public OffScreenindicator ()
		{
			Move (0, 0);
			Resize (640, 240);
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			using (Cairo.Context cc = Gdk.CairoHelper.Create (evnt.Window)) {
				SetSourceColor (cc);
				cc.MoveTo (40, 200);
				cc.SetFontSize (120);
				cc.ShowText ("Off Screen");
			}
			return true;
		}
	}

	public class Highlighter
	{
		private OpacityWindow bound;
		//private bool outOfScreen = false;

		public Highlighter (int x, int y, int w, int h)
		{
			var outOfScreen = x < 0 && y < 0 && x + w < 0 && y + h < 0;
			if (!outOfScreen)
				bound = CreateSolidWindow (x, y, w, h);
			else
				bound = new OffScreenindicator ();
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
			GLib.Timeout.Add (milliseconds, () => {
				bound.Destroy ();
				return false;
			});

			byte alpha = 0x80;
			bound.Show (alpha);
		}
	}
}
