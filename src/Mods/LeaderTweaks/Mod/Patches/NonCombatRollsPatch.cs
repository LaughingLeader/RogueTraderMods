using HarmonyLib;

using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderTweaks.Mod.Patches
{
	[HarmonyPatch]
	public static class NonCombatRollsPatch
	{
		private static bool CanAlterRoll(BaseUnitEntity? unit) => unit?.IsPlayerFaction == true && !unit.IsInCombat;
		private static int GetStatVal(MechanicEntity unit, StatType statType) => unit.GetStatOptional(statType)?.ModifiedValue ?? 0;

		//For skill checks that have a 0% chance to succeed, this allows passing them by ignoring the m_DifficultyClass.
		[HarmonyPatch(typeof(RulePerformPartySkillCheck), nameof(RulePerformPartySkillCheck.CreateSkillCheck))]
		[HarmonyPrefix]
		private static bool RulePerformSkillCheckOverride(RulePerformPartySkillCheck __instance, ref RulePerformSkillCheck __result)
		{
			if (!Main.IsEnabled || !Main.Settings.AlwaysSucceedNonCombatSkillChecks) return true;

			if (__instance.m_SourceCheck == null && CanAlterRoll(__instance.InitiatorUnit))
			{
				if(__instance.Initiator is MechanicEntity entity && GetStatVal(entity, __instance.m_StatType) + __instance.m_DifficultyClass <= 0)
				{
					__result = new RulePerformSkillCheck(__instance.Roller, __instance.m_StatType, 100)
					{
						Voice = __instance.Voice,
						EnsureSuccess = __instance.EnsureSuccess
					};
					return false;
				}
			}
			return true;
		}

		[HarmonyPatch(typeof(RuleRollDice), nameof(RuleRollDice.Roll)), HarmonyPostfix]
		private static void RuleRollDiceOnRoll(RuleRollDice __instance)
		{
			if (!Main.IsEnabled || !Main.Settings.AlwaysSucceedNonCombatSkillChecks) return;

			if (Rulebook.CurrentContext.Current is RuleRollChance chanceRoll)
			{
				if (chanceRoll.RollTypeValue == RollType.Skill && CanAlterRoll(__instance.InitiatorUnit))
				{
					if(chanceRoll.Chance > 0)
					{
						__instance.m_Result = chanceRoll.Chance;
					}
#if DEBUG
					else
					{
						Log.Info($"[RuleRollDice.Roll({chanceRoll.Chance})]: {new System.Diagnostics.StackTrace()}");
					}
#endif
				}
			}
		}
	}
}
