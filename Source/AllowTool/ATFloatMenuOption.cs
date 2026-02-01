using System;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AllowTool;

public class ATFloatMenuOption : FloatMenuOption
{
	private const float WatermarkDrawSize = 30f;

	private const float MouseOverLabelShift = 4f;

	private readonly bool showWatermark;

	private readonly string tooltipText;

	public ATFloatMenuOption(string label, Action action, MenuOptionPriority priority = MenuOptionPriority.Default, Action<Rect> mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, string tooltipText = null)
		: base(label, action, priority, mouseoverGuiAction, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget)
	{
		this.tooltipText = tooltipText;
		showWatermark = AllowToolController.Instance.Handles.ContextWatermarkSetting;
		if (showWatermark)
		{
			base.Label = "      " + label;
		}
	}

	public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
	{
		bool result = base.DoGUI(rect, colonistOrdering, floatMenu);
		if (showWatermark)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width, rect.height - 1f);
			bool flag = !base.Disabled && Mouse.IsOver(rect2);
			Texture2D contextMenuWatermark = AllowToolDefOf.Textures.contextMenuWatermark;
			GUI.DrawTexture(new Rect(rect.x + (flag ? 4f : 0f), rect.y, 30f, 30f), contextMenuWatermark);
			if (tooltipText != null)
			{
				TooltipHandler.TipRegion(rect2, tooltipText);
			}
		}
		return result;
	}
}
