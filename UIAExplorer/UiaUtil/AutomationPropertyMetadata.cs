using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Automation;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	public class AutomationPropertyMetadata
	{
		private static Dictionary<AutomationProperty, AutomationPropertyMetadata> propertMetadata
			= new Dictionary<AutomationProperty, AutomationPropertyMetadata>();
		public static Dictionary<AutomationProperty, AutomationPropertyMetadata> PropertMetadata
		{
			get { return propertMetadata; }
		}

		private static List<AutomationPropertyCatelog> predefinedCatelogs
			= new List<AutomationPropertyCatelog>();
		public static List<AutomationPropertyCatelog> PredefinedCatelogs
		{
			get { return predefinedCatelogs; }
		}

		private static Dictionary<AutomationPattern, AutomationPropertyCatelog> patternCatelogs
			= new Dictionary<AutomationPattern, AutomationPropertyCatelog>();
		public static Dictionary<AutomationPattern, AutomationPropertyCatelog> PatternCatelogs
		{
			get { return patternCatelogs; }
		}

		private static AutomationPropertyCatelog unknownCatelog = null;
		public static AutomationPropertyMetadata GetMetadata (AutomationProperty property)
		{
			AutomationPropertyMetadata meta = null;
			if (!propertMetadata.TryGetValue(property, out meta))
			{
				meta = new AutomationPropertyMetadata(property, unknownCatelog);
				meta.Browsable = true;
				unknownCatelog.Properties.Add(property);
				propertMetadata.Add(property, meta);
			}
			return meta;
		}

		public static AutomationPattern GetPropertyPattern (AutomationProperty prop)
		{
			string propName = prop.ProgrammaticName;
			int dotIndex = propName.IndexOf ('.');
			if (dotIndex == -1)
				return null;
			string patternIdTypeName = propName.Substring (0, dotIndex);
			if (patternIdTypeName == "AutomationElementIdentifiers")
				return null;
			Type t = typeof(AutomationElementIdentifiers).Assembly.GetType (
				"System.Windows.Automation." + patternIdTypeName);
			if (t == null)
				return null;
			var filedInfo = t.GetField ("Pattern",
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Static);
			if (filedInfo == null)
				return null;
			return filedInfo.GetValue (null) as AutomationPattern;
		}

		public static AutomationProperty [] GetPatternProperties (AutomationPattern pattern)
		{
			AutomationPropertyCatelog catelog = null;
			if (patternCatelogs.TryGetValue (pattern, out catelog)) {
				return catelog.Properties.ToArray ();
			} else {
				Log.Error ("Cannot find {0} pattern", Automation.PatternName (pattern));
				return new AutomationProperty [0];
			}
		}

		static AutomationPropertyMetadata()
		{
			unknownCatelog = new AutomationPropertyCatelog("Unknown");

			var generalCatelog = new AutomationPropertyCatelog("General Accessibility");
			var stateCatelog = new AutomationPropertyCatelog("State");
			var idCatelog = new AutomationPropertyCatelog("Identification");
			var visCatelog = new AutomationPropertyCatelog("Visibility");
			var patternCheckCatelog = new AutomationPropertyCatelog("Pattern Availability");
			var miscCatelog = new AutomationPropertyCatelog("Misc");
			predefinedCatelogs.Add(generalCatelog);
			predefinedCatelogs.Add(stateCatelog);
			predefinedCatelogs.Add(idCatelog);
			predefinedCatelogs.Add(visCatelog);
			predefinedCatelogs.Add(patternCheckCatelog);
			predefinedCatelogs.Add(miscCatelog);
			
			AddPropertyMetadata(AutomationElement.AccessKeyProperty, generalCatelog, typeof(string), true);
			AddPropertyMetadata(AutomationElement.AcceleratorKeyProperty, generalCatelog, typeof(string), true);
			AddPropertyMetadata(AutomationElement.IsKeyboardFocusableProperty, generalCatelog, typeof(bool), true);

			AddPropertyMetadata(AutomationElement.IsEnabledProperty, stateCatelog, typeof(bool), true);
			AddPropertyMetadata(AutomationElement.HasKeyboardFocusProperty, stateCatelog, typeof(bool), true);

			AddPropertyMetadata(AutomationElement.ClassNameProperty, idCatelog, typeof(string), true);
			AddPropertyMetadata(AutomationElement.ControlTypeProperty, idCatelog, typeof(ControlType), true);
			AddPropertyMetadata(AutomationElement.CultureProperty, idCatelog, typeof(System.Globalization.CultureInfo), true);
			AddPropertyMetadata(AutomationElement.AutomationIdProperty, idCatelog, typeof(string), true);
			AddPropertyMetadata(AutomationElement.LocalizedControlTypeProperty, idCatelog, typeof(string), true);
			AddPropertyMetadata(AutomationElement.NameProperty, idCatelog, typeof(string), true);
			AddPropertyMetadata(AutomationElement.ProcessIdProperty, idCatelog, typeof(int), true);
			AddPropertyMetadata(AutomationElement.RuntimeIdProperty, idCatelog, typeof(int[]), true);
			AddPropertyMetadata(AutomationElement.IsPasswordProperty, idCatelog, typeof(bool), true);
			AddPropertyMetadata(AutomationElement.IsControlElementProperty, idCatelog, typeof(bool), true);
			AddPropertyMetadata(AutomationElement.IsContentElementProperty, idCatelog, typeof(bool), true);

			AddPropertyMetadata(AutomationElement.BoundingRectangleProperty, visCatelog, typeof(System.Windows.Rect), true);
			AddPropertyMetadata(AutomationElement.ClickablePointProperty, visCatelog, typeof(System.Windows.Point), true);
			AddPropertyMetadata(AutomationElement.IsOffscreenProperty, visCatelog, typeof(bool), true);

			AddPropertyMetadata(AutomationElement.IsDockPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsExpandCollapsePatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsGridPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsGridItemPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsInvokePatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsMultipleViewPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsRangeValuePatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsSelectionItemPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsSelectionPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsScrollPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsScrollItemPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsTablePatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsTableItemPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsTextPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsTogglePatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsTransformPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsValuePatternAvailableProperty, patternCheckCatelog, typeof(bool), false);
			AddPropertyMetadata(AutomationElement.IsWindowPatternAvailableProperty, patternCheckCatelog, typeof(bool), false);

			AddPropertyMetadata(AutomationElement.NativeWindowHandleProperty, miscCatelog, typeof(int), true);
			AddPropertyMetadata(AutomationElement.LabeledByProperty, miscCatelog, typeof(AutomationElement), true);
			AddPropertyMetadata(AutomationElement.OrientationProperty, miscCatelog, typeof(OrientationType), false);
			AddPropertyMetadata(AutomationElement.FrameworkIdProperty, miscCatelog, typeof(string), false);
			AddPropertyMetadata(AutomationElement.ItemTypeProperty, miscCatelog, typeof(string), false);
			AddPropertyMetadata(AutomationElement.ItemStatusProperty, miscCatelog, typeof(string), false);
			AddPropertyMetadata(AutomationElement.IsRequiredForFormProperty, miscCatelog, typeof(bool), false);

			//DockPattern
			var dockPatternCatelog = new AutomationPropertyCatelog("Dock Pattern");
			patternCatelogs.Add(DockPattern.Pattern, dockPatternCatelog);
			AddPropertyMetadata(DockPattern.DockPositionProperty, dockPatternCatelog, typeof(DockPosition), true);

			//ExpandCollapsePattern
			var expandCollapsePatternCatelog = new AutomationPropertyCatelog("ExpandCollapse Pattern");
			patternCatelogs.Add(ExpandCollapsePattern.Pattern, expandCollapsePatternCatelog);
			AddPropertyMetadata(ExpandCollapsePattern.ExpandCollapseStateProperty, expandCollapsePatternCatelog, typeof(ExpandCollapseState), true);

			//GridItemPattern
			var gridItemPatternCatelog = new AutomationPropertyCatelog("GridItem Pattern");
			patternCatelogs.Add(GridItemPattern.Pattern, gridItemPatternCatelog);
			AddPropertyMetadata(GridItemPattern.RowProperty, gridItemPatternCatelog, typeof(int), true);
			AddPropertyMetadata(GridItemPattern.ColumnProperty, gridItemPatternCatelog, typeof(int), true);
			AddPropertyMetadata(GridItemPattern.RowSpanProperty, gridItemPatternCatelog, typeof(int), true);
			AddPropertyMetadata(GridItemPattern.ColumnSpanProperty, gridItemPatternCatelog, typeof(int), true);
			AddPropertyMetadata(GridItemPattern.ContainingGridProperty, gridItemPatternCatelog, typeof(AutomationElement), true);

			//GridPattern
			var gridPatternCatelog = new AutomationPropertyCatelog("Grid Pattern");
			patternCatelogs.Add(GridPattern.Pattern, gridPatternCatelog);
			AddPropertyMetadata(GridPattern.RowCountProperty, gridPatternCatelog, typeof(int), true);
			AddPropertyMetadata(GridPattern.ColumnCountProperty, gridPatternCatelog, typeof(int), true);

			//InvokePattern
			var invokePatternCatelog = new AutomationPropertyCatelog("Invoke Pattern");
			patternCatelogs.Add(InvokePattern.Pattern, invokePatternCatelog);

			//MultipleViewPattern
			var multipleViewPatternCatelog = new AutomationPropertyCatelog("MultipleView Pattern");
			patternCatelogs.Add(MultipleViewPattern.Pattern, multipleViewPatternCatelog);
			AddPropertyMetadata(MultipleViewPattern.CurrentViewProperty, multipleViewPatternCatelog, typeof(int), true);
			AddPropertyMetadata(MultipleViewPattern.SupportedViewsProperty, multipleViewPatternCatelog, typeof(int[]), true);

			//RangeValuePattern
			var rangeValuePatternCatelog = new AutomationPropertyCatelog("RangeValue Pattern");
			patternCatelogs.Add(RangeValuePattern.Pattern, rangeValuePatternCatelog);
			AddPropertyMetadata(RangeValuePattern.ValueProperty, rangeValuePatternCatelog, typeof(double), true);
			AddPropertyMetadata(RangeValuePattern.IsReadOnlyProperty, rangeValuePatternCatelog, typeof(bool), true);
			AddPropertyMetadata(RangeValuePattern.MinimumProperty, rangeValuePatternCatelog, typeof(double), true);
			AddPropertyMetadata(RangeValuePattern.MaximumProperty, rangeValuePatternCatelog, typeof(double), true);
			AddPropertyMetadata(RangeValuePattern.LargeChangeProperty, rangeValuePatternCatelog, typeof(double), true);
			AddPropertyMetadata(RangeValuePattern.SmallChangeProperty, rangeValuePatternCatelog, typeof(double), true);

			//ScrollPattern
			var scrollPatternCatelog = new AutomationPropertyCatelog("Scroll Pattern");
			patternCatelogs.Add(ScrollPattern.Pattern, scrollPatternCatelog);
			AddPropertyMetadata(ScrollPattern.HorizontalScrollPercentProperty, scrollPatternCatelog, typeof(double), true);
			AddPropertyMetadata(ScrollPattern.HorizontalViewSizeProperty, scrollPatternCatelog, typeof(double), true);
			AddPropertyMetadata(ScrollPattern.VerticalScrollPercentProperty, scrollPatternCatelog, typeof(double), true);
			AddPropertyMetadata(ScrollPattern.VerticalViewSizeProperty, scrollPatternCatelog, typeof(double), true);
			AddPropertyMetadata(ScrollPattern.HorizontallyScrollableProperty, scrollPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(ScrollPattern.VerticallyScrollableProperty, scrollPatternCatelog, typeof(bool), true);

			//ScrollItemPattern
			var scrollItemPatternCatelog = new AutomationPropertyCatelog("ScrollItem Pattern");
			patternCatelogs.Add(ScrollItemPattern.Pattern, scrollItemPatternCatelog);

			//SelectionItemPattern
			var selectionItemPatternCatelog = new AutomationPropertyCatelog("SelectionItem Pattern");
			patternCatelogs.Add(SelectionItemPattern.Pattern, selectionItemPatternCatelog);
			AddPropertyMetadata(SelectionItemPattern.IsSelectedProperty, selectionItemPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(SelectionItemPattern.SelectionContainerProperty, selectionItemPatternCatelog, typeof(AutomationElement), true);

			//SelectionPattern
			var selectionPatternCatelog = new AutomationPropertyCatelog("Selection Pattern");
			patternCatelogs.Add(SelectionPattern.Pattern, selectionPatternCatelog);
			AddPropertyMetadata(SelectionPattern.SelectionProperty, selectionPatternCatelog, typeof(AutomationElement[]), true);
			AddPropertyMetadata(SelectionPattern.CanSelectMultipleProperty, selectionPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(SelectionPattern.IsSelectionRequiredProperty, selectionPatternCatelog, typeof(bool), true);

			//TableItemPattern
			var tableItemPatternCatelog = new AutomationPropertyCatelog("TableItem Pattern");
			patternCatelogs.Add(TableItemPattern.Pattern, tableItemPatternCatelog);
			AddPropertyMetadata(TableItemPattern.RowHeaderItemsProperty, tableItemPatternCatelog, typeof(AutomationElement[]), true);
			AddPropertyMetadata(TableItemPattern.ColumnHeaderItemsProperty, tableItemPatternCatelog, typeof(AutomationElement), true);

			//TablePattern
			var tablePatternCatelog = new AutomationPropertyCatelog("Table Pattern");
			patternCatelogs.Add(TablePattern.Pattern, tablePatternCatelog);
			AddPropertyMetadata(TablePattern.RowHeadersProperty, tablePatternCatelog, typeof(AutomationElement[]), true);
			AddPropertyMetadata(TablePattern.ColumnHeadersProperty, tablePatternCatelog, typeof(AutomationElement[]), true);
			AddPropertyMetadata(TablePattern.RowOrColumnMajorProperty, tablePatternCatelog, typeof(RowOrColumnMajor), true);

			//TextPattern
			var textPatternCatelog = new AutomationPropertyCatelog("Text Pattern");
			patternCatelogs.Add(TextPattern.Pattern, textPatternCatelog);

			//TogglePattern
			var togglePatternCatelog = new AutomationPropertyCatelog("Toggle Pattern");
			patternCatelogs.Add(TogglePattern.Pattern, togglePatternCatelog);
			AddPropertyMetadata(TogglePattern.ToggleStateProperty, togglePatternCatelog, typeof(ToggleState), true);

			//TransformPattern
			var transformPatternCatelog = new AutomationPropertyCatelog("Transform Pattern");
			patternCatelogs.Add(TransformPattern.Pattern, transformPatternCatelog);
			AddPropertyMetadata(TransformPattern.CanMoveProperty, transformPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(TransformPattern.CanResizeProperty, transformPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(TransformPattern.CanRotateProperty, transformPatternCatelog, typeof(bool), true);

			//ValuePattern
			var valuePatternCatelog = new AutomationPropertyCatelog("Value Pattern");
			patternCatelogs.Add(ValuePattern.Pattern, valuePatternCatelog);
			AddPropertyMetadata(ValuePattern.ValueProperty, valuePatternCatelog, typeof(string), true);
			AddPropertyMetadata(ValuePattern.IsReadOnlyProperty, valuePatternCatelog, typeof(string), true);

			//WindowPattern
			var windowPatternCatelog = new AutomationPropertyCatelog("Window Pattern");
			patternCatelogs.Add(WindowPattern.Pattern, windowPatternCatelog);
			AddPropertyMetadata(WindowPattern.CanMaximizeProperty, windowPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(WindowPattern.CanMinimizeProperty, windowPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(WindowPattern.IsModalProperty, windowPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(WindowPattern.IsTopmostProperty, windowPatternCatelog, typeof(bool), true);
			AddPropertyMetadata(WindowPattern.WindowInteractionStateProperty, windowPatternCatelog, typeof(WindowInteractionState), true);
			AddPropertyMetadata(WindowPattern.WindowVisualStateProperty, windowPatternCatelog, typeof(WindowVisualState), true);
		}

		private static void AddPropertyMetadata(AutomationProperty property,
			AutomationPropertyCatelog catelog,
			Type t, bool browsable)
		{
			AddPropertyMetadata(property, catelog, t, browsable, property.ProgrammaticName);
		}

		private static void AddPropertyMetadata (AutomationProperty property,
			AutomationPropertyCatelog catelog,
			Type t, bool browsable,
			string description)
		{
			var meta = new AutomationPropertyMetadata(property, catelog);
			meta.Type = t;
			meta.Browsable = browsable;
			meta.Description = description;
			catelog.Properties.Add(property);
			propertMetadata.Add(property, meta);
		}

		public AutomationPropertyMetadata(AutomationProperty property,
			AutomationPropertyCatelog catelog)
		{
			this.Property = property;
			this.Catelog = catelog;
		}

		public AutomationProperty Property { get; private set; }
		public AutomationPropertyCatelog Catelog { get; private set; }
		public string Description { get; private set; }
		public Type Type { get; private set; }
		public bool Browsable { get; set; }
		
		public string DisplayName
		{
			get {
				return Property.ProgrammaticName.Substring(Property.ProgrammaticName.LastIndexOf('.') + 1);
			}
		}
	}
}
