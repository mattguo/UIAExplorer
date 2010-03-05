using System;
using System.Windows.Automation;
using System.Linq;
using Gdk;
using Gtk;
using System.Reflection;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	public static class IconUtil
	{
		private static IconFactory iconFactory = new IconFactory();

		public static void Initialize ()
		{
			iconFactory.AddDefault();
			AddStockIconFromResource ("invalid",
				"Mono.Accessibility.UIAExplorer.Icons.invalid.png",
				IconSize.Menu);
			AddControlTypeIcons ();
		}

		public static Pixbuf GetIcon (int windowHandle)
		{
#if WIN32
			throw new NotImplementedException();
#else
			var screen = Wnck.Screen.Default;
			screen.ForceUpdate ();
			Wnck.Window wnd = screen.Windows.FirstOrDefault (w => w.Xid == (ulong)windowHandle);
			return (wnd != null) ? wnd.MiniIcon : null;
#endif
		}

		public static string GetStockName (ControlType ct)
		{
			int dotIndex = ct.ProgrammaticName.LastIndexOf ('.');
			return ct.ProgrammaticName.Substring (dotIndex + 1).ToLower ();
		}

		private static void AddStockIconFromResource (string stockName,
			string resName, IconSize iconSize)
		{
			var ass = Assembly.GetExecutingAssembly ();
			if (ass.GetManifestResourceInfo(resName) == null) {
				Log.Error ("Resource: \"{0}\" not found", resName);
				return;
			}
			IconSource source = new IconSource();
			source.Pixbuf = new Pixbuf (ass, resName);
			source.Size = iconSize;
			StockItem item = new StockItem(stockName, null, 0, Gdk.ModifierType.ShiftMask, null);
			var iconSet = new IconSet ();
			iconSet.AddSource (source);
			iconFactory.Add (stockName, iconSet);
			StockManager.Add (item);
		}

		private static void AddControlTypeIcons ()
		{
			foreach (var fieldInfo in
				typeof (ControlType).GetFields (BindingFlags.Public | BindingFlags.Static)) {
				var stockName = fieldInfo.Name.ToLower ();
				AddStockIconFromResource (stockName,
					string.Format ("Mono.Accessibility.UIAExplorer.Icons.ControlType.{0}.png", stockName),
					IconSize.Menu);
			}
		}
	}
}