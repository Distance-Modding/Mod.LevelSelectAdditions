using Distance.LevelSelectAdditions.Scripts.Menus;
using HarmonyLib;
using System.Collections.Generic;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to create and prepare the Level Set Options Menus for use.
	/// </summary>
	[HarmonyPatch(typeof(OptionsMenuLogic), nameof(OptionsMenuLogic.GetSubmenus))]
	internal static class OptionsMenuLogic__GetSubmenus
	{
		[HarmonyPrefix]
		internal static void Prefix(OptionsMenuLogic __instance)
		{
			// Okay. So GetSubmenus is called numerous times during the lifetime of the game
			//  (whenever switching between in-level and in-menu).
			// Because of that, these menus and everything are assigned and created multiple times.
			// The real gotcha is *that's normal*. Unity has some jank-fu that auto-assigns objects to be
			// treated as null when their container/scene(?) is disposed of. So null checks would act as if this
			// was called for the first time.
			// tl;dr: Keep things as they are, let them be instantiated again, and again, and again, and again...

			Mod.Instance.OptionsMenu = __instance;


			LevelSetOptionsMenu levelSetOptionsMenu = __instance.GetOrAddComponent<LevelSetOptionsMenu>();

			levelSetOptionsMenu.RemoveFancyFadeIn();

			Mod.Instance.LevelSetOptionsMenu = levelSetOptionsMenu;

			List<OptionsSubmenu> menus = new List<OptionsSubmenu>(__instance.subMenus_);

			foreach (var menu in __instance.subMenus_)
			{
				if (menu.Name_ == levelSetOptionsMenu.Name_)
				{
					menus.Remove(menu);
				}
			}

			menus.Add(levelSetOptionsMenu);

			__instance.subMenus_ = menus.ToArray();
		}
	}
}
