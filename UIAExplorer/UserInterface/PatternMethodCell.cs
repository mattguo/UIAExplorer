using System;
using System.Linq;
using System.ComponentModel;
using MonoDevelop.Components.PropertyGrid;
using Mono.Accessibility.UIAExplorer.Discriptors;
using Gtk;

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
					Dialog dialog = new Dialog ("Set Method Parameters", null,
						DialogFlags.Modal | DialogFlags.DestroyWithParent,
						Gtk.Stock.Ok, ResponseType.Ok,
						Gtk.Stock.Cancel, ResponseType.Cancel);
					var parameterSet = new ParameterSetDescriptor (invoke.Method);
					PropertyGrid grid = new PropertyGrid();
					grid.CurrentObject = parameterSet;
					grid.ShowHelp = false;
					dialog.VBox.PackStart (grid, true, true, 0);
					grid.ShowAll ();
					dialog.SetSizeRequest (360, 420);
					ResponseType response = (ResponseType) dialog.Run ();
					dialog.Destroy ();
					if (response == ResponseType.Ok) {
						object [] parameterValues = parameterSet.Parameters.Select(p => p.ParameterValue).ToArray ();
						//output return value and out paras.
						invoke.Method.Invoke (patternObj, parameterValues);
					}
				}
			} catch (Exception ex) {
				if (ex is System.Reflection.TargetInvocationException
					&& ex.InnerException != null)
					ex = ex.InnerException;
				Message.Error ("{0}:{1}{2}",
					ex.GetType().Name,
					Environment.NewLine,
					ex.Message);
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
