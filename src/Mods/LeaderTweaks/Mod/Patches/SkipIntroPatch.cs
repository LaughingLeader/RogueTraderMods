using HarmonyLib;

using Kingmaker.UI.Legacy.MainMenuUI;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LeaderTweaks.Mod.Patches
{
	[HarmonyPatch(typeof(SplashScreenController), nameof(SplashScreenController.ShowSplashScreen))]
	public static class SkipIntroPatch
	{
		private static bool Prefix(SplashScreenController __instance)
		{
			if (!Main.IsEnabled || !Main.Settings.SkipIntro) return true;
			__instance.StartCoroutine(__instance.SkipWaitingSplashScreens());
			//__instance.Complete();
			return false;
		}
	}
}
