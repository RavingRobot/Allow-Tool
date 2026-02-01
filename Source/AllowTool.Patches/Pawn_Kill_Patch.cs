using System;
using AllowTool.Settings;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(Pawn), "Kill", new Type[]
{
	typeof(DamageInfo?),
	typeof(Hediff)
})]
internal static class Pawn_Kill_Patch
{
	[HarmonyPostfix]
	public static void UnforbidDraftedHuntBody(Pawn __instance, DamageInfo? dinfo)
	{
		Pawn pawn = dinfo?.Instigator as Pawn;
		PartyHuntSettings partyHuntSettings = AllowToolController.Instance.WorldSettings?.PartyHunt;
		if (pawn != null && partyHuntSettings != null && partyHuntSettings.UnforbidDrops && partyHuntSettings.PawnIsPartyHunting(pawn))
		{
			__instance.Corpse?.SetForbidden(value: false, warnOnFail: false);
		}
	}
}
