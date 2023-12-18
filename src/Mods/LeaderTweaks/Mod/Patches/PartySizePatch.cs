using HarmonyLib;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace LeaderTweaks.Mod.Patches
{
	[HarmonyPatch]
	public static class PartySizePatch
	{
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
		private static bool FixMaxPartySizeLimit(GroupChangerCommonVM __instance, Action go, Action close, List<UnitReference> lastUnits, List<BlueprintUnit> requiredUnits, bool isCapital)
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

				/*IEnumerable <GroupChangerCharacterVM> enumerable = from v in __instance.m_LastUnits.Concat(Game.Instance.Player.PartyCharacters).Concat(from v in Game.Instance.Player.RemoteCompanions.Where(__instance.ShouldShowRemote)
				select UnitReference.FromIAbstractUnitEntity(v)).Distinct()
				orderby __instance.MustBeInParty((BaseUnitEntity)v.ToIBaseUnitEntity()) descending
				select v into u
				select new GroupChangerCharacterVM(u, __instance.MustBeInParty((BaseUnitEntity)u.ToIBaseUnitEntity()));*/

				int num = 0;
				foreach (var item in items)
				{
					Log.Info($"[num] Item: {item}");
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
	}
}
