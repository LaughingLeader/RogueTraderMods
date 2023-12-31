﻿using HarmonyLib;

using Kingmaker;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Progression.Features;

using Leader;

namespace LeaderTweaks.Mod.Patches.QOL
{
    public static class RelentlessBlaze
    {
        private static readonly string RelentlessBlazeFeature = "89723dbb70634201a1826c49482c152a";
        private static readonly string IgniteAbility = "a2cca43669184eaa9f0da981f204e1c9";
        private static readonly string OrchestrateFlamesFeature = "b46cc3b4b29a4dcf9c62b14e7a38796d";
        private static readonly string OrchestrateFlamesAbility = "7720d74e51f94184bb43b97ce9c9e53f";
        private static readonly string OrchestrateFlamesBuff = "123d1d6d75394528b1955bb4c3b88103";
        private static readonly string BurningBuff = "1393efb71f5844efa894a2d2682b3d59"; // Or 8da2a947a1a8bc543b9e7a9cad054414
        private static readonly string WarpBurnBuff = "3ddbf131cbd54087a665d4b2e026b0f2";

        private static readonly BuffDuration CombatDuration = new(null, BuffEndCondition.CombatEnd);
        private static readonly BuffDuration OutOfCombatDuration = new(null, BuffEndCondition.RemainAfterCombat);

        private static bool _initialized = false;
        private static bool _isValid = false;

        private static BlueprintFeature? blazeFeature;
        private static BlueprintAbility? igniteAbility;
        private static BlueprintAbility? orchestrateAbility;
        private static BlueprintBuff? orchestrateBuff;
        private static BlueprintBuff? warpBurnBuff;

        private static void Init()
        {
            blazeFeature = Utilities.GetBlueprintByGuid<BlueprintFeature>(RelentlessBlazeFeature);
            igniteAbility = Utilities.GetBlueprintByGuid<BlueprintAbility>(IgniteAbility);
            orchestrateAbility = Utilities.GetBlueprintByGuid<BlueprintAbility>(OrchestrateFlamesAbility);
            orchestrateBuff = Utilities.GetBlueprintByGuid<BlueprintBuff>(OrchestrateFlamesBuff);
            warpBurnBuff = Utilities.GetBlueprintByGuid<BlueprintBuff>(WarpBurnBuff);
            _initialized = true;
            _isValid = blazeFeature != null && igniteAbility != null && orchestrateAbility != null && orchestrateBuff != null && warpBurnBuff != null;
        }

        public static void RemoveCombatStartBuffs(MechanicEntity entity)
        {
            if (!_initialized) Init();
            if (!_isValid) return;

            if (entity?.IsInPlayerParty == true && entity is BaseUnitEntity unit && unit.Facts.Contains(blazeFeature))
            {
                unit.Buffs.Remove(warpBurnBuff);
                unit.Buffs.Remove(orchestrateBuff);
            }
        }

        public static void ApplyCombatStartBuffs(MechanicEntity? entity)
        {
            if (!_initialized) Init();
            if (!_isValid || entity == null) return;

            //Log.Info($"[ApplyCombatStartBuffs] unit({entity.Name}) IsInPlayerParty({entity.IsInPlayerParty}) IsInCombat({entity.IsInCombat})");

            if (!Game.Instance.IsSpaceCombat && entity.IsInPlayerParty == true && entity is BaseUnitEntity unit)
            {
                if (unit.Facts.Contains(blazeFeature))
                {
                    var duration = entity.IsInCombat ? CombatDuration : OutOfCombatDuration;

                    if (unit.Abilities.Contains(igniteAbility) && !unit.Buffs.Contains(warpBurnBuff))
                    {
                        unit.Buffs.Add(warpBurnBuff, entity, duration);
                    }
                    if (unit.Buffs.Contains(warpBurnBuff) && unit.Abilities.Contains(orchestrateAbility) && !unit.Buffs.Contains(orchestrateBuff))
                    {
                        unit.Buffs.Add(orchestrateBuff, entity, duration);
                    }
                }
            }
        }

        public static void OnCombatStarted()
        {
            foreach (var unit in Game.Instance.Player.Party)
            {
                if (unit.IsInCombat)
                {
                    ApplyCombatStartBuffs(unit);
                }
            }
        }

        public static bool OnDamage(ContextActionDealDamage __instance, ref int __result)
        {
            var guid = __instance.Owner?.AssetGuid;
            if ((guid == WarpBurnBuff || guid == BurningBuff) && __instance.TargetEntity is BaseUnitEntity unit
                && unit.Facts.Contains(blazeFeature) && unit.Buffs.Contains(orchestrateBuff))
            {
                __result = 0;
                return false;
            }
            return true;
        }

        /*[HarmonyPatch(typeof(ContextActionDealDamage), nameof(ContextActionDealDamage.GetDamagePrediction)), HarmonyPostfix]		
		private static void OnBurningDamagePrediction(AbilityExecutionContext context, MechanicEntity? target, ContextActionDealDamage __instance, ref DamagePredictionData __result)
		{
			if (!Main.IsEnabled || !Main.Settings.RelentlessBlazeBurningDealsZeroDamage) return;
			var guid = __instance.Owner?.AssetGuid;
			if ((guid == WarpBurnBuff || guid == BurningBuff) && target is BaseUnitEntity unit && unit.Facts.Contains(blazeFeature))
			{
				__result.MinDamage = 0;
				__result.MaxDamage = 0;
				__result.Penetration = 0;
			}
		}*/

        /*
		[HarmonyPatch(typeof(HealthController), nameof(HealthController.HandleUnitStartTurn))]
		private static void OnTurnStarted()
		{
			if (!Main.IsEnabled) return;
			var entity = EventInvokerExtensions.MechanicEntity;
			if (entity?.IsInCombat == true && Game.Instance.TurnController.CombatRound == 1)
			{
				ApplyCombatStartBuffs(entity);
			}
		}*/

        /*[HarmonyPatch(typeof(TurnController), nameof(TurnController.NextRound))]
		[HarmonyPostfix]
		private static void ApplyCombatStartBuffs(bool isFirstRound)
		{
			if(isFirstRound)
			{
				foreach(var unit in Game.Instance.State.AllUnits)
				{
					if(unit is UnitEntityData unitData)
					{
						unitData.HasFeatureSelection();
					}
				}
			}
			//var entity = EventInvokerExtensions.MechanicEntity;
			//if (!Game.Instance.IsSpaceCombat && entity?.IsInPlayerParty == true && entity is BaseUnitEntity unit)
			//{
			//	//Game.Instance.State.AllUnits
				
			//}
		}*/
    }
}
