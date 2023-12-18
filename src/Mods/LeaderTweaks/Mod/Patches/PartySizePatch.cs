using HarmonyLib;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UniRx;

namespace LeaderTweaks.Mod.Patches
{
	[HarmonyPatch]
	public static class PartySizePatch
	{
		//Doesn't seem to be used, since they hardcode 6 everywhere
		//public override int Kingmaker.Designers.EventConditionActionSystem.Evaluators.MaxPartySize.GetValueInternal()
		[HarmonyPatch(typeof(MaxPartySize), nameof(MaxPartySize.GetValueInternal)), HarmonyPrefix]
		private static bool OverrideMaxPartySize(ref int __result)
		{
			if(!Main.IsEnabled || Main.Settings.MaxPartySize == 6) return true;
			__result = Main.Settings.MaxPartySize;
			return false;
		}

		private static readonly ConstructorInfo GroupChangerVMCtor = AccessTools.Constructor(typeof(GroupChangerVM), new Type[] { typeof(Action), typeof(Action), typeof(List<UnitReference>), typeof(List<BlueprintUnit>), typeof(bool) });

		//Action go, Action close, List<UnitReference> lastUnits, List<BlueprintUnit> requiredUnits, bool isCapital = false
		[HarmonyPatch(typeof(GroupChangerCommonVM), MethodType.Constructor, typeof(Action), typeof(Action), typeof(List<UnitReference>), typeof(List<BlueprintUnit>), typeof(bool))]
		[HarmonyPrefix]
		private static bool GroupChangerCommonVMMaxPartySizeLimit(GroupChangerCommonVM __instance, Action go, Action close, List<UnitReference> lastUnits, List<BlueprintUnit> requiredUnits, bool isCapital)
		{
			if (!Main.IsEnabled || Main.Settings.MaxPartySize == 6) return true;

			try
			{
				//Should be equivalent to calling base()
				GroupChangerVMCtor.Invoke(__instance, new object[] { go, close, lastUnits, requiredUnits, isCapital });

				var maxSize = Main.Settings.MaxPartySize;

				var newUnits = Game.Instance.Player.RemoteCompanions.Where(x => x != null && __instance.ShouldShowRemote(x)).OrderByDescending(__instance.MustBeInParty)
					.Select(x => UnitReference.FromIAbstractUnitEntity(x));
				var items = __instance.m_LastUnits.Concat(Game.Instance.Player.PartyCharacters).Concat(newUnits)
					.Select(x => new GroupChangerCharacterVM(x, __instance.MustBeInParty((BaseUnitEntity)x.ToIBaseUnitEntity())));

				int num = 0;
				foreach (var item in items)
				{
					item.SetIsInParty(num < maxSize);
					if (num < maxSize)
					{
						__instance.m_PartyCharacter.Add(item);
					}
					else
					{
						__instance.m_RemoteCharacter.Add(item);
					}
					__instance.AddDisposable(item.Click.Subscribe(delegate (GroupChangerCharacterVM value)
					{
						__instance.MoveCharacter(value.UnitRef);
					}));
					num++;
				}
				return false;
			}
			catch (Exception ex)
			{
				//Log.Error($"Error line: {ex.StackTrace}");
				Log.Exception(ex);
			}
			return true;
		}

		//Fix visible party portraits being limited to 6
		[HarmonyPatch(typeof(PartyVM), MethodType.Constructor)]
		[HarmonyPostfix]
		private static void PartyVMMaxPartySizeLimit(PartyVM __instance)
		{
			if (!Main.IsEnabled || Main.Settings.MaxPartySize == 6) return;

			var remainingPortraits = Game.Instance.Player.Party.Count - 6;
			if(remainingPortraits > 0)
			{
				for(int i = 0;i < remainingPortraits; i++)
				{
					__instance.CharactersVM.Add(new PartyCharacterVM(__instance.NextPrev, i));
				}
			}
		}

		////UIStrings.Instance.GroupChangerTexts.MaxGroupCountWarning
		//77487c6f-b606-4bcd-bc6a-018339c06220
		//Party limit reached!

		//Fix being unable to add more than 6 characters when selecting a party
		[HarmonyPatch(typeof(GroupChangerVM), nameof(GroupChangerVM.CanMoveCharacterFromRemoteToParty))]
		[HarmonyPostfix]
		private static void GroupChangerVMMaxPartySizeLimit(ref string? __result, GroupChangerCommonVM __instance)
		{
			if (!Main.IsEnabled || Main.Settings.MaxPartySize == 6) return;

			//Hardcoded to == 6 normally

			if(__result != UIStrings.Instance.GroupChangerTexts.MaxNavigatorsCountWarning && __instance.m_PartyCharacter.Count < Main.Settings.MaxPartySize)
			{
				__result = null;
			}
		}
	}
}
