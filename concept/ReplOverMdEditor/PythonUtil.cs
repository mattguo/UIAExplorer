using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ReplOverMdEditor
{
	public enum PythonElementType
	{
		Identifier,
		Spacing,
		Operator,
		Comment,
		SingleQuotedString,
		DoubleQuotedString,
		TripleSingleQuotedString,
		TripleDoubleQuotedString
	}

	public enum PythonEndState
	{
		Normal,
		InComment,
		InSingleQuotedString,
		InDoubleQuotedString,
		InTripleSingleQuotedString,
		InTripleDoubleQuotedString
	}

	public struct PythonBlock
	{
		public PythonElementType Type;
		public int StartIndex;
		public string Text;

		public int EndIndex { get { return StartIndex + Text.Length; } }
	}

	[Serializable]
	public class ParseException : Exception
	{
		public ParseException ()
		{
		}

		public ParseException (string errorMessage)
			: base (errorMessage)
		{
		}

		public ParseException (string errorMessage, Exception innerEx)
			: base (errorMessage, innerEx)
		{
		}

		public string Script { get; set; }
		public int Position { get; set; }
	}

	public static class PythonUtil
	{
		private static char [] operators = "`~!@$%^&*()-+=|/?:;,.<>[]".ToCharArray ();
		private static char [] blankChars = " \t\r\n".ToCharArray ();

		public static List<PythonBlock> SplitBlocks (string script, out PythonEndState endState)
		{
			List<PythonBlock> blocks = new List<PythonBlock> ();
			if (string.IsNullOrEmpty (script)) {
				endState = PythonEndState.Normal;
				return blocks;
			}

			int offset = 0;
			string remainingScript = script;
			PythonElementType currentType;
			while (offset < script.Length) {
				char ch = remainingScript [0];
				if (ch == '_' || (ch >= 'a' && ch <= 'z') ||
					(ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
					currentType = PythonElementType.Identifier;
				else if (Array.IndexOf (operators, ch) != -1)
					currentType = PythonElementType.Operator;
				else if (Array.IndexOf (blankChars, ch) != -1)
					currentType = PythonElementType.Spacing;
				else if (ch == '#')
					currentType = PythonElementType.Comment;
				else if (remainingScript.StartsWith ("'''"))
					currentType = PythonElementType.TripleSingleQuotedString;
				else if (remainingScript.StartsWith ("\"\"\""))
					currentType = PythonElementType.TripleDoubleQuotedString;
				else if (ch == '\'')
					currentType = PythonElementType.SingleQuotedString;
				else if (ch == '"')
					currentType = PythonElementType.DoubleQuotedString;
				else {
					// TODO make a ParseException
					var parseError = new ParseException ("Unknown character");
					parseError.Position = offset;
					parseError.Script = remainingScript;
					throw parseError;
				}
				int blockLength = 0;
				switch (currentType) {
					case PythonElementType.Identifier:
						var m = Regex.Match (remainingScript, "[_a-zA-Z0-9]+");
						blockLength = m.Length;
						break;
					case PythonElementType.Operator:
						blockLength++;
						while (blockLength < remainingScript.Length &&
							Array.IndexOf (operators, remainingScript [blockLength]) != -1)
							blockLength++;
						break;
					case PythonElementType.Spacing:
						blockLength++;
						while (blockLength < remainingScript.Length &&
							Array.IndexOf (blankChars, remainingScript [blockLength]) != -1)
							blockLength++;
						break;
					case PythonElementType.Comment:
						blockLength = remainingScript.IndexOfAny (new char [] { '\r', '\n' });
						if (blockLength == -1) {
							endState = PythonEndState.InComment;
							return blocks;
						}
						break;
					case PythonElementType.SingleQuotedString:
						while (true) {
							blockLength = remainingScript.IndexOf ('\'', blockLength + 1);
							if (blockLength == -1) {
								endState = PythonEndState.InSingleQuotedString;
								return blocks;
							}
							if (remainingScript [blockLength - 1] != '\\') {
								blockLength++;
								break;
							}
						}
						break;
					case PythonElementType.DoubleQuotedString:
						while (true) {
							blockLength = remainingScript.IndexOf ('"', blockLength + 1);
							if (blockLength == -1) {
								endState = PythonEndState.InDoubleQuotedString;
								return blocks;
							}
							if (remainingScript [blockLength - 1] != '\\') {
								blockLength++;
								break;
							}
						}
						break;
					case PythonElementType.TripleSingleQuotedString:
						while (true) {
							blockLength = remainingScript.IndexOf ("'''", blockLength + 3);
							if (blockLength == -1) {
								endState = PythonEndState.InTripleSingleQuotedString;
								return blocks;
							}
							if (remainingScript [blockLength - 1] != '\\') {
								blockLength += 3;
								break;
							} else if (remainingScript [blockLength + 3] == '\'') {
								blockLength += 4;
								break;
							}
						}
						break;
					case PythonElementType.TripleDoubleQuotedString:
						while (true) {
							blockLength = remainingScript.IndexOf ("\"\"\"", blockLength + 3);
							if (blockLength == -1) {
								endState = PythonEndState.InTripleDoubleQuotedString;
								return blocks;
							}
							if (remainingScript [blockLength - 1] != '\\') {
								blockLength += 3;
								break;
							} else if (remainingScript [blockLength + 3] == '\"') {
								blockLength += 4;
								break;
							}
						}
						break;
				}
				PythonBlock block = new PythonBlock {
					StartIndex = offset,
					Text = remainingScript.Substring (0, blockLength),
					Type = currentType
				};
				blocks.Add (block);
				offset += blockLength;
				remainingScript = remainingScript.Substring (blockLength);
			}
			endState = PythonEndState.Normal;
			return blocks;
		}

		public static string GetRemainingScript (string script, List<PythonBlock> parsedBlocks)
		{
			var lastBlock = parsedBlocks [parsedBlocks.Count - 1];
			int endOfLastBlock = lastBlock.EndIndex;
			return endOfLastBlock < script.Length ? script.Substring (endOfLastBlock) : "";
		}
	}
}
