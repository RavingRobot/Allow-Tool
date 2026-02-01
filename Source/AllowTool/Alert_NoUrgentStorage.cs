using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AllowTool;

public class Alert_NoUrgentStorage : Alert
{
	private const int MaxListedCulpritsInExplanation = 5;

	public override AlertPriority Priority => AlertPriority.High;

	protected override Color BGColor => new Color(1f, 0.9215686f, 0.01568628f, 0.35f);

	public Alert_NoUrgentStorage()
	{
		defaultLabel = "Alert_noStorage_label".Translate();
	}

	public override TaggedString GetExplanation()
	{
		List<GlobalTargetInfo> designatedThingsWithoutStorageSpace = AllowToolController.Instance.HaulUrgentlyCache.GetDesignatedThingsWithoutStorageSpace();
		List<string> list = designatedThingsWithoutStorageSpace.Select((GlobalTargetInfo t) => t.Thing?.LabelShort).Take(5).ToList();
		if (designatedThingsWithoutStorageSpace.Count > 5)
		{
			list.Add("...");
		}
		return "Alert_noStorage_desc".Translate(list.ListElements());
	}

	public override AlertReport GetReport()
	{
		if (AllowToolController.Instance.Handles.StorageSpaceAlertSetting.Value)
		{
			List<GlobalTargetInfo> designatedThingsWithoutStorageSpace = AllowToolController.Instance.HaulUrgentlyCache.GetDesignatedThingsWithoutStorageSpace();
			if (designatedThingsWithoutStorageSpace.Count > 0)
			{
				return AlertReport.CulpritsAre(designatedThingsWithoutStorageSpace);
			}
		}
		return AlertReport.Inactive;
	}
}
