using System;
using System.ComponentModel;
using MonoDevelop.Components.PropertyGrid;
using Mono.Accessibility.UIAExplorer.Discriptors;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{
	[PropertyEditorType (typeof (PatternMethodInvoke))]
	public class PatternMethodCell : PropertyEditorCell 
	{
		private PatternMethodInvoke invoke;

		public PatternMethodCell (PatternMethodInvoke invoke)
		{
			this.invoke = invoke;
		}

		public override void GetSize (int availableWidth, out int width, out int height)
		{
			width = 20;
			height = 20;
		}

		public override void Render (Gdk.Drawable window, Gdk.Rectangle bounds, Gtk.StateType state)
		{
			//TODO Draw something to indicate "Run"
		}

		protected override IPropertyEditor CreateEditor (Gdk.Rectangle cell_area, Gtk.StateType state)
		{
			return new PatternMethodInvoker (invoke);
		}
	}

	public class PatternMethodInvoker : Gtk.Button, IPropertyEditor 
	{
		private PatternMethodInvoke invoke;

		public PatternMethodInvoker (PatternMethodInvoke invoke)
		{
			this.invoke = invoke;
		}

		public void Initialize (EditSession session)
		{
			this.Label = "Run";
			this.SetSizeRequest (20, 20);
			this.Clicked += HandleHandleClicked;
		}

		void HandleHandleClicked (object sender, EventArgs e)
		{
			var parameters = invoke.Method.GetParameters ();
			try {
				var patternObj = invoke.Element.GetCurrentPattern (invoke.Pattern);
				if (parameters.Length == 0) {
					invoke.Method.Invoke (patternObj, new object [0]);
				} else {
					//????? TODO get parameter inputs with a dialog...
					Message.Warn ("Invoking methods with parameters is not implemented yet");
				}
			} catch (Exception ex) {
				Message.Error ("Can't invoke this method: {0}", ex);
			}
		}

		private object val = null;
		public object Value { 
			get { return val; } 
			set { val = value; }
		}

		public event EventHandler ValueChanged;
	}
}
