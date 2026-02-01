using RimWorld;
using Verse;

namespace AllowTool.Context;

public class MenuEntry_FinishOffAll : BaseContextMenuEntry
{
	protected override string SettingHandleSuffix => "finishOffAll";

	protected override string BaseTextKey => "Designator_context_finish";

	protected override ThingRequestGroup DesignationRequestGroup => ThingRequestGroup.Pawn;

	public override ActivationResult Activate(Designator designator, Map map)
	{
		int num = 0;
		bool flag = false;
		foreach (Thing item in map.listerThings.ThingsInGroup(DesignationRequestGroup))
		{
			if (BaseContextMenuEntry.ThingIsValidForDesignation(item) && designator.CanDesignateThing(item).Accepted)
			{
				designator.DesignateThing(item);
				num++;
				if (AllowToolUtility.PawnIsFriendly(item))
				{
					flag = true;
				}
			}
		}
		if (num > 0 && flag)
		{
			Messages.Message("Designator_context_finish_allies".Translate(num), MessageTypeDefOf.CautionInput);
		}
		return ActivationResult.FromCount(num, BaseTextKey);
	}
}
