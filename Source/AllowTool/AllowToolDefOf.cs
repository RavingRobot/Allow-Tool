using System.Reflection;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace AllowTool;

[DefOf]
public static class AllowToolDefOf
{
	[StaticConstructorOnStartup]
	public static class Textures
	{
		public static Texture2D rightClickOverlay;

		public static Texture2D contextMenuWatermark;

		public static Texture2D designatorSelectionOption;

		public static Texture2D partyHunt;

		static Textures()
		{
			FieldInfo[] fields = typeof(Textures).GetFields(HugsLibUtility.AllBindingFlags);
			foreach (FieldInfo fieldInfo in fields)
			{
				fieldInfo.SetValue(null, ContentFinder<Texture2D>.Get(fieldInfo.Name));
			}
		}
	}

	public static DesignationDef HaulUrgentlyDesignation;

	public static DesignationDef FinishOffDesignation;

	public static ThingDesignatorDef AllowDesignator;

	public static ThingDesignatorDef ForbidDesignator;

	public static ThingDesignatorDef AllowAllDesignator;

	public static ThingDesignatorDef SelectSimilarDesignator;

	public static ThingDesignatorDef HaulUrgentlyDesignator;

	public static ThingDesignatorDef FinishOffDesignator;

	public static ThingDesignatorDef HarvestFullyGrownDesignator;

	public static ThingDesignatorDef StripMineDesignator;

    // CutGrassChopTrees
    public static ThingDesignatorDef CutGrassDesignator;
    public static ThingDesignatorDef ChopAllTreesDesignator;
    // CutGrassChopTrees

    public static ReverseDesignatorDef ReverseFinishOff;

	public static WorkTypeDef HaulingUrgent;

	public static WorkTypeDef FinishingOff;

	public static WorkGiverDef FinishOff;

	public static JobDef FinishOffPawn;

	public static EffecterDef EffecterWeaponGlint;

	public static KeyBindingDef ToolContextMenuAction;
}
