using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using MonoDevelop.Components.PropertyGrid;
using Mono.Accessibility.UIAExplorer.Discriptors;
using Gtk;
using System.Reflection;

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
			this.Clicked += HandleClicked;
		}

		void HandleClicked (object sender, EventArgs e)
		{
			var parameters = invoke.Method.GetParameters ();
			try {
				var patternObj = invoke.Element.GetCurrentPattern (invoke.Pattern);
				if (parameters.Length == 0) {
					invoke.Method.Invoke (patternObj, new object [0]);
				} else if (IsParametersTooComplicate (parameters)) {
					Message.Info ("The parameters of this method is too complicate to input," +
						"you're suggested to invoke this method with scripting");
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
					grid.CurrentObject = null;
					dialog.Destroy ();
					if (response == ResponseType.Ok) {
						// TODO, replace this dictionary with a list<annonymous_struct {int, string}>
						var outParams = new Dictionary<int, string> ();
						var paraValuesList = new List<object> ();
						for (int i = 0; i < parameterSet.Parameters.Count; i++) {
							var para = parameterSet.Parameters [i];
							if (para.IsOut)
								outParams.Add (i, para.Name);
							paraValuesList.Add (para.ParameterValue);
						}
						object [] paraValues = paraValuesList.ToArray ();

						object retVal = invoke.Method.Invoke (patternObj, paraValues);
						var sb = new System.Text.StringBuilder ();
						if (retVal != null) {
							sb.AppendFormat ("Return Value: {0}", retVal);
							sb.AppendLine ();
						}
						foreach (var outParam in outParams) {
							sb.AppendFormat ("{0}: {1}", outParam.Value, paraValues [outParam.Key]);
							sb.AppendLine ();
						}
						if (sb.Length > 0)
							Message.Info (sb.ToString ());
					}
				}
			} catch (Exception ex) {
				if (ex is TargetInvocationException
					&& ex.InnerException != null)
					ex = ex.InnerException;
				Message.Error (ex.ToString());
			}
		}

		static bool IsParametersTooComplicate (ParameterInfo[] parameters)
		{
			bool canHandle = true;
			foreach (ParameterInfo para in parameters) {
				var type = para.ParameterType;
				if (!(type.IsPrimitive || type.IsEnum || type == typeof(string))) {
					canHandle = false;
					break;
				}
			}
			return !canHandle;
		}


		private object val = null;
		public object Value { 
			get { return val; } 
			set { val = value; }
		}

		public event EventHandler ValueChanged;
	}
}
