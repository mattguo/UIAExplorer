using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Gtk;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using System.Runtime.InteropServices;


namespace IronPythonRepl
{
	// TODO add current line into the history
	// TODO color format current line
	// TODO Auto completion
	// TODO make the init title and script configurable, i.e. settable.
	// TODO no colorize, completion and indent in """ string
	public class Shell : TextView
	{
		private const string Prompt = "Py> ";
		private const string IndentString = "\t";

		private ScriptEngine engine = null;
		private ScriptScope scope = null;
		private HistoryManager history = new HistoryManager ();
		private TextMark endOfLastProcessing;

		public Shell (Assembly[] assembliesToLoad, string initScript, string title)
			: base ()
		{
			AcceptsTab = true;
			Pango.TabArray tabs = new Pango.TabArray (1, true);
			tabs.SetTab (0, Pango.TabAlign.Left, GetStringWidth (" ") * 8);
			Tabs = tabs;

			WrapMode = WrapMode.Word;
			CreateTags ();

			Pango.FontDescription font_description = new Pango.FontDescription ();
			font_description.Family = "Monospace";
			ModifyFont (font_description);

			TextIter end = Buffer.EndIter;
			// TODO
			Buffer.InsertWithTagsByName (ref end, title, "Comment");
			ShowPrompt (true);

			InitRuntime (assembliesToLoad, initScript);
		}

		public void SetVariable (string varName, object varValue)
		{
			scope.SetVariable (varName, varValue);
		}

		private int GetStringWidth (string str)
		{
			int width, height;
			
			Pango.Layout layout = new Pango.Layout (this.PangoContext);
			layout.SetText (str);
			layout.GetPixelSize (out width, out height);
			layout.Dispose ();
			return width;
		}

		private void InitRuntime (Assembly[] assembliesToLoad, string initScript)
		{
			engine = Python.CreateEngine ();
			scope = engine.CreateScope ();
			engine.Runtime.IO.SetOutput (new GuiStream ("Stdout", (x, y) => Output (x, y)), Encoding.UTF8);
			engine.Runtime.IO.SetErrorOutput (new GuiStream ("Error", (x, y) => Output (x, y)), Encoding.UTF8);
			foreach (var assembly in assembliesToLoad)
				engine.Runtime.LoadAssembly (assembly);
			ScriptSource source = engine.CreateScriptSourceFromString (initScript, SourceCodeKind.Statements);
			source.Execute (scope);
		}

		void CreateTags ()
		{
			TextTag freeze_tag = new TextTag ("Freezer") {
				Editable = false
			};
			Buffer.TagTable.Add (freeze_tag);

			TextTag prompt_tag = new TextTag ("Prompt") {
				Foreground = "orange",
				//Background = "#f8f8f8",
				Weight = Pango.Weight.Bold
			};
			Buffer.TagTable.Add (prompt_tag);

			TextTag prompt_continuation_tag = new TextTag ("PromptContinuation") {
				Foreground = "orange",
				//Background = "#f8f8f8",
				Weight = Pango.Weight.Bold
			};
			Buffer.TagTable.Add (prompt_continuation_tag);

			TextTag error_tag = new TextTag ("Error") {
				Foreground = "red"
			};
			Buffer.TagTable.Add (error_tag);

			TextTag stdout_tag = new TextTag ("Stdout") {
				Foreground = "blue"
			};
			Buffer.TagTable.Add (stdout_tag);

			TextTag comment = new TextTag ("Comment") {
				Foreground = "darkgreen"
			};
			Buffer.TagTable.Add (comment);
		}

		private bool CalcIndent (string script, out string indent)
		{
			indent = "";
			if (string.IsNullOrEmpty (script))
				return false;
			if (script [0] == ' ' || script [0] == '\t') {
				ShowError (Environment.NewLine + "unexpected indent, ");
				return false;
			}

			int lastEol = script.LastIndexOfAny (new char [] { '\n', '\r' });
			string lastLine = script.Substring (lastEol + 1);
			string trimmedLastLine = lastLine.TrimStart (' ', '\t');
			string lastIndent = lastLine.Substring (0, lastLine.Length - trimmedLastLine.Length);
			trimmedLastLine = trimmedLastLine.TrimEnd (' ', '\t');

			if (string.IsNullOrEmpty (trimmedLastLine))
				return false;
			else {
				indent = lastIndent;
				if (trimmedLastLine.EndsWith (":"))
					indent += IndentString;
				return indent.Length > 0;
			}
		}

