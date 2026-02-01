using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AllowTool;

public class Designator_HarvestFullyGrown : Designator_SelectableThings
{
	protected override DesignationDef Designation => DesignationDefOf.HarvestPlant;

	public Designator_HarvestFullyGrown()
	{
		UseDesignatorDef(AllowToolDefOf.HarvestFullyGrownDesignator);
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		PlantProperties plantProperties = t?.def.plant;
		bool flag = t.HasDesignation(Designation);
		return plantProperties != null && !flag && t is Plant { HarvestableNow: not false, LifeStage: PlantLifeStage.Mature } && !SpecialTreeMassDesignationFix.IsSpecialTree(t) && PlantMatchesModifierKeyFilter(plantProperties);
	}

	public override void DesignateThing(Thing t)
	{
		if (CanDesignateThing(t).Accepted)
		{
			base.Map.designationManager.RemoveAllDesignationsOn(t);
			t.ToggleDesignation(Designation, enable: true);
		}
	}

	protected override bool RemoveAllDesignationsAffects(LocalTargetInfo target)
	{
		PlantProperties plant = target.Thing.def.plant;
		return plant != null && PlantMatchesModifierKeyFilter(plant);
	}

	private static bool PlantMatchesModifierKeyFilter(PlantProperties props)
	{
		bool shiftIsHeld = HugsLibUtility.ShiftIsHeld;
		bool controlIsHeld = HugsLibUtility.ControlIsHeld;
		return (!shiftIsHeld && !controlIsHeld) || (shiftIsHeld && plantIsCrop()) || (controlIsHeld && plantIsTree());
		bool plantIsCrop()
		{
			return props.harvestTag == "Standard";
		}
		bool plantIsTree()
		{
			return props.harvestTag == "Wood";
		}
	}
}
