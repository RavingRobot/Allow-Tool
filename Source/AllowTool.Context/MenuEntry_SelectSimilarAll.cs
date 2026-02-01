using System;
using System.Collections.Generic;
using Verse;

namespace AllowTool.Context;

public class MenuEntry_SelectSimilarAll : BaseContextMenuEntry
{
	protected override string BaseTextKey => "Designator_context_similar";

	protected override string SettingHandleSuffix => "selectSimilarAll";

	public override ActivationResult Activate(Designator designator, Map map)
	{
		return SelectSimilarWithFilter(designator, map, BaseMessageKey, BaseMessageKey);
	}

	public static ActivationResult SelectSimilarWithFilter(Designator designator, Map map, string successMessageKey, string failureMessageKey, Predicate<Thing> filter = null)
	{
		Designator_SelectSimilar designator_SelectSimilar = (Designator_SelectSimilar)designator;
		designator_SelectSimilar = (Designator_SelectSimilar)designator_SelectSimilar.PickUpReverseDesignator();
		if (Find.Selector.NumSelected == 0)
		{
			return ActivationResult.Failure(failureMessageKey);
		}
		designator_SelectSimilar.ReindexSelectionConstraints();
		List<Thing> list = new List<Thing>();
		foreach (Thing allThing in map.listerThings.AllThings)
		{
			if (allThing != null && (filter == null || filter(allThing)) && designator_SelectSimilar.CanDesignateThing(allThing).Accepted)
			{
				list.Add(allThing);
			}
		}
		IntVec3 cameraCenter = Current.CameraDriver.MapPosition;
		list.SortBy((Thing t) => t.Position.DistanceTo(cameraCenter));
		int num = 0;
		bool flag = false;
		foreach (Thing item in list)
		{
			if (!designator_SelectSimilar.SelectionLimitAllowsAdditionalThing())
			{
				flag = true;
				break;
			}
			if (designator_SelectSimilar.TrySelectThing(item))
			{
				num++;
			}
		}
		return flag ? ActivationResult.SuccessMessage((successMessageKey + "_part").Translate(num, list.Count)) : ActivationResult.Success(successMessageKey, num);
	}
}
