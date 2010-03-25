using System;
using Mono.TextEditor;
using Cairo;

namespace ReplOverMdEditor
{
	public class SeparatorMarker : TextMarker
	{
		public override void Draw (TextEditor editor, Gdk.Drawable win, Pango.Layout layout, bool selected, int startOffset, int endOffset, int y, int startXPos, int endXPos)
		{
			using (Gdk.GC gc = new Gdk.GC(win)) {
				gc.RgbFgColor = new Gdk.Color (0xC0, 0xC0, 0xC0);
				int x = editor.TextViewMargin.XOffset;
				int h = y + editor.LineHeight - 1;
				win.DrawLine (gc, x, h, x + 600, h);
			}
		}
	}
}
