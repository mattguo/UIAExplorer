using System;
using System.Windows.Automation;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mono.Accessibility.UIAExplorer.Discriptors
{
	public class PatternAvailableIndicator : PropertyDescriptor
	{
		private AutomationPattern pattern = null;

		public PatternAvailableIndicator(
			AutomationPattern pattern)
			: base (pattern.ProgrammaticName, new Attribute[0])
		{
			this.pattern = pattern;
		}


		public override object GetValue (object component)
		{
			return true;
		}

		public override bool IsReadOnly {
			get {
				return true;
			}
		}
		
		public override string Category {
			get {
				return Automation.PatternName (pattern);
			}
		}

		public override string Description {
			get {
				return pattern.ProgrammaticName;
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
				return "** Supported **";
			}
		}

		public override Type PropertyType {
			get {
				return typeof (bool);
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
			return true;
		}

		public override int GetHashCode()
		{
			return pattern.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var target = obj as PatternAvailableIndicator;
			if (target != null)
				return pattern.Equals(target.pattern);
			else
				return false;
		}
	}
}
