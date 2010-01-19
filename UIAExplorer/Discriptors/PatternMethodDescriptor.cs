using System;
using System.Windows.Automation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Mono.Accessibility.UIAExplorer.UserInterface;

namespace Mono.Accessibility.UIAExplorer.Discriptors
{
	public struct PatternMethodInvoke
	{
		public AutomationElement Element;
		public AutomationPattern Pattern;
		public MethodInfo Method;

		public override int GetHashCode()
		{
			return Pattern.GetHashCode () ^ Method.GetHashCode ();
		}

		public override bool Equals(object obj)
		{
			try {
				var target = (PatternMethodInvoke) obj;
				return Pattern.Equals(target.Pattern) && Method.Equals (target.Method);
			} catch {
				return false;
			}
		}
	}
	
	public class PatternMethodDescriptor : PropertyDescriptor
	{
		private PatternMethodInvoke invoke;

		public PatternMethodDescriptor (
			AutomationElement element,
			AutomationPattern pattern,
			MethodInfo mi)
			: base (pattern.ProgrammaticName, new Attribute[0])
		{
			invoke.Element = element;
			invoke.Pattern = pattern;
			invoke.Method = mi;
		}

		public override object GetValue (object component)
		{
			return invoke;
		}

		public override bool IsReadOnly {
			get {
				return false;
			}
		}
		
		public override string Category {
			get {
				return Automation.PatternName (invoke.Pattern) + " Pattern";
			}
		}

		public override string Description {
			get {
				return string.Format ("{0}Pattern.{1} method", Automation.PatternName (invoke.Pattern), invoke.Method.Name );
			}
		}
		public override bool IsBrowsable
		{
			get
			{
				return true;
			}
		}

		public override string DisplayName {
			get {
				return invoke.Method.Name + " Method";
			}
		}

		public override Type PropertyType {
			get {
				return typeof (PatternMethodInvoke);
			}
		}

		public override Type ComponentType {
			get {
				return typeof (AutomationElementDescriptor);
			}
		}

		public override void SetValue (object component, object value)
		{
			//throw new System.NotImplementedException ();
		}

		public override void ResetValue (object component)
		{
			//throw new System.NotImplementedException();
		}

		public override bool CanResetValue (object component)
		{
			return false;
		}

		public override bool ShouldSerializeValue (object component)
		{
			return false;
		}

		public override int GetHashCode()
		{
			return invoke.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var target = obj as PatternMethodDescriptor;
			if (target != null)
				return invoke.Equals(target.invoke);
			else
				return false;
		}

		public override object GetEditor (Type editorBaseType)
		{
			if (editorBaseType == typeof (MonoDevelop.Components.PropertyGrid.PropertyEditorCell))
				return new PatternMethodCell (invoke);
			else
				return base.GetEditor (editorBaseType);
		}
	}
}
