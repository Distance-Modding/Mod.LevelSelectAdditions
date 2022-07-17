using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Scripts;
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
	/// Patch to add the LEVEL SET OPTIONS button to the bottom left button list when entering
	/// a Level Set grid view.
	/// </summary>
	/// <remarks>
	/// Required For: Level Set Options menu.
	/// </remarks>
	[HarmonyPatch(typeof(LevelGridGrid), nameof(LevelGridGrid.PushGrid))]
	internal static class LevelGridGrid__PushGrid
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//menuPanel.Push();
			//this.GridPushChange();
			// -to-
			//AddMenuPanelButtons_(this);
			//menuPanel.Push();
			//this.GridPushChange();

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 2].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i - 2].operand).Name == "Push") &&
					(codes[i    ].opcode == OpCodes.Call     && ((MethodInfo)codes[i    ].operand).Name == "GridPushChange"))
				{
					Mod.Instance.Logger.Info($"call MenuPanel.Push @ {i-2}");

					// Insert:  ldarg.0
					// Insert:  call AddMenuPanelButtons_
					// Before:  ldloc. (menuPanel)
					// Before:  callvirt MenuPanel.Push
					// NOTE: (i - 3) to insert before the ldloc used to call the MenuPanel.Push instance method.
					codes.InsertRange(i - 3, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelGridGrid__PushGrid).GetMethod(nameof(AddMenuPanelButtons_))),
					});

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		public static void AddMenuPanelButtons_(LevelGridGrid levelGridGrid)
		{
			levelGridGrid.GetOrAddComponent<LevelGridLevelSetOptionsLogic>();

			if (Mod.Instance.Config.EnableLevelSetOptionsMenu)
			{
				MenuPanel menuPanel = levelGridGrid.gridPanel_.GetComponent<MenuPanel>();
				if (menuPanel && !levelGridGrid.levelGridMenu_.IsSimpleMenu_)
				{
					if (!levelGridGrid.playlist_.IsResourcesPlaylist())
					{
						menuPanel.SetBottomLeftButton(InputAction.MenuStart, "PLAYLIST\nOPTIONS");
					}
					else if (Mod.BasicLevelSetOptionsSupported)
					{
						menuPanel.SetBottomLeftButton(InputAction.MenuStart, "LEVEL SET\nOPTIONS");
					}
				}
			}
		}

		#endregion
	}
}
