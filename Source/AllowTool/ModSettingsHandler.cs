using System;
using System.Collections.Generic;
using AllowTool.Context;
using AllowTool.Settings;
using HugsLib.Settings;
using UnityEngine;
using Verse;

namespace AllowTool;

public class ModSettingsHandler
{
	private const string DesignatorHandleNamePrefix = "show";

	private const string ReverseDesignatorHandleNamePrefix = "showrev";

	private readonly Dictionary<string, SettingHandle<bool>> designatorToggleHandles = new Dictionary<string, SettingHandle<bool>>();

	private readonly Dictionary<string, SettingHandle<bool>> reverseDesignatorToggleHandles = new Dictionary<string, SettingHandle<bool>>();

	private bool expandToolSettings;

	private bool expandProviderSettings;

	private bool expandReverseToolSettings;

	public SettingHandle<int> SelectionLimitSetting { get; private set; }

	public SettingHandle<bool> GlobalHotkeysSetting { get; private set; }

	public SettingHandle<bool> ContextOverlaySetting { get; private set; }

	public SettingHandle<bool> ContextWatermarkSetting { get; private set; }

	public SettingHandle<bool> ReplaceIconsSetting { get; private set; }

	public SettingHandle<bool> HaulWorktypeSetting { get; private set; }

	public SettingHandle<bool> FinishOffWorktypeSetting { get; private set; }

	public SettingHandle<bool> ExtendedContextActionSetting { get; private set; }

	public SettingHandle<bool> ReverseDesignatorPickSetting { get; private set; }

	public SettingHandle<bool> FinishOffSkillRequirement { get; private set; }

	public SettingHandle<bool> FinishOffUnforbidsSetting { get; private set; }

	public SettingHandle<bool> PartyHuntSetting { get; private set; }

	public SettingHandle<bool> StorageSpaceAlertSetting { get; private set; }

	public SettingHandle<StripMineGlobalSettings> StripMineSettings { get; private set; }

	public event Action PackSettingsChanged;

