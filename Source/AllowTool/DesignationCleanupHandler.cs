using System.Collections.Generic;
using Verse;

namespace AllowTool;

public static class DesignationCleanupHandler
{
	private const int TickInterval = 60;

	private static readonly Queue<Designation> cleanupList = new Queue<Designation>();

	private static readonly HashSet<Thing> workThingSet = new HashSet<Thing>();

	public static void Tick(int currentTick)
	{
		if (Current.Game == null || Current.Game.Maps == null)
		{
			return;
		}
		List<Map> maps = Current.Game.Maps;
		int num = currentTick % 60;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			if (num == map.uniqueID % 60)
			{
				CleanupDesignations(map);
			}
		}
	}

	private static void CleanupDesignations(Map map)
	{
		if (map.designationManager == null)
		{
			return;
		}
		HashSet<Thing> setOfHaulableThings = GetSetOfHaulableThings(map);
		List<Designation> allDesignations = map.designationManager.AllDesignations;
		for (int i = 0; i < allDesignations.Count; i++)
		{
			Designation designation = allDesignations[i];
			Thing thing = designation.target.Thing;
			if (thing != null && ((designation.def == AllowToolDefOf.FinishOffDesignation && !Designator_FinishOff.IsValidDesignationTarget(designation.target.Thing)) || (designation.def == AllowToolDefOf.HaulUrgentlyDesignation && !setOfHaulableThings.Contains(thing))))
			{
				cleanupList.Enqueue(designation);
			}
		}
		while (cleanupList.Count > 0)
		{
			Designation designation2 = cleanupList.Dequeue();
			designation2.designationManager.RemoveDesignation(designation2);
		}
		workThingSet.Clear();
	}

	private static HashSet<Thing> GetSetOfHaulableThings(Map map)
	{
		workThingSet.Clear();
		List<Thing> list = map.listerHaulables.ThingsPotentiallyNeedingHauling();
		for (int i = 0; i < list.Count; i++)
		{
			workThingSet.Add(list[i]);
		}
		return workThingSet;
	}
}
