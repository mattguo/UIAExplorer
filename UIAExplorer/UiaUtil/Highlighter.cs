using System;
using Gtk;
using GLib;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	public class Highlighter
	{
		private Window left;
		private Window right;
		private Window top;
		private Window bottom;
		
		private readonly int BORDER_WIDTH = 4;
		private readonly Gdk.Color BORDER_COLOR = new Gdk.Color (255, 127, 127);
		
		public Highlighter (int x, int y, int w, int h)
		{
#if WIN32
			left = CreateSolidWindow (x, y, w, h);
#else
			int x2 = x + w;
			int y2 = y + h;
			left = CreateBorderWindow (x, y, x, y2);
			right = CreateBorderWindow (x2, y, x2, y2);
			top = CreateBorderWindow (x, y, x2, y);
			bottom = CreateBorderWindow (x, y2, x2, y2);
#endif
		}

		private Gtk.Window CreateBorderWindow (int x1, int y1, int x2, int y2)
		{
			int halfBorderWidth = BORDER_WIDTH / 2;
			Gdk.Rectangle r1 = new Gdk.Rectangle (
				x1 - halfBorderWidth, y1 - halfBorderWidth,
				BORDER_WIDTH,  BORDER_WIDTH);
			Gdk.Rectangle r2 = new Gdk.Rectangle (
				x2 - halfBorderWidth, y2 - halfBorderWidth,
				BORDER_WIDTH, BORDER_WIDTH);
			r1 = r1.Union (r2);
			return CreateSolidWindow (r1.Left, r1.Top, r1.Width, r1.Height);
		}

		private Gtk.Window CreateSolidWindow (int x, int y, int w, int h)
		{
			var wnd = new Gtk.Window (WindowType.Toplevel);
			wnd.ModifyBg (StateType.Normal, BORDER_COLOR);
			wnd.Move (x, y);
			wnd.Resize (w, h);
			wnd.Decorated = false;
			wnd.KeepAbove = true;
			wnd.AcceptFocus = false;
			wnd.Sensitive = false;
			wnd.SkipTaskbarHint = true;
			return wnd;
		}

		public void Flash (uint milliseconds)
		{
			GLib.Timeout.Add (milliseconds, () => {
				left.Destroy ();
#if !WIN32
				right.Destroy ();
				top.Destroy ();
				bottom.Destroy ();
#endif
				return true;
			});
			left.ShowAll ();
#if !WIN32
			right.ShowAll ();
			top.ShowAll ();
			bottom.ShowAll ();
#endif
		}
	}
}
