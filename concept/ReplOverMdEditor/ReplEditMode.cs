using System;
using System.Collections.Generic;
using System.Text;
using Mono.TextEditor;
using System.Reflection;
using Mono.TextEditor.Highlighting;

namespace ReplOverMdEditor
{
	public enum SelectionAreaType
	{
		Empty = 0,
		InEditArea = 1,
		InReadonlyArea = 2,
		InBothArea = 3
	}

	public delegate void NewLinesHandler (ReplEditMode sender, int startLine, int lineCount);

	public class ReplEditMode : SimpleEditMode
	{
		#region Static Members
		private static List<Action<TextEditorData>> writeActions = new List<Action<TextEditorData>> ();
		private static List<Action<TextEditorData>> backDeleteActions = new List<Action<TextEditorData>> ();

		static ReplEditMode ()
		{
			writeActions.Add (MiscActions.IndentSelection);
			writeActions.Add (MiscActions.InsertNewLine);
			writeActions.Add (MiscActions.InsertNewLineAtEnd);
			writeActions.Add (MiscActions.InsertNewLinePreserveCaretPosition);
			writeActions.Add (MiscActions.InsertTab);
			writeActions.Add (MiscActions.RemoveIndentSelection);
			writeActions.Add (MiscActions.RemoveTab);
			writeActions.Add (DeleteActions.Backspace);
			writeActions.Add (DeleteActions.CaretLine);
			writeActions.Add (DeleteActions.CaretLineToEnd);
			writeActions.Add (DeleteActions.Delete);
			writeActions.Add (DeleteActions.DeleteSelection);
			writeActions.Add (DeleteActions.NextSubword);
			writeActions.Add (DeleteActions.NextWord);
			writeActions.Add (DeleteActions.PreviousSubword);
			writeActions.Add (DeleteActions.PreviousWord);
			writeActions.Add (DeleteActions.RemoveCharBeforeCaret);
			writeActions.Add (ClipboardActions.Cut);
			writeActions.Add (ClipboardActions.Paste);

			backDeleteActions.Add (DeleteActions.Backspace);
			backDeleteActions.Add (DeleteActions.PreviousSubword);
			backDeleteActions.Add (DeleteActions.PreviousWord);
		}

		protected static Gdk.Key GetPairCaseKey (Gdk.Key key)
		{
			int caseDiff = Gdk.Key.a - Gdk.Key.A;
			if (key >= Gdk.Key.a && key <= Gdk.Key.z)
				return key - caseDiff;
			else if (key >= Gdk.Key.A && key <= Gdk.Key.Z)
				return key + caseDiff;
			else
				return key;
		}
		#endregion

		#region Public Members

		public ReplEditMode (TextEditor editor)
		{
			this.Editor = editor;
			editor.GetTextEditorData ().Paste += ReplEditMode_Paste;
		}

		public override void RemovedFromTextEditor ()
		{
			if (Data != null)
				Data.Paste -= ReplEditMode_Paste;
		}

		public void StartNewInput ()
		{
			Editor.Caret.Offset = Editor.Document.Length;
			if (Editor.Caret.Column != 0)
				InsertNewLine ();
			Editor.Document.AddMarker (Editor.Caret.Line - 1, sectionLine);
			ClearUndoRedo ();
			Editor.Document.SetNotDirtyState ();
			EditStartLine = Editor.Caret.Line;
			RepaintPrompts ();
		}

		public void RepaintPrompts ()
		{
			int lineNo = EditStartLine;
			LineSegment line = Editor.Document.GetLine (lineNo);
			line.ClearMarker ();
			Editor.Document.AddMarker (lineNo, PromptMarker.StartPrompt);
			for (lineNo++; lineNo < Editor.Document.LineCount; lineNo++) {
				line = Editor.Document.GetLine (lineNo);
				line.ClearMarker ();
				Editor.Document.AddMarker (lineNo, PromptMarker.ContinuousPrompt);
			}
		}

		public void InsertNewLine ()
		{
			//insert a new line without ruining last line's markers
			var doc = Editor.Document;
			Editor.Caret.Offset = doc.Length;
			var lineNo = doc.LineCount - 1;
			var line = doc.GetLine (lineNo);
			List<TextMarker> markers = new List<TextMarker>();
			foreach (var marker in line.Markers)
				markers.Add (marker);
			Editor.InsertAtCaret (Environment.NewLine);
			foreach (var marker in markers)
				doc.AddMarker (lineNo, marker);
		}

