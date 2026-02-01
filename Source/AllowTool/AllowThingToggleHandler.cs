using System.Collections.Generic;
using AllowTool.Context;
using Verse;

namespace AllowTool;

internal static class AllowThingToggleHandler
{
	private static Designator allowDesignatorStandIn = new Designator_Allow();

	private static Designator forbidDesignatorStandIn = new Designator_Forbid();

	public static void EnhanceStockAllowToggle(Command_Toggle toggle)
	{
		Designator designator = (toggle.isActive() ? allowDesignatorStandIn : forbidDesignatorStandIn);
		DesignatorContextMenuController.RegisterReverseDesignatorPair(designator, toggle);
		AddIconReplacementSupport(toggle, designator);
	}

	public static IEnumerable<Designator> GetImpliedReverseDesignators()
	{
		yield return allowDesignatorStandIn;
		yield return forbidDesignatorStandIn;
	}

	public static void ReinitializeDesignators()
	{
		allowDesignatorStandIn = new Designator_Allow();
		forbidDesignatorStandIn = new Designator_Forbid();
	}

	private static void AddIconReplacementSupport(Command_Toggle toggle, Designator standInDesignator)
	{
		if (AllowToolController.Instance.Handles.ReplaceIconsSetting.Value)
		{
			toggle.icon = standInDesignator.icon;
		}
	}
}
