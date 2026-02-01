using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace AllowTool.Context;

public static class DesignatorContextMenuController
{
	private enum MouseButtons
	{
		Left,
		Right
	}

	private static readonly Dictionary<Command, ContextMenuProvider> designatorMenuProviders = new Dictionary<Command, ContextMenuProvider>();

	private static readonly Dictionary<Command, Designator> currentDrawnReverseDesignators = new Dictionary<Command, Designator>();

	private static readonly Vector2 overlayIconOffset = new Vector2(59f, 2f);

	private static readonly HashSet<Type> reversePickingSupportedDesignators = new HashSet<Type>
	{
		typeof(Designator_Cancel),
		typeof(Designator_Claim),
		typeof(Designator_Deconstruct),
		typeof(Designator_Uninstall),
		typeof(Designator_Haul),
		typeof(Designator_Hunt),
		typeof(Designator_Slaughter),
		typeof(Designator_Tame),
		typeof(Designator_PlantsCut),
		typeof(Designator_PlantsHarvest),
		typeof(Designator_PlantsHarvestWood),
		typeof(Designator_Mine),
		typeof(Designator_Strip),
		typeof(Designator_Open)
	};

	private static readonly ContextMenuProvider[] menuProviders = new ContextMenuProvider[14]
	{
		new ContextMenuProvider(typeof(Designator_Cancel), new MenuEntry_CancelSelected(), new MenuEntry_CancelDesignations(), new MenuEntry_CancelBlueprints()),
		new ContextMenuProvider(typeof(Designator_PlantsHarvest), new MenuEntry_HarvestAll(), new MenuEntry_HarvestHome()),
		new ContextMenuProvider(typeof(Designator_PlantsHarvestWood), new MenuEntry_ChopAll(), new MenuEntry_ChopHome()),
		new ContextMenuProvider(typeof(Designator_PlantsCut), new MenuEntry_CutBlighted()),
		new ContextMenuProvider(typeof(Designator_HarvestFullyGrown), new MenuEntry_HarvestGrownAll(), new MenuEntry_HarvestGrownHome()),
		new ContextMenuProvider(typeof(Designator_FinishOff), new MenuEntry_FinishOffAll()),
		new ContextMenuProvider(typeof(Designator_Haul), new MenuEntry_HaulAll()),
		new ContextMenuProvider(typeof(Designator_HaulUrgently), new MenuEntry_HaulUrgentAll(), new MenuEntry_HaulUrgentVisible()),
		new ContextMenuProvider(typeof(Designator_Hunt), new MenuEntry_HuntAll()),
		new ContextMenuProvider(typeof(Designator_Mine), new MenuEntry_MineConnected(), new MenuEntry_MineSelectStripMine()),
		new ContextMenuProvider(typeof(Designator_SelectSimilar), new MenuEntry_SelectSimilarAll(), new MenuEntry_SelectSimilarVisible(), new MenuEntry_SelectSimilarHome()),
		new ContextMenuProvider(typeof(Designator_Strip), new MenuEntry_StripAll()),
		new ContextMenuProvider(typeof(Designator_Allow), new MenuEntry_AllowVisible()),
		new ContextMenuProvider(typeof(Designator_Forbid), new MenuEntry_ForbidVisible())
	};

	private static readonly ContextMenuProvider fallbackMenuProvider = new ContextMenuProvider(null);

	public static void RebindAllContextMenus()
	{
		try
		{
			designatorMenuProviders.Clear();
			IEnumerable<Designator> enumerable = AllowToolUtility.EnumerateResolvedDirectDesignators();
			foreach (Designator item in enumerable)
			{
				TryBindDesignatorToProvider(item);
			}
			PrepareReverseDesignatorContextMenus();
		}
		catch (Exception e)
		{
			AllowToolController.Logger.ReportException(e);
		}
	}

	public static void DrawCommandOverlayIfNeeded(Command command, Vector2 topLeft)
	{
		Designator designator = TryResolveCommandToDesignator(command);
		if (designator == null)
		{
			return;
		}
		try
		{
			if (AllowToolController.Instance.Handles.ContextOverlaySetting.Value && designatorMenuProviders.ContainsKey(designator))
			{
				float num = ((command is Command_Toggle) ? 56f : 0f);
				AllowToolUtility.DrawRightClickIcon(topLeft.x + overlayIconOffset.x, topLeft.y + overlayIconOffset.y + num);
			}
		}
		catch (Exception e)
		{
			designatorMenuProviders.Remove(designator);
			AllowToolController.Logger.ReportException(e);
		}
	}

