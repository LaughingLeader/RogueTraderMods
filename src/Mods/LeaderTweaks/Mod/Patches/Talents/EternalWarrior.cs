using Kingmaker;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Progression.Features;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderTweaks.Mod.Patches.Talents
{
	public static class EternalWarrior
	{
		private static readonly string UlfarGuid = "7ce1b02119cc52a1b2a6cd1c00900e37";
		private static readonly string EternalWarriorFeatureGuid = "cbccc999a2404988add7736c2e9c2ab7";
		private static readonly string EternalWarriorCombatBuffGuid = "ae2c9da842774dd58942a55e723e3aa7"; // UlfarWolfen_CombatBuff
		private static readonly string EternalWarriorWorkBuffGuid = "d8061098276c4896b81d16049933eb26"; // UlfarWolfen_WorkBuff

		//AddMechanicsFeature.ProvidesFullCover

		private static BlueprintFeature? _eternalWarriorFeature;
		private static BlueprintFeature EternalWarriorFeature
		{
			get
			{
				_eternalWarriorFeature ??= Utilities.GetBlueprintByGuid<BlueprintFeature>(EternalWarriorFeatureGuid);
				return _eternalWarriorFeature;
			}
		}

		private static BlueprintBuff? _eternalWarriorWorkBuff;
		private static BlueprintBuff EternalWarriorWorkBuff
		{
			get
			{
				_eternalWarriorWorkBuff ??= Utilities.GetBlueprintByGuid<BlueprintBuff>(EternalWarriorWorkBuffGuid);
				return _eternalWarriorWorkBuff;
			}
		}

		public static void OnTurnStarted(MechanicEntity unit)
		{
			if (unit.Facts.Contains(EternalWarriorFeature) && !unit.Buffs.Contains(EternalWarriorWorkBuff))
			{
				unit.GetMechanicFeature(MechanicsFeatureType.ProvidesFullCover).ReleaseAll();
			}
		}

		public static void OnTurnEnded(MechanicEntity unit)
		{
			if (unit.IsInCombat && !unit.IsDeadOrUnconscious && unit.Facts.Contains(EternalWarriorFeature) && !unit.Buffs.Contains(EternalWarriorWorkBuff))
			{
				unit.GetMechanicFeature(MechanicsFeatureType.ProvidesFullCover).Retain();
			}
		}

		public static void OnCombatEnded()
		{
			foreach (var unit in Game.Instance.Player.Party.Where(x => x.Facts.Contains(EternalWarriorFeature)))
			{
				unit.GetMechanicFeature(MechanicsFeatureType.ProvidesFullCover).ReleaseAll();
			}
		}
	}
}
