using HugsLib.Utils;
using RimWorld;
using System.Reflection;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace AllowTool;

public class Designator_ChopAllTrees : Designator_SelectableThings
{
    protected override DesignationDef Designation => DesignationDefOf.HarvestPlant;

    public Designator_ChopAllTrees()
    {
        UseDesignatorDef(AllowToolDefOf.ChopAllTreesDesignator);
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        PlantProperties plantProperties = t?.def.plant;
        bool flag = t.HasDesignation(Designation);
        if (t is not Plant plant || plant.Position.Fogged(plant.Map))
            return false;

        // Обход HarvestableNow для ростков
        if (!plant.HarvestableNow)
        {
            var prop = typeof(Plant).GetProperty("HarvestableNow", BindingFlags.NonPublic | BindingFlags.Instance);
            prop?.SetValue(plant, true);
        }

        return plantProperties != null && !flag && !SpecialTreeMassDesignationFix.IsSpecialTree(t) && PlantMatchesModifierKeyFilter(plant);
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
        PlantProperties props = plant.def.plant;
        return props.IsTree || plant.def.defName.StartsWith("Plant_Tree");
    }
}
