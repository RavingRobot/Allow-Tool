using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AllowTool;

public class Designator_CutGrass : Designator_SelectableThings
{
    protected override DesignationDef Designation => DesignationDefOf.HarvestPlant;

    public Designator_CutGrass()
    {
        UseDesignatorDef(AllowToolDefOf.CutGrassDesignator);
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        PlantProperties plantProperties = t?.def.plant;
        bool flag = t.HasDesignation(Designation);
        if (t is not Plant plant || plant.Position.Fogged(plant.Map))
            return false;
        return plantProperties != null && !flag && PlantMatchesModifierKeyFilter(plant);
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
        if (target.Thing is Plant plant)
            return PlantMatchesModifierKeyFilter(plant);
        return false;
    }

    private static bool PlantMatchesModifierKeyFilter(Plant plant)
    {
        return plant.def.defName.Contains("Grass");
    }
}
