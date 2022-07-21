using Distance.LevelSelectAdditions.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to enable actually opening Playlist Mode panel when in the Choose Main Menu level display type.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.OpenLevelPlaylistMenu))]
	internal static class LevelSelectMenuLogic__OpenLevelPlaylistMenu
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelSelectMenuLogic __instance)
		{
			// Change the input field to the current 'known' file name if this playlist was loaded, or has previously been saved.
			string filePath = __instance.tempPlaylist_.GetFilePath();
			string fileName = null;
			if (filePath != null)
			{
				fileName = Resource.GetFileNameWithoutExtension(filePath);
			}

			__instance.levelPlaylistSelectMenu_.levelPathInput_.Value_ = fileName ?? string.Empty; // Should null be used here instead(?)
		}

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//if (this.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel || ...)
			// -to-
			//if (!AllowMainMenuDisplayTypeForQuickPlaylist_(this) || ...)

			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 3].opcode == OpCodes.Ldarg_0)  &&
					(codes[i - 2].opcode == OpCodes.Ldfld     && ((FieldInfo)codes[i - 2].operand).Name == "displayType_") &&
					(codes[i - 1].opcode == OpCodes.Ldc_I4_2) && // (LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
					(codes[i    ].opcode == OpCodes.Beq))
				{
					Mod.Instance.Logger.Info($"ldfld displayType_ @ {i-2}");
					Mod.Instance.Logger.Info($"ldc.i4.2           @ {i-1}");

					// Replace: ldarg.0
					// Replace: ldfld displayType_
					// Replace: ldc.i4.2
					// Replace: beq (preserve jump label)
					// With:    ldarg.0
					// With:    call AllowMainMenuDisplayTypeForQuickPlaylist_
					// With:    brfalse (preserve jump label)
					codes.RemoveRange(i - 2, 2);
					codes.InsertRange(i - 2, new CodeInstruction[]
					{
						//new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelSelectMenuLogic__OpenLevelPlaylistMenu).GetMethod(nameof(AllowMainMenuDisplayTypeForQuickPlaylist_))),
					});
					i -= 1; // instruction offset

					codes[i].opcode = OpCodes.Brfalse; // Preserve other instruction info, just change the opcode

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		public static bool AllowMainMenuDisplayTypeForQuickPlaylist_(LevelSelectMenuLogic levelSelectMenu)
		{
			if (Mod.Instance.Config.EnableChooseMainMenuQuickPlaylist)
			{
				return true;
			}
			return levelSelectMenu.displayType_ != LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
		}

		#endregion
	}
}
