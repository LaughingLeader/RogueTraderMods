using HarmonyLib;

using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MoreInfo.Mod.Patches
{
    public class CombatLogPatch : IPatch
    {
		public bool IsEnabled { get; set; }

		private static readonly MethodInfo m_Roll = AccessTools.Method(typeof(TurnController), nameof(TurnController.TryRollInitiative));
		//private static readonly MethodInfo m_OnTrigger = AccessTools.Method(typeof(RuleRollInitiative), nameof(RuleRollInitiative.OnTrigger));

		//private static readonly MethodInfo m_RollInitiative = AccessTools.Method(typeof(InitiativeHelper), nameof(InitiativeHelper.RollInitiative));
		//private static readonly MethodInfo m_RollInitiative2 = AccessTools.Method(typeof(InitiativeHelper), nameof(InitiativeHelper.ApplyInitiative));
		//private static readonly MethodInfo m_RollInitiative3 = AccessTools.Method(typeof(InitiativeHelper), nameof(InitiativeHelper.UpdateBuffsInitiative));

		public void Enable(Type thisType, Harmony harmony)
		{
			harmony.Patch(m_Roll, postfix: new HarmonyMethod(thisType, nameof(OnInitiativeRollingDone)));

			/*foreach(var unit in Game.Instance.Player.Party)
			{
				if(unit.Features.IsFirstInFight.Value == true)
				{
					Plugin.Log($"[{unit.Name}] IsFirstInFight Buffs({unit.Features.IsFirstInFight.AssociatedBuffs.Buffs.Count()})");
					if (unit.Features.IsFirstInFight.AssociatedBuffs.Buffs.Count() == 0)
					{
						unit.Features.IsFirstInFight.ReleaseAll();
						Plugin.Log($"Fixed IsFirstInFight being set for [{unit.Name}]");
					}
				}
			}*/

			/*harmony.Patch(m_OnTrigger, postfix: new HarmonyMethod(thisType, nameof(DebugInitiativeRoll)));
			harmony.Patch(m_RollInitiative, postfix: new HarmonyMethod(thisType, nameof(DebugRollInitiative)));
			harmony.Patch(m_RollInitiative2, postfix: new HarmonyMethod(thisType, nameof(DebugApplyInitiative)));
			harmony.Patch(m_RollInitiative3, postfix: new HarmonyMethod(thisType, nameof(DebugUpdateBuffsInitiative)));*/
			//Plugin.Log($"TurnOrder:\n{String.Join(System.Environment.NewLine, Game.Instance.Player.Party.OrderByDescending(x => x.Initiative.Value).Select(x => x.Name + ": " + PrintInitiative(x)))}");
		}

		public void Disable(Type thisType, Harmony harmony)
		{
			harmony.Unpatch(m_Roll, thisType.GetMethod(nameof(OnInitiativeRollingDone)));

			/*harmony.Unpatch(m_OnTrigger, thisType.GetMethod(nameof(DebugInitiativeRoll)));
			harmony.Unpatch(m_RollInitiative, thisType.GetMethod(nameof(DebugRollInitiative)));
			harmony.Unpatch(m_RollInitiative2, thisType.GetMethod(nameof(DebugApplyInitiative)));
			harmony.Unpatch(m_RollInitiative3, thisType.GetMethod(nameof(DebugUpdateBuffsInitiative)));*/
		}

		private static readonly string InitiativeMessage = "{0}: {1} ((AGI: {2}, PER: {3})";

		//private static void OnInitiativeRollingDone(ref IEnumerable<MechanicEntity> newCombatants, ref bool relax)
		private static void OnInitiativeRollingDone(ref List<MechanicEntity> ___m_JoinedThisTickEntities)
		{
			if (___m_JoinedThisTickEntities.Count > 0)
			{
				var tooltip = new TooltipTemplateGlossary("Initiative");
				var thread = InGameLog.GetThread<UnitInitiativeLogThread>(LogChannelType.AnyCombat);
				foreach (var entity in ___m_JoinedThisTickEntities.OrderByDescending(x => x.Initiative.Roll))
				{
					if (entity != null)
					{
						var agilityBonus = entity.GetStatOptional<ModifiableValueAttributeStat>(StatType.WarhammerAgility)?.WarhammerBonus ?? 0;
						var perceptionBonus = entity.GetStatOptional<ModifiableValueAttributeStat>(StatType.WarhammerPerception)?.WarhammerBonus ?? 0;
						var msg = String.Format(InitiativeMessage, entity.Name, Math.Floor(entity.Initiative.Roll), agilityBonus, perceptionBonus);
						InGameLog.AddMessage(thread, msg, Kingmaker.UI.Models.Log.Enums.PrefixIcon.None, tooltip);
					}
				}
			}

		}

		private static string PrintInitiative(MechanicEntity entity)
		{
			var initiative = entity.Initiative;
			var overrideInitiative = entity.GetCombatStateOptional()?.OverrideInitiative;
			return $"IsFirstInFight({entity.Features.IsFirstInFight}) IsInPlayerParty({entity.IsInPlayerParty}) IsPlayerFaction({entity.IsPlayerFaction}) Roll({initiative.Roll}) Value({initiative.Value}) TurnOrderPriority({initiative.TurnOrderPriority}) Order({initiative.Order}) OverrideInitiative({overrideInitiative}) Agility({entity.GetStatOptional<ModifiableValueAttributeStat>(StatType.WarhammerAgility)?.WarhammerBonus}) Perception({entity.GetStatOptional<ModifiableValueAttributeStat>(StatType.WarhammerPerception)?.WarhammerBonus})";
		}

		private static void DebugInitiativeRoll(ref RuleRollInitiative __instance)
		{
			Log.Info($"[RuleRollInitiative] Result({__instance.Result}) Modifiers.Value({__instance.Modifiers.Value})");
		}
		private static void DebugRollInitiative(ref MechanicEntity entity)
		{
			if (entity.Name != "Cassia") return;
			Log.Info($"[RollInitiative] {entity.Name + ": " + PrintInitiative(entity)}");
		}

		private static bool FixApplyInitiative(MechanicEntity entity)
		{
			if (entity.Name == "Cassia")
			{
				//Initiative initiative = entity.Initiative;
				float value = entity.Initiative.Roll;
				Log.Info($"[FixApplyInitiative] Roll({entity.Initiative.Roll}) => {value}");
				//initiative.Value = value;
				entity.Initiative.Order = InitiativeHelper.CalculateOrder(entity);
				return false;
			}
			return true;
		}

		private static void DebugApplyInitiative(ref MechanicEntity entity)
		{
			if (entity.Name != "Cassia") return;
			Log.Info($"[ApplyInitiative] {entity.Name + ": " + PrintInitiative(entity)}");
		}

		private static void DebugUpdateBuffsInitiative(ref MechanicEntity entity)
		{
			if (entity.Name != "Cassia") return;
			Log.Info($"[UpdateBuffsInitiative] {entity.Name + ": " + PrintInitiative(entity)}");
		}
	}
}
