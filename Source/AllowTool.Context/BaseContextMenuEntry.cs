using System;
using HugsLib.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace AllowTool.Context;

public abstract class BaseContextMenuEntry
{
	protected delegate void MenuActionMethod(Designator designator, Map map);

	private const string SettingHandlePrefix = "contextEntry_";

	private SettingHandle<bool> enabledHandle;

	public bool Enabled => enabledHandle == null || enabledHandle.Value;

	protected abstract string BaseTextKey { get; }

	protected abstract string SettingHandleSuffix { get; }

	protected virtual string Label => BaseTextKey.Translate();

	protected virtual string BaseMessageKey => BaseTextKey;

	protected virtual ThingRequestGroup DesignationRequestGroup => ThingRequestGroup.Everything;

	public SettingHandle<bool> RegisterSettingHandle(ModSettingsPack pack)
	{
		return enabledHandle = pack.GetHandle("contextEntry_" + SettingHandleSuffix, "setting_providerPrefix".Translate(Label), "setting_provider_desc".Translate(), defaultValue: true);
	}

	public virtual ActivationResult Activate(Designator designator, Map map)
	{
		return ActivateWithFilter(designator, map, null);
	}

	public virtual FloatMenuOption MakeMenuOption(Designator designator)
	{
		return MakeStandardOption(designator);
	}

	protected ActivationResult ActivateInHomeArea(Designator designator, Map map, Predicate<Thing> extraFilter = null)
	{
		Predicate<Thing> inHomeArea = GetHomeAreaFilter(map);
		return ActivateWithFilter(designator, map, (Thing thing) => inHomeArea(thing) && (extraFilter == null || extraFilter(thing)));
	}

	protected ActivationResult ActivateInVisibleArea(Designator designator, Map map, Predicate<Thing> extraFilter = null)
	{
		Predicate<Thing> thingIsVisible = GetVisibleThingFilter();
		return ActivateWithFilter(designator, map, (Thing thing) => thingIsVisible(thing) && (extraFilter == null || extraFilter(thing)));
	}

	protected ActivationResult ActivateWithFilter(Designator designator, Map map, Predicate<Thing> thingFilter)
	{
		int designationCount = DesignateAllThings(designator, map, thingFilter);
		return ActivationResult.FromCount(designationCount, BaseMessageKey);
	}

	protected int DesignateAllThings(Designator designator, Map map, Predicate<Thing> thingFilter)
	{
		int num = 0;
		foreach (Thing item in map.listerThings.ThingsInGroup(DesignationRequestGroup))
		{
			if (ThingIsValidForDesignation(item) && (thingFilter == null || thingFilter(item)) && designator.CanDesignateThing(item).Accepted)
			{
				designator.DesignateThing(item);
				num++;
			}
		}
		return num;
	}

	protected FloatMenuOption MakeStandardOption(Designator designator, string descriptionKey = null, Texture2D extraIcon = null)
	{
		Func<Rect, bool> func = null;
		float num = 0f;
		if (extraIcon != null)
		{
			func = delegate(Rect rect)
			{
				Graphics.DrawTexture(new Rect(rect.x + 10f, rect.height / 2f - 12f + rect.y, 24f, 24f), extraIcon);
				return false;
			};
			num = 34f;
		}
		string label = Label;
		Action action = delegate
		{
			ActivateAndHandleResult(designator);
		};
		float extraPartWidth = num;
		Func<Rect, bool> extraPartOnGUI = func;
		TaggedString? taggedString = descriptionKey?.Translate();
		return new ATFloatMenuOption(label, action, MenuOptionPriority.Default, null, null, extraPartWidth, extraPartOnGUI, null, taggedString.HasValue ? ((string)taggedString.GetValueOrDefault()) : null);
	}

	public void ActivateAndHandleResult(Designator designator)
	{
		try
		{
			Map currentMap = Find.CurrentMap;
			if (currentMap != null)
			{
				Activate(designator, currentMap)?.ShowMessage();
			}
		}
		catch (Exception ex)
		{
			AllowToolController.Logger.Error("Exception while processing context menu action: " + ex);
		}
	}

	protected static bool ThingIsValidForDesignation(Thing thing)
	{
		return thing?.def != null && thing.Map != null && !thing.Map.fogGrid.IsFogged(thing.Position);
	}

	protected Predicate<Thing> GetHomeAreaFilter(Map map)
	{
		Area_Home homeArea = map.areaManager.Home;
		return (Thing thing) => homeArea.GetCellBool(map.cellIndices.CellToIndex(thing.Position));
	}

	protected Predicate<Thing> GetVisibleThingFilter()
	{
		CellRect visibleRect = AllowToolUtility.GetVisibleMapRect();
		return (Thing t) => visibleRect.Contains(t.Position);
	}

	protected static Predicate<Thing> GetExceptSpecialTreeFilter()
	{
		return (Thing t) => !SpecialTreeMassDesignationFix.IsSpecialTree(t);
	}
}
