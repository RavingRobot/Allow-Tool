using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Settings;
using Verse;

namespace AllowTool.Context;

public struct ContextMenuProvider
{
	private readonly BaseContextMenuEntry[] entries;

	public Type HandledDesignatorType { get; }

	public bool HasCustomEnabledEntries
	{
		get
		{
			for (int i = 0; i < entries.Length; i++)
			{
				if (entries[i].Enabled)
				{
					return true;
				}
			}
			return false;
		}
	}

	public ContextMenuProvider(Type handledType, params BaseContextMenuEntry[] entries)
	{
		HandledDesignatorType = handledType;
		this.entries = entries;
	}

	public void OpenContextMenu(Designator designator)
	{
		List<FloatMenuOption> list = (from e in entries
			where e.Enabled
			select e.MakeMenuOption(designator)).Concat(designator.RightClickFloatMenuOptions).ToList();
		if (list.Count > 0)
		{
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}

	public bool TryInvokeHotkeyAction(Designator designator)
	{
		BaseContextMenuEntry baseContextMenuEntry = entries.FirstOrDefault((BaseContextMenuEntry e) => e.Enabled);
		if (baseContextMenuEntry != null)
		{
			baseContextMenuEntry.ActivateAndHandleResult(designator);
			return true;
		}
		return false;
	}

	public IEnumerable<SettingHandle<bool>> RegisterEntryHandles(ModSettingsPack pack)
	{
		return entries.Select((BaseContextMenuEntry e) => e.RegisterSettingHandle(pack));
	}
}