	public static bool TryProcessDesignatorInput(Designator designator)
	{
		try
		{
			if (Event.current.button == 0 && HugsLibUtility.ShiftIsHeld && (bool)AllowToolController.Instance.Handles.ReverseDesignatorPickSetting)
			{
				return TryPickDesignatorFromReverseDesignator(designator);
			}
			if (Event.current.button == 1 && designatorMenuProviders.TryGetValue(designator, out var value))
			{
				value.OpenContextMenu(designator);
				return true;
			}
		}
		catch (Exception e)
		{
			AllowToolController.Logger.ReportException(e);
		}
		return false;
	}

	public static Designator TryResolveCommandToDesignator(Command command)
	{
		if (command != null)
		{
			Designator value = command as Designator;
			if (value != null)
			{
				return value;
			}
			if (currentDrawnReverseDesignators.TryGetValue(command, out value))
			{
				return value;
			}
		}
		return null;
	}

	public static void ProcessContextActionHotkeyPress()
	{
		Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator;
		if (selectedDesignator != null)
		{
			if (!designatorMenuProviders.TryGetValue(selectedDesignator, out var value))
			{
				value = GetMenuProviderForDesignator(selectedDesignator);
			}
			value.TryInvokeHotkeyAction(selectedDesignator);
		}
		else
		{
			if (!AllowToolController.Instance.Handles.ExtendedContextActionSetting.Value)
			{
				return;
			}
			foreach (Designator value3 in currentDrawnReverseDesignators.Values)
			{
				if (designatorMenuProviders.TryGetValue(value3, out var value2) && value2.TryInvokeHotkeyAction(value3))
				{
					break;
				}
			}
		}
	}

	public static void RegisterReverseDesignatorPair(Designator designator, Command designatorButton)
	{
		currentDrawnReverseDesignators.Add(designatorButton, designator);
	}

	public static void RegisterReverseDesignatorPair(Designator designator, Command_Action designatorButton)
	{
		RegisterReverseDesignatorPair(designator, (Command)designatorButton);
	}

	internal static IEnumerable<SettingHandle<bool>> RegisterMenuEntryHandles(ModSettingsPack pack)
	{
		return menuProviders.SelectMany((ContextMenuProvider p) => p.RegisterEntryHandles(pack));
	}

	private static void PrepareReverseDesignatorContextMenus()
	{
		ClearReverseDesignatorPairs();
		foreach (Designator item in AllowToolUtility.EnumerateReverseDesignators())
		{
			TryBindDesignatorToProvider(item);
		}
		foreach (Designator impliedReverseDesignator in AllowThingToggleHandler.GetImpliedReverseDesignators())
		{
			TryBindDesignatorToProvider(impliedReverseDesignator);
		}
	}

	public static void ClearReverseDesignatorPairs()
	{
		currentDrawnReverseDesignators.Clear();
	}

	private static bool TryPickDesignatorFromReverseDesignator(Designator designator)
	{
		bool flag = false;
		if (designator != null && designator is IReversePickableDesignator reversePickableDesignator)
		{
			designator = reversePickableDesignator.PickUpReverseDesignator();
			flag = true;
		}
		if (designator != null && (flag || reversePickingSupportedDesignators.Contains(designator.GetType())))
		{
			Find.DesignatorManager.Select(designator);
			return true;
		}
		return false;
	}

	private static void TryBindDesignatorToProvider(Designator designator)
	{
		if (designator != null && !designatorMenuProviders.ContainsKey(designator))
		{
			ContextMenuProvider menuProviderForDesignator = GetMenuProviderForDesignator(designator);
			if (menuProviderForDesignator.HasCustomEnabledEntries || DesignatorShouldHaveFallbackContextMenuProvider(designator))
			{
				designatorMenuProviders.Add(designator, menuProviderForDesignator);
			}
		}
	}

	private static ContextMenuProvider GetMenuProviderForDesignator(Designator designator)
	{
		for (int i = 0; i < menuProviders.Length; i++)
		{
			if (menuProviders[i].HandledDesignatorType.IsInstanceOfType(designator))
			{
				return menuProviders[i];
			}
		}
		return fallbackMenuProvider;
	}

	private static bool DesignatorShouldHaveFallbackContextMenuProvider(Designator designator)
	{
		try
		{
			if (designator.GetType() != typeof(Designator_Build))
			{
				if (AllowToolController.Instance.Reflection.DesignatorGetDesignationMethod.Invoke(designator, new object[0]) != null)
				{
					return true;
				}
				if ((bool)AllowToolController.Instance.Reflection.DesignatorHasDesignateAllFloatMenuOptionField.GetValue(designator))
				{
					return true;
				}
				MethodInfo method = designator.GetType().GetMethod("get_RightClickFloatMenuOptions", HugsLibUtility.AllBindingFlags);
				if (method != null && method.DeclaringType != typeof(Designator))
				{
					return true;
				}
			}
		}
		catch (Exception)
		{
		}
		return false;
	}
}
