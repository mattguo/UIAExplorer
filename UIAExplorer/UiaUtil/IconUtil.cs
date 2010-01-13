using System;
using System.Linq;
using Gdk;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	public static class IconUtil
	{
		public static Pixbuf GetIcon (int windowHandle)
		{
			var screen = Wnck.Screen.Default;
			screen.ForceUpdate ();
			Wnck.Window wnd = screen.Windows.FirstOrDefault (w => w.Xid == (ulong)windowHandle);
			return (wnd != null) ? wnd.MiniIcon : null;
		}
	}
}