		public int EditStartOffset
		{
			get
			{
				return Editor.Document.GetLine (EditStartLine).Offset;
			}
		}

		public string Script
		{
			get
			{
				return Editor.Document.GetTextBetween (EditStartOffset, Editor.Document.Length);
			}
		}

		public int EditStartLine { get; protected set; }

		public event NewLinesHandler NewLines;

		#endregion

		#region Non-Public Members

		// TODO, I hack like this because the original Editor will become null in ReplEditMode_Paste
		private new TextEditor Editor { get; set; }

		private void ReplEditMode_Paste (int insertionOffset, string text)
		{
			int startLine = Editor.Document.OffsetToLineNumber (insertionOffset);
			int endLine = Editor.Caret.Line;
			if (endLine > startLine)
				OnNewLines (startLine, endLine - startLine);
		}

		protected override void HandleKeypress (Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
		{
			bool allInEditArea = SelectionAreaType == SelectionAreaType.InEditArea ||
				(SelectionAreaType == SelectionAreaType.Empty && Data.Caret.Offset >= EditStartOffset);

			int keyCode = GetKeyCode (key, modifier);
			int keyCode2 = GetKeyCode (GetPairCaseKey (key), modifier);
			
			bool caretJustBeforeReadolyArea = !Data.IsSomethingSelected && Caret.Offset == EditStartOffset;

			if (KeyBindings.ContainsKey (keyCode)) {
				CheckAndRunAction (KeyBindings [keyCode], !allInEditArea, caretJustBeforeReadolyArea);
			} else if (keyCode2 != keyCode && KeyBindings.ContainsKey (keyCode2)) {
				CheckAndRunAction (KeyBindings [keyCode2], !allInEditArea, caretJustBeforeReadolyArea);
			} else if (!char.IsControl ((char) unicodeKey)) {
				if (allInEditArea) {
					InsertCharacter (unicodeKey);
				} else if (!Data.IsSomethingSelected) {
					Caret.Offset = Editor.Document.Length;
					InsertCharacter (unicodeKey);
				}
			} else
				base.HandleKeypress (key, unicodeKey, modifier);
		}

		private void CheckAndRunAction (Action<TextEditorData> action,
			bool intersectsWithReadolyArea,
			bool caretJustBeforeReadolyArea)
		{
			if (writeActions.Contains (action) && intersectsWithReadolyArea)
				return;
			if (backDeleteActions.Contains (action) && caretJustBeforeReadolyArea)
				return;
			//if (action == MiscActions.InsertNewLinePreserveCaretPosition)
			//    // TODO we don't handle InsertNewLinePreserveCaretPosition for now.
			//    action = MiscActions.InsertNewLine;
			var selection = Editor.SelectionRange;
			int oldInsertPos = (selection != null) ?
				1:
				Caret.Line;
			RunAction (action);
			if (action == MiscActions.InsertNewLinePreserveCaretPosition ||
				action == MiscActions.InsertNewLine ||
				action == MiscActions.InsertNewLineAtEnd)
				OnNewLines (oldInsertPos, 1);
		}

		protected void OnNewLines (int startLine, int lineCount)
		{
			if (NewLines != null)
				NewLines (this, startLine, lineCount);
		}

		protected SelectionAreaType SelectionAreaType
		{
			get
			{
				if (!Data.IsSomethingSelected)
					return SelectionAreaType.Empty;
				var selectionRange = Editor.SelectionRange;
				if (selectionRange.Offset >= EditStartOffset)
					return SelectionAreaType.InEditArea;
				if (selectionRange.EndOffset < EditStartOffset)
					return SelectionAreaType.InReadonlyArea;
				return SelectionAreaType.InBothArea;
			}
		}

		protected void ClearUndoRedo ()
		{
			var undoStackFieldInfo = typeof (Document).GetField ("undoStack",
				BindingFlags.Instance | BindingFlags.NonPublic);
			((Stack<Document.UndoOperation>) undoStackFieldInfo.GetValue (Editor.Document)).Clear ();
			var redoStackFieldInfo = typeof (Document).GetField ("redoStack",
				BindingFlags.Instance | BindingFlags.NonPublic);
			((Stack<Document.UndoOperation>) redoStackFieldInfo.GetValue (Editor.Document)).Clear ();
		}

		private TextMarker sectionLine = new SeparatorMarker ();//UnderlineMarker (new Gdk.Color (0, 0, 63), 0, 20);

		#endregion
	}
}
