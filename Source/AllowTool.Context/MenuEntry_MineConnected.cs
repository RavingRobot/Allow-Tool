using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AllowTool.Context;

public class MenuEntry_MineConnected : BaseContextMenuEntry
{
	private delegate bool InitialCandidateFilter(IntVec3 cell, Map map);

	private delegate bool ExpansionCandidateFilter(IntVec3 fromCell, IntVec3 toCell, Map map);

	protected override string BaseTextKey => "Designator_context_mine";

	protected override string SettingHandleSuffix => "mineConnected";

	public override ActivationResult Activate(Designator designator, Map map)
	{
		MineDesignateSelectedOres(map);
		if (!map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Mine).Any())
		{
			return ActivationResult.Failure(BaseMessageKey);
		}
		int num = FloodExpandDesignationType(DesignationDefOf.Mine, map, (IntVec3 cell, Map m) => !m.fogGrid.IsFogged(cell), MineDesignationExpansionIsValid);
		return ActivationResult.Success(BaseMessageKey, num);
	}

	private bool MineDesignationExpansionIsValid(IntVec3 cellFrom, IntVec3 cellTo, Map map)
	{
		Thing thing = TryGetMineableAtPos(cellFrom, map);
		Thing thing2 = TryGetMineableAtPos(cellTo, map);
		return thing != null && thing2 != null && thing.def == thing2.def;
	}

	private Thing TryGetMineableAtPos(IntVec3 pos, Map map)
	{
		Building building = map.edificeGrid[pos];
		return (building?.def.building != null && building.def.mineable && building.def.building.isResourceRock) ? building : null;
	}

	private void MineDesignateSelectedOres(Map map)
	{
		IEnumerable<Thing> enumerable = from o in Find.Selector.SelectedObjects.OfType<Thing>()
			where TryGetMineableAtPos(o.Position, map) != null
			select o;
		foreach (Thing item in enumerable)
		{
			item.Position.ToggleDesignation(DesignationDefOf.Mine, enable: true);
		}
	}

	private int FloodExpandDesignationType(DesignationDef designationDef, Map map, InitialCandidateFilter initialFilter, ExpansionCandidateFilter expansionFilter)
	{
		List<IntVec3> list = (from d in map.designationManager.SpawnedDesignationsOfDef(designationDef)
			where !d.target.HasThing
			select d.target.Cell).ToList();
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>(list);
		Queue<IntVec3> queue = new Queue<IntVec3>(list.Where((IntVec3 c) => initialFilter(c, map)));
		IntVec3[] adjacentCellsAround = GenAdj.AdjacentCellsAround;
		int num = 1000000;
		int num2 = 0;
		while (queue.Count > 0 && num > 0)
		{
			num--;
			IntVec3 intVec = queue.Dequeue();
			for (int num3 = 0; num3 < adjacentCellsAround.Length; num3++)
			{
				IntVec3 intVec2 = intVec + adjacentCellsAround[num3];
				try
				{
					if (!hashSet.Contains(intVec2) && expansionFilter(intVec, intVec2, map))
					{
						map.designationManager.AddDesignation(new Designation(intVec2, designationDef));
						hashSet.Add(intVec2);
						num2++;
						queue.Enqueue(intVec2);
					}
				}
				catch (Exception arg)
				{
					hashSet.Add(intVec2);
					AllowToolController.Logger.Warning($"Exception while trying to designate cell {intVec2} via \"mine connected\": {arg}");
				}
			}
		}
		if (num == 0)
		{
			AllowToolController.Logger.Error("Ran out of cycles while expanding designations: " + Environment.StackTrace);
		}
		return num2;
	}
}
