using System;
using Mono.TextEditor;
using Mono.TextEditor.Highlighting;
using Cairo;

namespace ReplOverMdEditor
{
	public class PromptMarker : TextMarker, IIconBarMarker
	{

		public static PromptMarker StartPrompt = new PromptMarker (">");
		public static PromptMarker ContinuousPrompt = new PromptMarker ("-");

		public PromptMarker (String prompt)
		{
			if (string.IsNullOrEmpty (prompt))
				throw new ArgumentNullException ("prompt");
			Prompt = prompt;
		}

		public void DrawIcon (Mono.TextEditor.TextEditor editor, Gdk.Drawable win, LineSegment lineSegment, int lineNumber, int x, int y, int width, int height)
		{
			using (Context cr = Gdk.CairoHelper.Create (win)) {
				cr.SetSourceRGBA (0.5, 0.5, 0.0, 1.0);
				cr.MoveTo (x + 5, y + height - 1);
				cr.SetFontSize (height - 2);
				cr.SelectFontFace ("Sans", FontSlant.Normal, FontWeight.Bold);
				cr.ShowText (Prompt);
			}
		}

		public void MousePress (MarginMouseEventArgs args)
		{
		}

		public void MouseRelease (MarginMouseEventArgs args)
		{
		}

		public string Prompt
		{
			get;
			protected set;
		}
	}

}
