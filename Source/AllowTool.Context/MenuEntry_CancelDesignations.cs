using RimWorld;
using Verse;

namespace AllowTool.Context;

public class MenuEntry_CancelDesignations : BaseContextMenuEntry
{
	protected override string SettingHandleSuffix => "cancelDesginations";

	protected override string BaseTextKey => "Designator_context_cancel_desig";

	public override ActivationResult Activate(Designator designator, Map map)
	{
		int num = 0;
		int num2 = 0;
		DesignationManager designationManager = map.designationManager;
		Designation[] array = designationManager.AllDesignations.ToArray();
		foreach (Designation designation in array)
		{
			if (designation.def != null && designation.def.designateCancelable && designation.def != DesignationDefOf.Plan)
			{
				if (designation.target.Thing != null)
				{
					num++;
				}
				else
				{
					num2++;
				}
				designationManager.RemoveDesignation(designation);
			}
		}
		return ActivationResult.SuccessMessage("Designator_context_cancel_desig_msg".Translate(num, num2));
	}
}
