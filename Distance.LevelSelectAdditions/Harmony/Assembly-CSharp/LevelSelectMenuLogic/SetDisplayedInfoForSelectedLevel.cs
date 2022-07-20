using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to make the Visit Workshop page button visible for the Choose Main Menu display type
	/// (when changing selection in the level select menu).
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.SetDisplayedInfoForSelectedLevel))]
	internal static class LevelSelectMenuLogic__SetDisplayedInfoForSelectedLevel
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//if (this.displayType_ != LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel && ...)
			// -to-
			//if (AllowMainMenuDisplayTypeForVisitWorkshop_(this) && ...)

			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 3].opcode == OpCodes.Ldarg_0)  &&
					(codes[i - 2].opcode == OpCodes.Ldfld     && ((FieldInfo)codes[i - 2].operand).Name == "displayType_") &&
					(codes[i - 1].opcode == OpCodes.Ldc_I4_2) && // (LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
					(codes[i    ].opcode == OpCodes.Beq))
				{
					Mod.Instance.Logger.Info($"ldfld displayType_ @ {i-2}");
					Mod.Instance.Logger.Info($"ldc.i4.2           @ {i-1}");

					// After:   ldarg.0
					// Replace: ldfld displayType_
					// Replace: ldc.i4.2
					// Replace: beq (preserve jump label)
					// With:    call AllowMainMenuDisplayTypeForVisitWorkshop_
					// With:    brfalse (preserve jump label)
					codes.RemoveRange(i - 2, 2);
					codes.InsertRange(i - 2, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Call, typeof(LevelSelectMenuLogic__SetDisplayedInfoForSelectedLevel).GetMethod(nameof(AllowMainMenuDisplayTypeForVisitWorkshop_))),
					});
					i -= 1; // preserve i instruction offset

					codes[i].opcode = OpCodes.Brfalse; // preserve jump operand, just change the opcode

					break;
				}
			}

			return codes.AsEnumerable();
		}

		#region Helper Functions

		public static bool AllowMainMenuDisplayTypeForVisitWorkshop_(LevelSelectMenuLogic levelSelectMenu)
		{
			if (Mod.Instance.Config.EnableChooseMainMenuVisitWorkshopButton)
			{
				return true;
			}
			return levelSelectMenu.displayType_ != LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
		}

		#endregion
	}
}