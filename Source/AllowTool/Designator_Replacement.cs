using System.Collections.Generic;
using Verse;

namespace AllowTool;

public abstract class Designator_Replacement : Designator_SelectableThings
{
	protected Designator replacedDesignator;

	private bool InheritReplacedDesignatorIcon => !AllowToolController.Instance.Handles.ReplaceIconsSetting;

	public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
	{
		get
		{
			foreach (FloatMenuOption rightClickFloatMenuOption in base.RightClickFloatMenuOptions)
			{
				yield return rightClickFloatMenuOption;
			}
			if (replacedDesignator == null)
			{
				yield break;
			}
			foreach (FloatMenuOption rightClickFloatMenuOption2 in replacedDesignator.RightClickFloatMenuOptions)
			{
				yield return rightClickFloatMenuOption2;
			}
		}
	}

	protected override void ResolveIcon()
	{
		if (replacedDesignator != null && InheritReplacedDesignatorIcon)
		{
			icon = replacedDesignator.icon;
		}
		else
		{
			base.ResolveIcon();
		}
	}
}
