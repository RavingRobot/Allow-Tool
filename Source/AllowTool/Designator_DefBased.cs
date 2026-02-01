using AllowTool.Context;
using RimWorld;
using UnityEngine;
using Verse;

namespace AllowTool;

public abstract class Designator_DefBased : Designator, IReversePickableDesignator, IGlobalHotKeyProvider
{
	private bool visible = true;

	public ThingDesignatorDef Def { get; private set; }

	public override bool Visible => visible;

	public KeyBindingDef GlobalHotKey => Def.hotkeyDef;

	protected Designator_DefBased()
	{
		useMouseIcon = true;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
	}

	protected void UseDesignatorDef(ThingDesignatorDef def)
	{
		Def = def;
		defaultLabel = def.label;
		defaultDesc = def.description;
		soundSucceeded = def.soundSucceeded;
		hotKey = def.hotkeyDef;
		visible = AllowToolController.Instance.Handles.IsDesignatorEnabled(def);
		ResolveIcon();
		OnDefAssigned();
	}

	protected virtual void OnDefAssigned()
	{
	}

	protected virtual void ResolveIcon()
	{
		Def.GetIconTexture(delegate(Texture2D tex)
		{
			icon = tex;
		});
	}

	public virtual Designator PickUpReverseDesignator()
	{
		return this;
	}
}
