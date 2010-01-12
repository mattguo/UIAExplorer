using System;
using Gtk;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{
	public interface IDockPad
	{
		Widget Control { get; }
		string Title { get; }
	}
}
