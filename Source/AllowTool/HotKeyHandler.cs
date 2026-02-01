using System.Collections.Generic;
using System.Linq;
using AllowTool.Context;
using UnityEngine;
using Verse;

namespace AllowTool;

public class HotKeyHandler
{
	private struct HotkeyListener(Designator designator, KeyBindingDef hotKey)
	{
		public readonly Designator designator = designator;

		public readonly KeyBindingDef hotKey = hotKey;
	}

	private readonly List<HotkeyListener> activeListeners = new List<HotkeyListener>();

	public void OnGUI()
	{
		if (Event.current.type == EventType.KeyDown)
		{
			CheckForHotkeyPresses();
		}
	}

	public void RebindAllDesignators()
	{
		activeListeners.Clear();
		IEnumerable<Designator> enumerable = from d in AllowToolUtility.EnumerateResolvedDirectDesignators()
			where d is IGlobalHotKeyProvider globalHotKeyProvider && globalHotKeyProvider.GlobalHotKey != null
			select d;
		foreach (Designator item in enumerable)
		{
			activeListeners.Add(new HotkeyListener(item, ((IGlobalHotKeyProvider)item).GlobalHotKey));
		}
	}

	private void CheckForHotkeyPresses()
	{
		if (Find.CurrentMap == null || Event.current.keyCode == KeyCode.None)
		{
			return;
		}
		if (AllowToolDefOf.ToolContextMenuAction.JustPressed)
		{
			DesignatorContextMenuController.ProcessContextActionHotkeyPress();
		}
		if (!AllowToolController.Instance.Handles.GlobalHotkeysSetting)
		{
			return;
		}
		for (int i = 0; i < activeListeners.Count; i++)
		{
			if (activeListeners[i].hotKey.JustPressed && activeListeners[i].designator.Visible)
			{
				Find.DesignatorManager.Select(activeListeners[i].designator);
				break;
			}
		}
	}
}
