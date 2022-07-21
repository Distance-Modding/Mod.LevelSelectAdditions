using Distance.LevelSelectAdditions.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to fix "QUICK PLAYLIST..." text overwriting playlist name after adding a level (even in vanilla).
	/// <para/>
	/// Also includes patch to make the Visit Workshop page button visible for the Choose Main Menu display type
	/// (when first opening the level select menu).
	/// <para/>
	/// Also includes patch to hide the Start Playlist button when in Playlist Mode for the Choose Main Menu display type.
	/// <para/>
	/// Also includes patch to hide unused Leaderboards (and optionally Playlist Mode) buttons when in the Choose Main Menu display type.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.SetupLevelPlaylistVisuals))]
	internal static class LevelSelectMenuLogic__SetupLevelPlaylistVisuals
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelSelectMenuLogic __instance)
		{
			// We *might* be entering Playlist Mode, so we need to re-evaluate bottom left button visibility.
			__instance.UpdateBottomLeftButtonVisibility();
		}

		[HarmonyPostfix]
		internal static void Postfix(LevelSelectMenuLogic __instance)
		{
			// We need to update the Quick Playlist label because every call to this function assigns its
			//  text value to QUICK PLAYLIST... which we don't want when we have a named playlist.
			__instance.UpdateQuickPlaylistText();
		}
		
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling (1/2)...");
			// VISUAL:
			//if (... && this.displayType_ != LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel && ...)
			// -to-
			//if (... && AllowMainMenuDisplayTypeForVisitWorkshop_(this) && ...)

			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 3].opcode == OpCodes.Ldarg_0)  &&
					(codes[i - 2].opcode == OpCodes.Ldfld     && ((FieldInfo)codes[i - 2].operand).Name == "displayType_") &&
					(codes[i - 1].opcode == OpCodes.Ldc_I4_2) && // (LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
					(codes[i].opcode == OpCodes.Beq))
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
						new CodeInstruction(OpCodes.Call, typeof(LevelSelectMenuLogic__SetupLevelPlaylistVisuals).GetMethod(nameof(AllowMainMenuDisplayTypeForVisitWorkshop_))),
					});
					i -= 1; // preserve i instruction offset

					codes[i].opcode = OpCodes.Brfalse; // preserve jump operand, just change the opcode

					break;
				}
			}

			Mod.Instance.Logger.Info("Transpiling (2/2)...");
			// VISUAL:
			//bool active = playlist_.Count > 0;
			//this.startPlaylistButton_.SetActive(active);
			// -to-
			//bool active = playlist_.Count > 0;
			//SetStartPlaylistButtonActive_(this, active);

			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 3].opcode == OpCodes.Ldarg_0) &&
					(codes[i - 2].opcode == OpCodes.Ldfld    && ((FieldInfo) codes[i - 2].operand).Name == "startPlaylistButton_") &&
					(codes[i    ].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i    ].operand).Name == "SetActive"))
				{
					Mod.Instance.Logger.Info($"callvirt SetActive @ {i}");

					// Replace: ldarg.0
					// Replace: ldfld startPlaylistButton_
					// Ignore:  ldloc.
					// Replace: callvirt SetActive
					// With:    ldarg.0
					// Ignore:  ldloc.
					// With:    call SetStartPlaylistButtonActive_
					codes.RemoveAt(i - 2);
					i -= 1; // preserve i instruction offset

					codes.RemoveAt(i);
					codes.InsertRange(i, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Call, typeof(LevelSelectMenuLogic__SetupLevelPlaylistVisuals).GetMethod(nameof(SetStartPlaylistButtonActive_))),
					});

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

		public static void SetStartPlaylistButtonActive_(LevelSelectMenuLogic levelSelectMenu, bool active)
		{
			// Never allow starting a playlist while in ChooseMainMenuLevel display type.
			if (levelSelectMenu.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
			{
				active = false;
			}
			levelSelectMenu.startPlaylistButton_.SetActive(active);
		}

		#endregion
	}
}