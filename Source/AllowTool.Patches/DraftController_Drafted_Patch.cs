using System;
using HarmonyLib;
using RimWorld;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted", new Type[] { typeof(bool) })]
internal static class DraftController_Drafted_Patch
{
	[HarmonyPostfix]
	public static void NotifyPawnUndrafted(Pawn_DraftController __instance, bool value)
	{
		if (!value)
		{
			PartyHuntHandler.OnPawnUndrafted(__instance.pawn);
		}
	}
}
