using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(Designator_PlantsCut), "CanDesignateThing", new Type[] { typeof(Thing) })]
internal static class Designator_PlantsCut_Patch
{
	[HarmonyPostfix]
	public static void PreventSpecialTreeMassDesignation(Thing t, ref AcceptanceReport __result)
	{
		__result = SpecialTreeMassDesignationFix.RejectSpecialTreeMassDesignation(t, __result);
	}
}
