using Distance.LevelSelectAdditions.Extensions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to handle selecting the last-accessed playlist, instead of whichever is the first to
	/// hold the last-accessed level.
	/// <para/>
	/// Additionally, this patch enables displaying The Other Side as a proper Sprint Campaign.
	/// </summary>
	[HarmonyPatch(typeof(LevelGridMenu), nameof(LevelGridMenu.CreateEntries))]
	internal static class LevelGridMenu__CreateEntries
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling 1/2...");
			// VISUAL: PATCH 1/2
			//if (isAdventure && this.profileProgress_.CollectedAllCrabs())
			//{
			//	unlocked    = true;
			//	unlockStyle = LevelGridMenu.PlaylistEntry.UnlockStyle.None;
			//	gameModeID  = GameModeID.TheOtherSide;
			//	this.CreateAndAddCampaignLevelSet(set, "The Other Side", unlocked, unlockStyle, gameModeID);
			//}
			// -to-
			//if (GetTheOtherSideCampaignVisible_(this) && this.profileProgress_.CollectedAllCrabs())
			//{
			//	unlocked    = GetTheOtherSideCampaignUnlocked_(this);
			//	unlockStyle = GetTheOtherSideCampaignUnlockStyle_(this);
			//	gameModeID  = GetTheOtherSideCampaignGameModeID_(this);
			//	this.CreateAndAddCampaignLevelSet(set, "The Other Side", unlocked, unlockStyle, gameModeID);
			//}

			for (int i = 10; i < codes.Count; i++)
			{
				if ((codes[i - 10].opcode.Name.StartsWith("ldloc")) && // isAdventure
					(codes[i - 9].opcode == OpCodes.Brfalse) &&

					(codes[i - 6].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i - 6].operand).Name == "CollectedAllCrabs") &&
					(codes[i - 5].opcode == OpCodes.Brfalse) &&

					(codes[i - 4].opcode == OpCodes.Ldc_I4_1) && // true
					(codes[i - 2].opcode == OpCodes.Ldc_I4_7) && // UnlockStyle.None
					(codes[i    ].opcode == OpCodes.Ldc_I4_S && codes[i    ].operand.ToString() == "16")) // GameModeID.TheOtherSide
				{
					Mod.Instance.Logger.Info($"callvirt CollectedAllCrabs @ {i-6}");

					// Force adventure check to true, because the parent block is only hit for Adventure/Sprint.
					// Replace: ldloc. (isAdventure)
					//// With:    ldc.i4.1 (true)
					// With:    ldarg.0
					// With:    call GetTheOtherSideCampaignVisible_
					//codes[i - 10].opcode = OpCodes.Ldc_I4_1;
					//codes[i - 10].operand = null;
					codes.RemoveAt(i - 10);
					codes.InsertRange(i - 10, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelGridMenu__CreateEntries).GetMethod(nameof(GetTheOtherSideCampaignVisible_))),
					});
					i += 1; // preserve i instruction offset

					// Replace: ldc.i4.1 (true)
					// With:    ldarg.0
					// With:    call GetTheOtherSideCampaignUnlocked_
					codes.RemoveAt(i - 4);
					codes.InsertRange(i - 4, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelGridMenu__CreateEntries).GetMethod(nameof(GetTheOtherSideCampaignUnlocked_))),
					});
					i += 1; // preserve i instruction offset

					// Replace: ldc.i4.7 (UnlockStyle.None)
					// With:    ldarg.0
					// With:    call GetTheOtherSideCampaignUnlockStyle_
					codes.RemoveAt(i - 2);
					codes.InsertRange(i - 2, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelGridMenu__CreateEntries).GetMethod(nameof(GetTheOtherSideCampaignUnlockStyle_))),
					});
					i += 1; // preserve i instruction offset

					// Replace: ldc.i4.s 16 (GameModeID.TheOtherSide)
					// With:    ldarg.0
					// With:    call GetTheOtherSideCampaignGameModeID_
					codes.RemoveAt(i);
					codes.InsertRange(i, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelGridMenu__CreateEntries).GetMethod(nameof(GetTheOtherSideCampaignGameModeID_))),
					});
					i += 1; // preserve i instruction offset

					break;
				}
			}

			Mod.Instance.Logger.Info("Transpiling 2/2...");
			// VISUAL: PATCH 2/2
			//if (__instance.buttonList_.ScrollableEntryCount_ > 0)
			//{
			//	...
			//	 -to-
			//	HandleSelectPlaylistEntry_(this);
			//}

			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 3].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i - 3].operand).Name == "get_ScrollableEntryCount_") &&
					(codes[i - 2].opcode == OpCodes.Ldc_I4_0) &&
					(codes[i - 1].opcode == OpCodes.Ble))
				{
					// i is first instruction in block.
					Mod.Instance.Logger.Info($"callvirt get_ScrollableEntryCount_ @ {i-3}");

					for (int j = i; j < codes.Count; j++)
					{
						if ((codes[j].opcode == OpCodes.Call && ((MethodInfo)codes[j].operand).Name == "DoNextFrame"))
						{
							Mod.Instance.Logger.Info($"call DoNextFrame @ {j}");

							// j is last instruction in block.

							// Replace: all instructions inside block.
							// With:    ldarg.0
							// With:    call HandleSelectPlaylistEntry_
							codes.RemoveRange(i, j - i + 1);
							codes.InsertRange(i, new CodeInstruction[]
							{
								new CodeInstruction(OpCodes.Ldarg_0, null),
								new CodeInstruction(OpCodes.Call, typeof(LevelGridMenu__CreateEntries).GetMethod(nameof(HandleSelectPlaylistEntry_))),
							});

							break;
						}
					}

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		// NOTE: This is a replacement for the isAdventure condition checked before `CollectedAllCrabs()`.
		public static bool GetTheOtherSideCampaignVisible_(LevelGridMenu levelGridMenu)
		{
			if (levelGridMenu.displayType_ == LevelSelectMenuAbstract.DisplayType.Adventure)
			{
				return true;
			}
			else
			{
				return Mod.Instance.Config.EnableTheOtherSideSprintCampaign;
			}
		}

		// NOTE: 'Unlocked' is NOT whether the campaign level set displays at all, but whether it can be accessed.
		//       The 'Visible' check is always handled by `CollectedAllCrabs()` and the above helper function.
		public static bool GetTheOtherSideCampaignUnlocked_(LevelGridMenu levelGridMenu)
		{
			if (levelGridMenu.displayType_ == LevelSelectMenuAbstract.DisplayType.Adventure)
			{
				return true;
			}
			else
			{
				return levelGridMenu.profileProgress_.LevelsCompletedInModePercentage(GameModeID.TheOtherSide) >= 1f;
			}
		}

		public static LevelGridMenu.PlaylistEntry.UnlockStyle GetTheOtherSideCampaignUnlockStyle_(LevelGridMenu levelGridMenu)
		{
			if (levelGridMenu.displayType_ == LevelSelectMenuAbstract.DisplayType.Adventure)
			{
				return LevelGridMenu.PlaylistEntry.UnlockStyle.None;
			}
			else
			{
				return LevelGridMenu.PlaylistEntry.UnlockStyle.SprintCampaign;
			}
		}

		public static GameModeID GetTheOtherSideCampaignGameModeID_(LevelGridMenu levelGridMenu)
		{
			if (levelGridMenu.displayType_ == LevelSelectMenuAbstract.DisplayType.Adventure)
			{
				return GameModeID.TheOtherSide;
			}
			else
			{
				return GameModeID.Sprint;
			}
		}


		public static void HandleSelectPlaylistEntry_(LevelGridMenu levelGridMenu)
		{
			List<LevelGridMenu.PlaylistEntry> entries = levelGridMenu.ScrollableEntries_;
			int selectedIndex = -1;

			// First try to find the selected entry by state stored for the last-selected playlist.
			string lastLevelSetID = Mod.Instance.Config.GetStateLastLevelSetID(levelGridMenu.displayType_, levelGridMenu.modeID_);
			if (!string.IsNullOrEmpty(lastLevelSetID))
			{
				selectedIndex = entries.FindIndex((LevelGridMenu.PlaylistEntry val) => val.playlist_.GetLevelSetID() == lastLevelSetID);
			}

			// If that fails, then fallback to the default method.
			string absoluteLevelPath = levelGridMenu.GetSelectedAbsoluteLevelPath();
			if (selectedIndex == -1 && !string.IsNullOrEmpty(absoluteLevelPath))
			{
				selectedIndex = entries.FindIndex((LevelGridMenu.PlaylistEntry val) => val.ContainsLevelPath(absoluteLevelPath));
			}

			// Default to first playlist if selection still not found.
			if (selectedIndex == -1)
			{
				selectedIndex = 0;
			}

			levelGridMenu.selectedEntry_ = entries[selectedIndex];
			levelGridMenu.DoNextFrame(delegate {
				levelGridMenu.SelectEntry(levelGridMenu.selectedEntry_, true);
			});
		}

		#endregion
	}
}
