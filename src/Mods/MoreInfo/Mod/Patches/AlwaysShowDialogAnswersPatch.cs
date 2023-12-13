using HarmonyLib;

using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.Interfaces;

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
	public static class AlwaysShowDialogAnswersPatch
	{
		private static readonly List<BlueprintScriptableObject> _shownOnceAnswers = new();

		[HarmonyPatch(typeof(DialogDebug), nameof(DialogDebug.Add), new Type[3] { typeof(BlueprintScriptableObject), typeof(string), typeof(Color) })]
		[HarmonyPostfix]
		private static void OnDialogDebugAdd(BlueprintScriptableObject blueprint, string message, Color color)
		{
			if (!Main.IsEnabled || !Main.Settings.ShowAllDialogOptions) return;
			if (blueprint != null && blueprint is BlueprintAnswer && message.Contains("show once"))
			{
				_shownOnceAnswers.Add(blueprint);
			}
		}

		[HarmonyPatch(typeof(DialogController), nameof(DialogController.AddAnswers)), HarmonyPostfix]
		private static void OnAddAnswers(IEnumerable<BlueprintAnswerBase> answers, BlueprintCueBase continueCue)
		{
			_shownOnceAnswers.Clear();
		}

		[HarmonyPatch(typeof(BlueprintAnswer), nameof(BlueprintAnswer.CanShow)), HarmonyPostfix]
		private static void IgnoreAnswerShowConditions(ref bool __result, ref BlueprintAnswer __instance)
		{
			if (!Main.IsEnabled || !Main.Settings.ShowAllDialogOptions) return;
			if (!__result && __instance != null)
			{
				if (!_shownOnceAnswers.Contains(__instance))
				{
					__result = true;
				}
			}
		}

		/*
		[HarmonyPatch(typeof(BlueprintAnswersList), nameof(BlueprintAnswersList.CanSelect)), HarmonyPostfix]
		private static void IgnoreSelectConditions(ref bool __result, ref BlueprintAnswersList __instance)
		{
			if (!__result && __instance?.ShowOnce == false)
			{
				__result = true;
			}
		}

		[HarmonyPatch(typeof(BlueprintCue), nameof(BlueprintCue.CanShow)), HarmonyPostfix]
		private static void IgnoreCueShowConditions(ref bool __result, ref BlueprintCueBase __instance)
		{
			if (!__result && __instance?.ShowOnce == false)
			{
				__result = true;
			}
		}

		private static void AlwaysTrue(ref bool __result) => __result = true;

		private static string DumpCondition(Condition condition)
		{
			return $"{condition.GetType()}";
		}

		private static void DebugCondition(ref bool __result, ref Condition __instance)
		{
			try
			{
				if (__instance != null && Game.Instance?.DialogController?.DialogData != null) Log.Info($"[Condition({__instance})] = {__result}");
			}
			catch (Exception) { }
		}
*/
	}
}
