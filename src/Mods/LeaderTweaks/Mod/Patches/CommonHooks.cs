using HarmonyLib;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Progression.Features;

using Leader;

using LeaderTweaks.Mod.Patches.QOL;
using LeaderTweaks.Mod.Patches.Talents;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderTweaks.Mod.Patches
{
	[HarmonyPatch]
	public static class CommonHooks
	{
		[HarmonyPatch(typeof(TurnController), nameof(TurnController.TryRollInitiative)), HarmonyPostfix]
		private static void OnCombatStarted()
		{
			if (!Main.IsEnabled || Game.Instance.TurnController.CombatRound != 1) return;

			if(!Game.Instance.IsSpaceCombat)
			{
				if (Main.Settings.Talents.AutoRelentlessBlaze) RelentlessBlaze.OnCombatStarted();
			}
		}

		/* This is a prefix so log spam is avoided, since the original function sends out an event via Rulebook.Trigger(new RuleCalculateDamage), which reports the previous damage.
		 */
		[HarmonyPatch(typeof(ContextActionDealDamage), nameof(ContextActionDealDamage.DealHitPointsDamage)), HarmonyPrefix]
		private static bool OnDamage(ContextActionDealDamage __instance, ref int __result)
		{
			if (!Main.IsEnabled) return true;

			if(Main.Settings.Talents.RelentlessBlazeBurningDealsZeroDamage && !RelentlessBlaze.OnDamage(__instance, ref __result))
			{
				return false;
			}
			if(Main.Settings.Talents.DisablePainChannelingSelfDamage && !PainChanneling.OnDamage(__instance, ref __result))
			{
				return false;
			}
			return true;
		}
	}
}
