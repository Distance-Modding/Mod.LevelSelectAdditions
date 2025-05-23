using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to enable Playlist Mode button inputs for the Choose Main Menu display type.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.UpdateInput))]
	internal static class LevelSelectMenuLogic__UpdateInput
	{
		// Prefix patch version:
		/*[HarmonyPrefix]
		internal static void Prefix(LevelSelectMenuLogic __instance)
		{
			if (__instance.ignoreMenuInputForOneFrame_ || !G.Sys.MenuPanelManager_.MenuInputEnabled_ || __instance.SearchButtonSelected_)
			{
				return;
			}

			if (__instance.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
			{
				// Enable PLAYLIST MODE button in the Choose Main Menu level display type.
				if (__instance.inputManager_.GetKeyUp(InputAction.MenuSpecial_2, -2))
				{
					if (__instance.showingLevelPlaylist_)
					{
						if (__instance.tempPlaylist_.Count_ > 0)
						{
							__instance.menuPanelManager_.ShowOkCancel("Closing the playlist will clear it. Are you sure that you want to continue?",
								"Closing Playlist",
								new MessagePanelLogic.OnButtonClicked(__instance.ClearTempPlaylist),
								new MessagePanelLogic.OnButtonClicked(__instance.ClosingPlaylistPop),
								UIWidget.Pivot.Center);
						}
						else
						{
							__instance.ClearTempPlaylist();
						}
					}
					else
					{
						__instance.showingLevelPlaylist_ = true;
						__instance.SetupLevelPlaylistVisuals();
					}
				}

				// Enable REMOVE LEVEL button in the Choose Main Menu level display type.
				if (__instance.showingLevelPlaylist_ && __instance.inputManager_.GetKeyUp(InputAction.MenuSpecial_1, -2))
				{
					if (__instance.tempPlaylist_.Count_ > 0)
					{
						__instance.tempPlaylist_.Remove(__instance.tempPlaylist_.Count_ - 1);
					}
					__instance.SetupLevelPlaylistVisuals();
				}
			}
		}*/

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Log.LogInfo("Transpiling...");
			// VISUAL:
			//if (this.displayType_ != LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel && ...)
			// -to-
			//if (AllowMainMenuDisplayTypeForQuickPlaylist_(this) && ...)

			// 1st appearance: block to handle toggling Playlist Mode on/off
			// 2nd appearance: block to handle the REMOVE LEVEL input in Playlist Mode.
			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 3].opcode == OpCodes.Ldarg_0)  &&
					(codes[i - 2].opcode == OpCodes.Ldfld     && ((FieldInfo)codes[i - 2].operand).Name == "displayType_") &&
					(codes[i - 1].opcode == OpCodes.Ldc_I4_2) && // (LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
					(codes[i    ].opcode == OpCodes.Beq))
				{
					if (i >= 7)
					{
						if ((codes[i - 7].opcode == OpCodes.Ldc_I4_S && ((sbyte)     codes[i - 7].operand) == 44) &&
							(codes[i - 6].opcode == OpCodes.Ldc_I4_S && ((sbyte)     codes[i - 6].operand) == -2) &&
							(codes[i - 5].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i - 5].operand).Name == "GetKeyUp") &&
							(codes[i - 4].opcode == OpCodes.Brfalse))
						{
							Mod.Log.LogInfo($"ldc.i4.s 44 @ {i-7}");
							Mod.Log.LogInfo($"ldc.i4.s -2 @ {i-6}");
							Mod.Log.LogInfo($"callvirt GetKeyUp @ {i-5}");

							// This is the condition for OpenLeaderboards, which we don't want to enable for main menus.
							continue;
						}
					}

					Mod.Log.LogInfo($"ldfld displayType_ @ {i-2}");
					Mod.Log.LogInfo($"ldc.i4.2           @ {i-1}");

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
						new CodeInstruction(OpCodes.Call, typeof(LevelSelectMenuLogic__UpdateInput).GetMethod(nameof(AllowMainMenuDisplayTypeForQuickPlaylist_))),
					});
					i -= 1; // instruction offset

					codes[i].opcode = OpCodes.Brfalse; // Preserve other instruction info, just change the opcode


					// Don't break after first appearance, we need to patch this instruction pattern 2 times.
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		public static bool AllowMainMenuDisplayTypeForQuickPlaylist_(LevelSelectMenuLogic levelSelectMenu)
		{
			if (Mod.EnableChooseMainMenuQuickPlaylist.Value)
			{
				return true;
			}
			return levelSelectMenu.displayType_ != LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
		}

		#endregion
	}
}
