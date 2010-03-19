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
		public const string Prompt = "Py> ";
		public const string IndentString = "\t";

		private ScriptEngine engine = null;
		private ScriptScope scope = null;
		private HistoryManager history = new HistoryManager ();
		private TextMark endOfLastProcessing;

		public Shell ()
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
			Buffer.InsertWithTagsByName (ref end,
				"IronPython Shell, use 'acc' to call the selected AutomationElement, " +
				"and type 'help()' for help.\nEnter statements or expressions below.\n",
				"Comment");
			ShowPrompt (true);

			InitRuntime ();
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

		private void InitRuntime ()
		{
			engine = Python.CreateEngine ();
			scope = engine.CreateScope ();
			engine.Runtime.IO.SetOutput (new GuiStream ("Stdout", (x, y) => Output (x, y)), Encoding.UTF8);
			engine.Runtime.IO.SetErrorOutput (new GuiStream ("Error", (x, y) => Output (x, y)), Encoding.UTF8);
			engine.Runtime.LoadAssembly (Assembly.Load ("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			engine.Runtime.LoadAssembly (Assembly.Load ("WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
			engine.Runtime.LoadAssembly (Assembly.Load ("UIAutomationClient, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
			engine.Runtime.LoadAssembly (Assembly.Load ("UIAutomationTypes, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
			StringBuilder imports = new StringBuilder ();
			// TODO write init script into a resource file
			imports.AppendLine ("from System import *");
			imports.AppendLine ("from System.Collections.Generic import *");
			imports.AppendLine ("from System.Windows import *");
			imports.AppendLine ("from System.Windows.Automation import *");
			imports.AppendLine ("from System.Linq import *");
			imports.AppendLine ("def help() : print 'press Ctrl+J to invoke auto-completion'");
			ScriptSource source = engine.CreateScriptSourceFromString (imports.ToString (), SourceCodeKind.Statements);
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

		private enum ScriptEndState
		{
			Nommal,
			EndWithColon,
			CommentJustFinish,
			InCommentBlock,
			EmptyLine
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
		}

		private string [] GetCompletions (string line, out int prefixLen, out int defaultIndex)
		{

			//TODO implement
			prefixLen = 2;
			defaultIndex = 5;
			return new string [] { "aaa", "bbb", "ccc", "aaa", "bbb", "ccc", "aaa", "bbb", "cccasdasdasdasdsadsadsadasdasdasdas" };
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

				case Gdk.Key.j:
				case Gdk.Key.J:
					// Press Ctrl+J to invoke auto-completion
					if ((evnt.State & Gdk.ModifierType.ControlMask) != Gdk.ModifierType.ControlMask)
						break;

					int prefixLen;
					int defaultIndex;
					string [] completions = GetCompletions (LineUntilCursor, out prefixLen, out defaultIndex);
					if (completions == null)
						return true;

					TextIter insertPos = Cursor;
					insertPos.LineIndex -= prefixLen;

					if (completions.Length == 1) {
						TextIter cursor = Cursor;
						Buffer.Delete (ref insertPos, ref cursor);
						Buffer.Insert (ref insertPos, completions [0]);
						return true;
					}
					
					// Show completion window
					int x, y;
					GdkWindow.GetOrigin (out x, out y);
					var r = GetIterLocation (Cursor);
					x += r.X;
					y += r.Y;
					var w = new CompletionWindow (completions, 5);
					w.Move (x, y);
					w.ShowAll ();
					w.SelectCompletion += (choice) => {
						TextIter cursor = Cursor;
						Buffer.Delete (ref insertPos, ref cursor);
						Buffer.Insert (ref insertPos, choice);
					};

					return true;

				default:
					break;
			}

			return base.OnKeyPressEvent (evnt);
		}

		private void ShowPrompt (bool newline)
		{
			TextIter end_iter = Buffer.EndIter;

			if (newline)
				Buffer.Insert (ref end_iter, Environment.NewLine);
			Buffer.Insert (ref end_iter, Prompt);
			endOfLastProcessing = Buffer.CreateMark (null, Buffer.EndIter, true);
			
			TextIter promptBegin = InputLineBegin;
			promptBegin.LineIndex -= Prompt.Length;
			
			Buffer.ApplyTag (Buffer.TagTable.Lookup ("Prompt"),
				promptBegin, InputLineBegin);
			Buffer.ApplyTag (Buffer.TagTable.Lookup ("Freezer"), Buffer.StartIter, InputLineBegin);
			

			//for (int i = 0; i < indent; i++)
			//    Buffer.Insert (ref end_iter, IndentString);

			//Buffer.PlaceCursor (Buffer.EndIter);
			//ScrollMarkOnscreen (Buffer.InsertMark);

			//TextIter prompt_start_iter = InputLineBegin;
			//prompt_start_iter.LineIndex -= prompt.Length;

			//TextIter prompt_end_iter = InputLineBegin;
			////prompt_end_iter.LineIndex -= 1;

			//Buffer.ApplyTag (Buffer.TagTable.Lookup (indent > 0 ? "PromptContinuation" : "Prompt"),
			//        prompt_start_iter, prompt_end_iter);
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
