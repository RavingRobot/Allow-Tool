using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(Pawn), "GetGizmos")]
internal static class Pawn_GetGizmos_Patch
{
	[HarmonyPostfix]
	public static void InsertPartyHuntGizmo(Pawn __instance, ref IEnumerable<Gizmo> __result)
	{
		Gizmo gizmo = PartyHuntHandler.TryGetGizmo(__instance);
		if (gizmo != null)
		{
			__result = AppendGizmo(__result, gizmo);
		}
	}

	private static IEnumerable<Gizmo> AppendGizmo(IEnumerable<Gizmo> originalSequence, Gizmo addition)
	{
		foreach (Gizmo item in originalSequence)
		{
			yield return item;
		}
		yield return addition;
	}
}