	internal void PrepareSettingsHandles(ModSettingsPack pack)
	{
		GlobalHotkeysSetting = pack.GetHandle("globalHotkeys", "setting_globalHotkeys_label".Translate(), "setting_globalHotkeys_desc".Translate(), defaultValue: true);
		ContextOverlaySetting = pack.GetHandle("contextOverlay", "setting_contextOverlay_label".Translate(), "setting_contextOverlay_desc".Translate(), defaultValue: true);
		ContextWatermarkSetting = pack.GetHandle("contextWatermark", "setting_contextWatermark_label".Translate(), "setting_contextWatermark_desc".Translate(), defaultValue: true);
		ReplaceIconsSetting = pack.GetHandle("replaceIcons", "setting_replaceIcons_label".Translate(), "setting_replaceIcons_desc".Translate(), defaultValue: false);
		HaulWorktypeSetting = pack.GetHandle("haulUrgentlyWorktype", "setting_haulUrgentlyWorktype_label".Translate(), "setting_haulUrgentlyWorktype_desc".Translate(), defaultValue: true);
		FinishOffWorktypeSetting = pack.GetHandle("finishOffWorktype", "setting_finishOffWorktype_label".Translate(), "setting_finishOffWorktype_desc".Translate(), defaultValue: false);
		ExtendedContextActionSetting = pack.GetHandle("extendedContextActionKey", "setting_extendedContextHotkey_label".Translate(), "setting_extendedContextHotkey_desc".Translate(), defaultValue: true);
		ReverseDesignatorPickSetting = pack.GetHandle("reverseDesignatorPick", "setting_reverseDesignatorPick_label".Translate(), "setting_reverseDesignatorPick_desc".Translate(), defaultValue: true);
		FinishOffUnforbidsSetting = pack.GetHandle("finishOffUnforbids", "setting_finishOffUnforbids_label".Translate(), "setting_finishOffUnforbids_desc".Translate(), defaultValue: true);
		PartyHuntSetting = pack.GetHandle("partyHunt", "setting_partyHunt_label".Translate(), "setting_partyHunt_desc".Translate(), defaultValue: true);
		StorageSpaceAlertSetting = pack.GetHandle("storageSpaceAlert", "setting_storageSpaceAlert_label".Translate(), "setting_storageSpaceAlert_desc".Translate(), defaultValue: true);
		SelectionLimitSetting = pack.GetHandle("selectionLimit", "setting_selectionLimit_label".Translate(), "setting_selectionLimit_desc".Translate(), 200, Validators.IntRangeValidator(50, 100000));
		SelectionLimitSetting.SpinnerIncrement = 50;
		MakeSettingsCategoryToggle(pack, "setting_showToolToggles_label", delegate
		{
			expandToolSettings = !expandToolSettings;
		});
		foreach (ThingDesignatorDef allDef in DefDatabase<ThingDesignatorDef>.AllDefs)
		{
			string text = "show" + allDef.defName;
			SettingHandle<bool> handle = pack.GetHandle(text, "setting_showTool_label".Translate(allDef.label), null, defaultValue: true);
			handle.VisibilityPredicate = () => expandToolSettings;
			designatorToggleHandles[text] = handle;
		}
		MakeSettingsCategoryToggle(pack, "setting_showProviderToggles_label", delegate
		{
			expandProviderSettings = !expandProviderSettings;
		});
		SettingHandle.ShouldDisplay visibilityPredicate = () => expandProviderSettings;
		foreach (SettingHandle<bool> item in DesignatorContextMenuController.RegisterMenuEntryHandles(pack))
		{
			item.VisibilityPredicate = visibilityPredicate;
		}
		MakeSettingsCategoryToggle(pack, "setting_showReverseToggles_label", delegate
		{
			expandReverseToolSettings = !expandReverseToolSettings;
		});
		foreach (ReverseDesignatorDef allDef2 in DefDatabase<ReverseDesignatorDef>.AllDefs)
		{
			string text2 = "showrev" + allDef2.defName;
			SettingHandle<bool> handle2 = pack.GetHandle(text2, "setting_showTool_label".Translate(allDef2.designatorDef.label), "setting_reverseDesignator_desc".Translate(), defaultValue: true);
			handle2.VisibilityPredicate = () => expandReverseToolSettings;
			reverseDesignatorToggleHandles[text2] = handle2;
		}
		FinishOffSkillRequirement = pack.GetHandle("finishOffSkill", "setting_finishOffSkill_label".Translate(), "setting_finishOffSkill_desc".Translate(), defaultValue: true);
		FinishOffSkillRequirement.VisibilityPredicate = () => Prefs.DevMode;
		StripMineSettings = pack.GetHandle<StripMineGlobalSettings>("stripMineSettings", null, null);
		if (StripMineSettings.Value == null)
		{
			StripMineSettings.Value = new StripMineGlobalSettings();
		}
		StripMineSettings.VisibilityPredicate = () => false;
		RegisterPackHandlesChangedCallback(pack);
	}

	public bool IsDesignatorEnabled(ThingDesignatorDef def)
	{
		return GetToolHandleSettingValue(designatorToggleHandles, "show" + def.defName);
	}

	public bool IsReverseDesignatorEnabled(ReverseDesignatorDef def)
	{
		return GetToolHandleSettingValue(reverseDesignatorToggleHandles, "showrev" + def.defName);
	}

	private void MakeSettingsCategoryToggle(ModSettingsPack pack, string labelId, Action buttonAction)
	{
		SettingHandle<bool> handle = pack.GetHandle(labelId, labelId.Translate(), null, defaultValue: false);
		handle.Unsaved = true;
		handle.CustomDrawer = delegate(Rect rect)
		{
			if (Widgets.ButtonText(rect, "setting_showToggles_btn".Translate()))
			{
				buttonAction();
			}
			return false;
		};
	}

	private bool GetToolHandleSettingValue(Dictionary<string, SettingHandle<bool>> handleDict, string handleName)
	{
		SettingHandle<bool> value;
		return handleDict.TryGetValue(handleName, out value) && value.Value;
	}

	private void RegisterPackHandlesChangedCallback(ModSettingsPack pack)
	{
		Action<SettingHandle> action = delegate
		{
			this.PackSettingsChanged?.Invoke();
		};
		foreach (SettingHandle handle in pack.Handles)
		{
			if (handle is SettingHandle<bool> settingHandle)
			{
				((SettingHandle)settingHandle).ValueChanged += action;
			}
		}
	}
}
