using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AllowTool;

public static class AllowToolUtility
{
	public static bool ReverseDesignatorDatabaseInitialized => Current.Root?.uiRoot is UIRoot_Play uIRoot_Play && uIRoot_Play.mapUI?.reverseDesignatorDatabase != null;

	public static int ToggleForbiddenInCell(IntVec3 cell, Map map, bool makeForbidden)
	{
		if (map == null)
		{
			throw new NullReferenceException("map is null");
		}
		int num = 0;
		List<Thing> list;
		try
		{
			list = map.thingGrid.ThingsListAtFast(cell);
		}
		catch (IndexOutOfRangeException innerException)
		{
			IntVec3 intVec = cell;
			throw new IndexOutOfRangeException("Cell out of bounds: " + intVec.ToString(), innerException);
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is ThingWithComps thingWithComps && thingWithComps.def.selectable)
			{
				CompForbiddable comp = thingWithComps.GetComp<CompForbiddable>();
				if (comp != null && comp.Forbidden != makeForbidden)
				{
					comp.Forbidden = makeForbidden;
					num++;
				}
			}
		}
		return num;
	}

	public static void EnsureAllColonistsHaveWorkTypeEnabled(WorkTypeDef def, Map map)
	{
		try
		{
			HashSet<(int, Pawn)> hashSet = new HashSet<(int, Pawn)>();
			if (map?.mapPawns == null)
			{
				return;
			}
			IEnumerable<Pawn> enumerable = map.mapPawns.PawnsInFaction(Faction.OfPlayer).Concat(map.mapPawns.PrisonersOfColony);
			foreach (Pawn item in enumerable)
			{
				Pawn_WorkSettings workSettings = item.workSettings;
				if (workSettings != null && workSettings.EverWork && !item.WorkTypeIsDisabled(def))
				{
					int priority = workSettings.GetPriority(def);
					if (priority != 3)
					{
						workSettings.SetPriority(def, 3);
						hashSet.Add((priority, item));
					}
				}
			}
			if (hashSet.Count > 0)
			{
				AllowToolController.Logger.Message("Adjusted work type priority of {0} for pawns:\n{1}", def.defName, hashSet.Select<(int, Pawn), string>(((int prevValue, Pawn pawn) t) => $"{t.pawn.Name?.ToStringShort.ToStringSafe()}:{t.prevValue}->{3}").ListElements());
			}
		}
		catch (Exception ex)
		{
			AllowToolController.Logger.Error("Exception while adjusting work type priority in colonist pawns: " + ex);
		}
	}

	public static bool PawnIsFriendly(Thing t)
	{
		Pawn pawn = t as Pawn;
		return pawn?.Faction != null && (pawn.IsPrisonerOfColony || !pawn.Faction.HostileTo(Faction.OfPlayer));
	}

	public static void DrawMouseAttachedLabel(string text)
	{
		DrawMouseAttachedLabel(text, Color.white);
	}

	public static void DrawMouseAttachedLabel(string text, Color textColor)
	{
		Vector2 vector = new Vector2(8f, 20f);
		Vector2 mousePosition = Event.current.mousePosition;
		if (!text.NullOrEmpty())
		{
			Rect rect = new Rect(mousePosition.x + vector.x, mousePosition.y + vector.y + 32f, 200f, 9999f);
			Text.Font = GameFont.Small;
			Color color = GUI.color;
			GUI.color = textColor;
			Widgets.Label(rect, text);
			GUI.color = color;
		}
	}

	public static bool PawnCapableOfViolence(Pawn pawn)
	{
		return !pawn.WorkTagIsDisabled(WorkTags.Violent);
	}

	public static void DrawRightClickIcon(float x, float y)
	{
		Texture2D rightClickOverlay = AllowToolDefOf.Textures.rightClickOverlay;
		GUI.DrawTexture(new Rect(x, y, rightClickOverlay.width, rightClickOverlay.height), rightClickOverlay);
	}

	public static ATFloatMenuOption MakeCheckmarkOption(string labelKey, string descriptionKey, Func<bool> getter, Action<bool> setter)
	{
		bool checkOn = getter();
		string label = labelKey.Translate();
		Action action = delegate
		{
			setter(!getter());
			checkOn = getter();
			HugsLibController.SettingsManager.SaveChanges();
			SoundDef soundDef = (checkOn ? SoundDefOf.Checkbox_TurnedOn : SoundDefOf.Checkbox_TurnedOff);
			soundDef.PlayOneShotOnCamera();
		};
		Func<Rect, bool> extraPartOnGUI = delegate(Rect rect)
		{
			Widgets.Checkbox(rect.x + 10f, rect.height / 2f - 12f + rect.y, ref checkOn);
			return false;
		};
		TaggedString? taggedString = descriptionKey?.Translate();
		return new ATFloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 34f, extraPartOnGUI, null, taggedString.HasValue ? ((string)taggedString.GetValueOrDefault()) : null);
	}

	public static CellRect GetVisibleMapRect()
	{
		Rect rect = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
		Vector2 screenLoc = new Vector2(rect.x, (float)UI.screenHeight - rect.y);
		Vector2 screenLoc2 = new Vector2(rect.x + rect.width, (float)UI.screenHeight - (rect.y + rect.height));
		Vector3 vector = UI.UIToMapPosition(screenLoc);
		Vector3 vector2 = UI.UIToMapPosition(screenLoc2);
		return new CellRect
		{
			minX = Mathf.FloorToInt(vector.x),
			minZ = Mathf.FloorToInt(vector2.z),
			maxX = Mathf.FloorToInt(vector2.x),
			maxZ = Mathf.FloorToInt(vector.z)
		};
	}

	public static IEnumerable<Designator> EnumerateResolvedDirectDesignators()
	{
		return DefDatabase<DesignationCategoryDef>.AllDefs.SelectMany((DesignationCategoryDef cat) => cat.AllResolvedDesignators).ToArray();
	}

	public static IEnumerable<Designator> EnumerateReverseDesignators()
	{
		return ReverseDesignatorDatabaseInitialized ? Find.ReverseDesignatorDatabase.AllDesignators : new List<Designator>();
	}
}
