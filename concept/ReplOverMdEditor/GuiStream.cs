using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace ReplOverMdEditor
{
	class GuiStream : Stream
	{
		string kind;
		Action<string> callback;

		public GuiStream (Action<string> cb)
		{
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
			callback (Encoding.UTF8.GetString (buffer, offset, count));
		}
	}
}
