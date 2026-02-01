using System;
using AllowTool.Context;
using AllowTool.Settings;
using HugsLib;
using HugsLib.Utils;
using UnityEngine;
using Verse;

namespace AllowTool;

[EarlyInit]
public class AllowToolController : ModBase
{
	private HotKeyHandler hotKeys;

	private bool dependencyRefreshScheduled;

	private bool modSettingsHaveChanged;

	private int fixedUpdateCount;

	public static AllowToolController Instance { get; private set; }

	public override string ModIdentifier => "AllowTool";

	private ModLogger GetLogger => base.Logger;

	internal new static ModLogger Logger => Instance.GetLogger;

	public WorldSettings WorldSettings { get; private set; }

	public ModSettingsHandler Handles { get; private set; }

	public ReflectionHandler Reflection { get; private set; }

	internal HaulUrgentlyCacheHandler HaulUrgentlyCache { get; private set; }

	private AllowToolController()
	{
		Instance = this;
	}

    public override void EarlyInitialize()
    {
		Handles = new ModSettingsHandler();
		Handles.PackSettingsChanged += delegate
		{
			modSettingsHaveChanged = true;
		};
		Reflection = new ReflectionHandler();
		Reflection.PrepareReflection();
		HaulUrgentlyCache = new HaulUrgentlyCacheHandler();
		hotKeys = new HotKeyHandler();
		LongEventHandler.QueueLongEvent(PickUpAndHaulCompatHandler.Apply, null, doAsynchronously: false, null);
	}

	public override void FixedUpdate()
	{
		HaulUrgentlyCache.ProcessCacheEntries(fixedUpdateCount, Time.unscaledTime);
		fixedUpdateCount++;
	}

	public override void Tick(int currentTick)
	{
		DesignationCleanupHandler.Tick(currentTick);
	}

	public override void OnGUI()
	{
		hotKeys.OnGUI();
	}

	public override void WorldLoaded()
	{
		WorldSettings = Find.World.GetComponent<WorldSettings>();
		HaulUrgentlyCache.ClearCacheForAllMaps();
	}

	public override void MapLoaded(Map map)
	{
		if (!Handles.HaulWorktypeSetting)
		{
			AllowToolUtility.EnsureAllColonistsHaveWorkTypeEnabled(AllowToolDefOf.HaulingUrgent, map);
		}
		if (!Handles.FinishOffWorktypeSetting)
		{
			AllowToolUtility.EnsureAllColonistsHaveWorkTypeEnabled(AllowToolDefOf.FinishingOff, map);
		}
	}

	public override void MapDiscarded(Map map)
	{
		HaulUrgentlyCache.ClearCacheForMap(map);
	}

	public override void SettingsChanged()
	{
		if (modSettingsHaveChanged)
		{
			modSettingsHaveChanged = false;
			ResolveAllDesignationCategories();
			if (AllowToolUtility.ReverseDesignatorDatabaseInitialized)
			{
				Find.ReverseDesignatorDatabase.Reinit();
			}
		}
	}

	internal void OnBeforeImpliedDefGeneration()
	{
		try
		{
			Handles.PrepareSettingsHandles(Instance.Settings);
			if (!Handles.HaulWorktypeSetting)
			{
				AllowToolDefOf.HaulingUrgent.visible = false;
			}
			if ((bool)Handles.FinishOffWorktypeSetting)
			{
				AllowToolDefOf.FinishingOff.visible = true;
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Error during early setting handle setup: " + ex);
		}
	}

	internal void OnDesignationCategoryResolveDesignators()
	{
		ScheduleDesignatorDependencyRefresh();
	}

	internal void OnReverseDesignatorDatabaseInit(ReverseDesignatorDatabase database)
	{
		ReverseDesignatorHandler.InjectReverseDesignators(database);
		ScheduleDesignatorDependencyRefresh();
	}

	internal void ScheduleDesignatorDependencyRefresh()
	{
		if (dependencyRefreshScheduled)
		{
			return;
		}
		dependencyRefreshScheduled = true;
		HugsLibController.Instance.DoLater.DoNextUpdate(delegate
		{
			try
			{
				dependencyRefreshScheduled = false;
				hotKeys.RebindAllDesignators();
				AllowThingToggleHandler.ReinitializeDesignators();
				DesignatorContextMenuController.RebindAllContextMenus();
			}
			catch (Exception arg)
			{
				Logger.Error($"Error during designator dependency refresh: {arg}");
			}
		});
	}

	private void ResolveAllDesignationCategories()
	{
		foreach (DesignationCategoryDef allDef in DefDatabase<DesignationCategoryDef>.AllDefs)
		{
			Reflection.DesignationCategoryDefResolveDesignatorsMethod.Invoke(allDef, new object[0]);
		}
	}
}
