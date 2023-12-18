using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leader;
using Kingmaker.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using static Kingmaker.UnitLogic.Mechanics.Actions.ContextActionDealDamage;

namespace LeaderTweaks.Mod.Patches
{
	[HarmonyPatch(typeof(ContextActionDealDamage), nameof(ContextActionDealDamage.DealHitPointsDamage))]
	public static class PainChannelingFix
	{
		private static readonly string PainChannelingFeature = "dee534eedb7a4478b525630c39ec1c96";

		private static bool Prefix(ref int __result, ContextActionDealDamage __instance)
		{
			if (!Main.IsEnabled || !Main.Settings.Fixes.DisablePainChannelingSelfDamage) return true;

			if (__instance.Owner?.AssetGuid == PainChannelingFeature && __instance.TargetEntity == __instance.Context.MaybeOwner)
			{
				__result = 0;
				return false;
			}
			return true;
		}
	}
}
