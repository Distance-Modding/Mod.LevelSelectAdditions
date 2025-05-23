using Distance.LevelSelectAdditions.Helpers;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to enable adding a selected level in Playlist Mode while in the Choose Main Menu level display type.
	/// <para/>
	/// Also includes patch to handle resetting the selected main menu (to remove level set selections).
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.OnLevelButtonClicked))]
	internal static class LevelSelectMenuLogic__OnLevelButtonClicked
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelSelectMenuLogic __instance)
		{
			if (__instance.CurrentLevelLocked_)
			{
				return;
			}
			if (__instance.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
			{
				MainMenuLevelSetHelper.SetMainMenuLevelSet(null);
			}
		}

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Log.LogInfo("Transpiling...");
			// VISUAL:
			//if (this.displayType_ != LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
			// -to-
			//if (AllowMainMenuDisplayTypeForQuickPlaylist_(this))

			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 3].opcode == OpCodes.Ldarg_0)  &&
					(codes[i - 2].opcode == OpCodes.Ldfld     && ((FieldInfo)codes[i - 2].operand).Name == "displayType_") &&
					(codes[i - 1].opcode == OpCodes.Ldc_I4_2) && // (LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
					(codes[i    ].opcode == OpCodes.Bne_Un))
				{
					Mod.Log.LogInfo($"ldfld displayType_ @ {i-2}");
					Mod.Log.LogInfo($"ldc.i4.2           @ {i-1}");

					// Replace: ldarg.0
					// Replace: ldfld displayType_
					// Replace: ldc.i4.2
					// Replace: bne.un (preserve jump label)
					// With:    ldarg.0
					// With:    call AllowMainMenuDisplayTypeForQuickPlaylist_
					// With:    brtrue (preserve jump label)
					codes.RemoveRange(i - 2, 2);
					codes.InsertRange(i - 2, new CodeInstruction[]
					{
						//new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelSelectMenuLogic__OnLevelButtonClicked).GetMethod(nameof(AllowMainMenuDisplayTypeForQuickPlaylist_))),
					});
					i -= 1; // instruction offset

					codes[i].opcode = OpCodes.Brtrue; // Preserve other instruction info, just change the opcode

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		public static bool AllowMainMenuDisplayTypeForQuickPlaylist_(LevelSelectMenuLogic levelSelectMenu)
		{
			// We need to ensure logic for ChooseMainMenu ONLY passes through if we're currently in Quick Playlist mode.
			if (levelSelectMenu.showingLevelPlaylist_ && Mod.EnableChooseMainMenuQuickPlaylist.Value)
			{
				return true;
			}
			return levelSelectMenu.displayType_ != LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
		}

		#endregion
	}
}
