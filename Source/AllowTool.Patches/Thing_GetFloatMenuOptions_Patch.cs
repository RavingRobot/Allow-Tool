using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(ThingWithComps), "GetFloatMenuOptions")]
internal static class Thing_GetFloatMenuOptions_Patch
{
	[HarmonyPostfix]
	public static void FinishOffWhenDrafted(ref IEnumerable<FloatMenuOption> __result, Thing __instance, Pawn selPawn)
	{
		FloatMenuOption floatMenuOption = WorkGiver_FinishOff.InjectThingFloatOptionIfNeeded(__instance, selPawn);
		if (floatMenuOption != null)
		{
			List<FloatMenuOption> list = __result.ToList();
			list.Add(floatMenuOption);
			__result = list;
		}
	}
}
