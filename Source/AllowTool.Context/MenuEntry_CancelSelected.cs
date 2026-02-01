using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AllowTool.Context;

public class MenuEntry_CancelSelected : BaseContextMenuEntry
{
	protected override string SettingHandleSuffix => "cancelSelected";

	protected override string BaseTextKey => "Designator_context_cancel_selected";

	public override ActivationResult Activate(Designator designator, Map map)
	{
		HashSet<object> selectedObjects = new HashSet<object>(Find.Selector.SelectedObjects);
		HashSet<IntVec3> selectedTilePositions = new HashSet<IntVec3>(from t in selectedObjects
			where t is Thing
			select ((Thing)t).Position);
		DesignationDef[] array = (from des in map.designationManager.AllDesignations
			where des.target.HasThing ? selectedObjects.Contains(des.target.Thing) : selectedTilePositions.Contains(des.target.Cell)
			select des.def).Distinct().ToArray();
		HashSet<LocalTargetInfo> hashSet = new HashSet<LocalTargetInfo>();
		Designation[] array2 = map.designationManager.AllDesignations.ToArray();
		foreach (Designation designation in array2)
		{
			if (array.Contains(designation.def))
			{
				map.designationManager.RemoveDesignation(designation);
				hashSet.Add(designation.target);
			}
		}
		return (hashSet.Count > 0) ? ActivationResult.Success(BaseMessageKey, array.Length, hashSet.Count) : ActivationResult.Failure(BaseMessageKey);
	}
}
