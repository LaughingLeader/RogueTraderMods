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
using Kingmaker.TextTools;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace MoreInfo.Mod.Patches
{
	[HarmonyPatch]
    public static class CombatLogPatch
    {
		private static readonly string InitiativeMessage = "{0}: Rolled {1} initiative";
		private static readonly string InitiativeMessageWithBonuses = "{0}: Rolled {1} initiative ({2} AGI, {3} PER)";

		private static void AddMessagesAsOne(List<MechanicEntity> ___m_JoinedThisTickEntities)
		{
			if (___m_JoinedThisTickEntities.Count > 0)
			{
				var tooltip = new TooltipTemplateGlossary("Initiative");
				var thread = InGameLog.GetThread<UnitInitiativeLogThread>(LogChannelType.AnyCombat);
				var messages = new List<string>() { "Initiative Rolls:" };
				InGameLog.AddMessage(thread, "Initiative Rolls:", PrefixIcon.RightArrow, tooltip);
				foreach (var entity in ___m_JoinedThisTickEntities.OrderByDescending(x => x.Initiative.Roll))
				{
					if (entity != null)
					{
						var agilityBonus = entity.GetStatOptional<ModifiableValueAttributeStat>(StatType.WarhammerAgility)?.WarhammerBonus ?? 0;
						var perceptionBonus = entity.GetStatOptional<ModifiableValueAttributeStat>(StatType.WarhammerPerception)?.WarhammerBonus ?? 0;
						messages.Add(String.Format(InitiativeMessageWithBonuses, entity.Name, Math.Floor(entity.Initiative.Roll), agilityBonus, perceptionBonus));
					}
				}
				InGameLog.AddMessage(thread, String.Join(Environment.NewLine, messages), PrefixIcon.None, tooltip);
			}
		}


		[HarmonyPatch(typeof(TurnController), nameof(TurnController.TryRollInitiative)), HarmonyPostfix]
		private static void OnInitiativeRollingDone(ref List<MechanicEntity> ___m_JoinedThisTickEntities)
		{
			if (!Main.IsEnabled || !Main.Settings.ShowInitiativeRolls) return;
			if (___m_JoinedThisTickEntities.Count > 0)
			{
				var thread = InGameLog.GetThread<UnitInitiativeLogThread>(LogChannelType.AnyCombat);
				if (thread == null) return;
				var tooltip = new TooltipTemplateGlossary("Initiative");
				//InGameLog.AddMessage(thread, "Initiative Rolls:", PrefixIcon.RightArrow, tooltip);
				foreach (var entity in ___m_JoinedThisTickEntities.OrderByDescending(x => x.Initiative.Roll))
				{
					if (entity != null && !entity.IsDeadOrUnconscious && entity.IsInCombat)
					{
						var name = LogHelper.GetEntityName(entity);
						if (name == "<no name>") name = "Unknown";
						var str = String.Format(InitiativeMessage, name, Math.Floor(entity.Initiative.Roll));
						var message = new CombatLogMessage(str, Color.black, PrefixIcon.None, tooltip, true, unit: entity);
						thread.AddMessage(message);
					}
				}
			}
		}
	}
}