		private void InputScript ()
		{
			string script = InputLine;
			string indent;
			bool continuation = CalcIndent (script, out indent);
			
			TextIter end = Buffer.EndIter;
			if (continuation) {
				Buffer.Insert (ref end, Environment.NewLine + indent);
			} else {
				Buffer.InsertWithTagsByName (ref end, Environment.NewLine, "Stdout");
				var source = engine.CreateScriptSourceFromString (script, SourceCodeKind.InteractiveCode);
				try {
					source.Execute (scope);
				} catch (Exception e) {
					ShowError (e.Message + Environment.NewLine);
				}
				ShowPrompt (false);
				history.AppendHistory (script);
			}
			ScrollToIter (Buffer.EndIter, 0, false, 0, 0);
		}

		private string [] GetCompletions (string line, out int prefixLen, out int defaultIndex)
		{
			//TODO THe following is still a simple while buggy implementation
			int lineIndex = line.Length - 1;
			for (; lineIndex >= 0; lineIndex--) {
				char ch = line[lineIndex] ;
				if (!(ch == '.' || ch == '_' || (ch >= 'a' && ch <= 'z') ||
					(ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9')))
					break;
			}
			lineIndex++;

			string prefix = line.Substring (lineIndex, line.Length - lineIndex);
			int lastDotIndex = prefix.LastIndexOf ('.');
			string prefixObject;
			string prefixMember;
			if (lastDotIndex != -1) {
				prefixObject = line.Substring (0, lastDotIndex);
				prefixMember = line.Substring (lastDotIndex + 1, prefix.Length - lastDotIndex -1);
			}
			else {
				prefixObject = "";
				prefixMember = prefix;
			}
			prefixLen = prefixMember.Length;

			string [] completions = null;
			defaultIndex = 0;
			var source = engine.CreateScriptSourceFromString (
				string.Format ("dir({0})", prefixObject), SourceCodeKind.Expression);
			try {
				var dirResult = (IronPython.Runtime.List) source.Execute (scope);
				List <string> completionList = new List<string>();
				for (int i = 0; i < dirResult.Count; i++)
					completionList.Add (dirResult[i].ToString ());
				completionList.Sort (StringComparer.InvariantCultureIgnoreCase);
				defaultIndex = completionList.BinarySearch (prefixMember, StringComparer.InvariantCultureIgnoreCase);
				if (defaultIndex < 0)
					defaultIndex = -defaultIndex;
				if (defaultIndex >= completionList.Count)
					defaultIndex = completionList.Count - 1;
				completions = completionList.ToArray ();
			} catch (Exception) {
			}

			return completions;
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			if (Cursor.Compare (InputLineBegin) < 0) {
				Buffer.MoveMark (Buffer.SelectionBound, InputLineEnd);
				Buffer.MoveMark (Buffer.InsertMark, InputLineEnd);
			}

			switch (evnt.Key) {
				case Gdk.Key.Return:
				case Gdk.Key.KP_Enter:
					InputScript ();
					return true;

				case Gdk.Key.Page_Up:
					history.HistoryUp ();
					InputLine = history.Current;
					return true;

				case Gdk.Key.Page_Down:
					history.HistoryDown ();
					InputLine = history.Current;
					return true;

				case Gdk.Key.Left:
					if (Cursor.Compare (InputLineBegin) <= 0)
						return true;
					break;

				case Gdk.Key.Up:
					var lineUntilCursor = LineUntilCursor;
					if (lineUntilCursor.IndexOfAny (new char [] { '\r', '\n' }) == -1)
						return true;
					break;

				case Gdk.Key.Home:
					Buffer.MoveMark (Buffer.InsertMark, InputLineBegin);
					if ((evnt.State & Gdk.ModifierType.ShiftMask) != Gdk.ModifierType.ShiftMask) {
						Buffer.MoveMark (Buffer.SelectionBound, InputLineBegin);
					}
					return true;

				// TODO disable implicit ShowCompletionWindow for now.
				//case Gdk.Key.period:
				//    TextIter end = Buffer.EndIter;
				//    Buffer.Insert (ref end, ".");
				//    ShowCompletionWindow ();
				//    return true;
				case Gdk.Key.j:
				case Gdk.Key.J:
					// Press Ctrl+J to invoke auto-completion
					if ((evnt.State & Gdk.ModifierType.ControlMask) != Gdk.ModifierType.ControlMask)
						break;
					ShowCompletionWindow ();
					return true;

				default:
					break;
			}

			return base.OnKeyPressEvent (evnt);
		}

		private void ShowCompletionWindow ()
		{
			int prefixLen;
			int defaultIndex;
			string [] completions = GetCompletions (LineUntilCursor, out prefixLen, out defaultIndex);
			if (completions == null)
				return;

			TextIter insertPos = Cursor;
			insertPos.BackwardChars (prefixLen);

			if (completions.Length == 1) {
				TextIter cursor = Cursor;
				Buffer.Delete (ref insertPos, ref cursor);
				Buffer.Insert (ref insertPos, completions [0]);
				return;
			}

			int x, y;
			GdkWindow.GetOrigin (out x, out y);
			var r = GetIterLocation (Cursor);
			x += r.X;
			y += r.Y;
			var w = new CompletionWindow (completions, defaultIndex);
			w.Move (x, y);
			w.ShowAll ();
			w.SelectCompletion += (choice) => {
				TextIter cursor = Cursor;
				Buffer.Delete (ref insertPos, ref cursor);
				Buffer.Insert (ref insertPos, choice);
			};
		}

		private void ShowPrompt (bool newline)
		{
			TextIter end_iter = Buffer.EndIter;

			if (newline)
				Buffer.Insert (ref end_iter, Environment.NewLine);
			Buffer.Insert (ref end_iter, Prompt);
			endOfLastProcessing = Buffer.CreateMark (null, Buffer.EndIter, true);
			
			TextIter promptBegin = InputLineBegin;
			promptBegin.BackwardChars (Prompt.Length);
			
			Buffer.ApplyTag (Buffer.TagTable.Lookup ("Prompt"),
				promptBegin, InputLineBegin);
			Buffer.ApplyTag (Buffer.TagTable.Lookup ("Freezer"), Buffer.StartIter, InputLineBegin);
		}

		private void Output (string kind, string s)
		{
			TextIter end = Buffer.EndIter;
			Buffer.InsertWithTagsByName (ref end, s, kind);
		}

		private void ShowError (string err)
		{
			Output ("Error", err);
		}

		TextIter InputLineBegin
		{
			get { return Buffer.GetIterAtMark (endOfLastProcessing); }
		}

		TextIter InputLineEnd
		{
			get { return Buffer.EndIter; }
		}

		TextIter Cursor
		{
			get { return Buffer.GetIterAtMark (Buffer.InsertMark); }
		}

		string InputLine
		{
			get { return Buffer.GetText (InputLineBegin, InputLineEnd, false); }
			set
			{
				TextIter start = InputLineBegin;
				TextIter end = InputLineEnd;
				Buffer.Delete (ref start, ref end);
				start = InputLineBegin;
				Buffer.Insert (ref start, value);
				ScrollMarkOnscreen (Buffer.InsertMark);
			}
		}

		string LineUntilCursor
		{
			get
			{
				return Buffer.GetText (InputLineBegin, Cursor, false);
			}
		}

		public void ClearText ()
		{
			Buffer.Clear ();
			ShowPrompt (false);
		}
	}

	class GuiStream : Stream
	{
		string kind;
		Action<string, string> callback;

		public GuiStream (string k, Action<string, string> cb)
		{
			kind = k;
			callback = cb;
		}

		public override bool CanRead { get { return false; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return true; } }


		public override long Length { get { return 0; } }
		public override long Position { get { return 0; } set { } }
		public override void Flush () { }
		public override int Read ([In, Out] byte [] buffer, int offset, int count) { return -1; }

		public override long Seek (long offset, SeekOrigin origin) { return 0; }

		public override void SetLength (long value) { }

		public override void Write (byte [] buffer, int offset, int count)
		{
			callback (kind, Encoding.UTF8.GetString (buffer, offset, count));
		}
	}

}
