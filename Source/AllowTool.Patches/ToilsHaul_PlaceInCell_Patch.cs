using System;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(Toils_Haul))]
[HarmonyPatch("PlaceHauledThingInCell")]
[HarmonyPatch(new Type[]
{
	typeof(TargetIndex),
	typeof(Toil),
	typeof(bool),
	typeof(bool)
})]
internal static class ToilsHaul_PlaceInCell_Patch
{
	[HarmonyPostfix]
	public static void ClearHaulUrgently(Toil __result)
	{
		Action originalInitAction = __result.initAction;
		__result.initAction = delegate
		{
			Thing carriedThing = __result.actor.carryTracker.CarriedThing;
			if (carriedThing != null)
			{
				__result.actor.Map.designationManager.TryRemoveDesignationOn(carriedThing, AllowToolDefOf.HaulUrgentlyDesignation);
			}
			originalInitAction();
		};
	}
}
