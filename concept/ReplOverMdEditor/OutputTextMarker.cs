using System;
using System.Collections.Generic;
using System.Text;
using Mono.TextEditor;

namespace ReplOverMdEditor
{
	public class OutputTextMarker : TextMarker
	{
		public static OutputTextMarker Error = new OutputTextMarker (new Gdk.Color (255, 0, 0));
		public static OutputTextMarker Log = new OutputTextMarker (new Gdk.Color (0, 127, 0));
		public static OutputTextMarker Info = new OutputTextMarker (new Gdk.Color (0, 0, 127));

		public OutputTextMarker (Gdk.Color color)
		{
			this.Color = color;
		}

		public override ChunkStyle GetStyle (ChunkStyle baseStyle)
		{
			ChunkStyle style = new ChunkStyle (baseStyle);
			style.Color = Color;
			style.ChunkProperties &= ~ChunkProperties.Bold;
			style.ChunkProperties &= ~ChunkProperties.Italic;
			return style;

		}

		public virtual Gdk.Color Color { get; set; }
	}
}
