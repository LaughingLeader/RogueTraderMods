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
	public class AlwaysShowDialogAnswersPatch : IPatch
	{
		public bool IsEnabled { get; set; }

		private static readonly MethodInfo m_CanSelect = AccessTools.Method(typeof(BlueprintAnswersList), nameof(BlueprintAnswersList.CanSelect));
		private static readonly MethodInfo m_AnswerCanShow = AccessTools.Method(typeof(BlueprintAnswer), nameof(BlueprintAnswer.CanShow));
		private static readonly MethodInfo m_CueCanShow = AccessTools.Method(typeof(BlueprintCue), nameof(BlueprintCue.CanShow));
		private static readonly MethodInfo m_ConditionsCheckerCheck = AccessTools.Method(typeof(ConditionsChecker), nameof(ConditionsChecker.Check), new Type[1] {typeof(IConditionDebugContext) });
		private static readonly MethodInfo m_AddAnswers = AccessTools.Method(typeof(DialogController), nameof(DialogController.AddAnswers));

		private static readonly MethodInfo m_DialogDebugAdd = AccessTools.Method(typeof(DialogDebug), nameof(DialogDebug.Add), new Type[3] {typeof(BlueprintScriptableObject), typeof(string), typeof(Color)});

		//private static readonly MethodInfo g_IsEditor = AccessTools.PropertyGetter(typeof(UnityEngine.Application), nameof(UnityEngine.Application.isEditor));

		public void Enable(Type thisType, Harmony harmony)
		{
			//harmony.Patch(m_CanSelect, postfix: new HarmonyMethod(thisType, nameof(IgnoreSelectConditions)));
			harmony.Patch(m_AnswerCanShow, postfix: new HarmonyMethod(thisType, nameof(IgnoreAnswerShowConditions)));
			//harmony.Patch(m_CueCanShow, postfix: new HarmonyMethod(thisType, nameof(IgnoreCueShowConditions)));
			//harmony.Patch(m_ConditionsCheckerCheck, postfix: new HarmonyMethod(thisType, nameof(DebugCondition)));
			harmony.Patch(m_DialogDebugAdd, postfix: new HarmonyMethod(thisType, nameof(OnDialogDebugAdd)));
			harmony.Patch(m_AddAnswers, postfix: new HarmonyMethod(thisType, nameof(OnAddAnswers)));
		}

		public void Disable(Type thisType, Harmony harmony)
		{
			//harmony.Unpatch(m_CanSelect, thisType.GetMethod(nameof(IgnoreSelectConditions)));
			harmony.Unpatch(m_AnswerCanShow, thisType.GetMethod(nameof(IgnoreAnswerShowConditions)));
			//harmony.Unpatch(m_CueCanShow, thisType.GetMethod(nameof(IgnoreCueShowConditions)));
			//harmony.Unpatch(m_ConditionsCheckerCheck, thisType.GetMethod(nameof(DebugCondition)));
			harmony.Unpatch(m_DialogDebugAdd, thisType.GetMethod(nameof(OnDialogDebugAdd)));
			harmony.Unpatch(m_AddAnswers, thisType.GetMethod(nameof(OnAddAnswers)));
		}

		private static List<BlueprintScriptableObject> _shownOnceAnswers = new();

		private static void OnDialogDebugAdd(BlueprintScriptableObject blueprint, string message, Color color)
		{
			if(blueprint != null && blueprint is BlueprintAnswer && message.Contains("show once"))
			{
				_shownOnceAnswers.Add(blueprint);
			}
		}

		private static void IgnoreSelectConditions(ref bool __result, ref BlueprintAnswersList __instance)
		{
			if (!__result && __instance?.ShowOnce == false)
			{
				__result = true;
			}
		}

		private static void IgnoreAnswerShowConditions(ref bool __result, ref BlueprintAnswer __instance)
		{
			if(!__result && __instance != null)
			{
				if (!_shownOnceAnswers.Contains(__instance))
				{
					__result = true;
				}
			}
		}

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

		private static void OnAddAnswers(IEnumerable<BlueprintAnswerBase> answers, BlueprintCueBase continueCue)
		{
			_shownOnceAnswers.Clear();
			/*if (answers != null)
			{
				Log.Info($"[OnAddAnswers] Answers:\n{String.Join(Environment.NewLine, answers.Select(x => x.name))}");
				foreach(var answer in answers)
				{
					if(answer.name == "Answer_0003" && answer is BlueprintAnswer banswer)
					{
						var sep = ";";
						Log.Info($"[Answer_0003] ShowOnce({banswer.ShowOnce}) CanShow({banswer.CanShow()}) ShowConditions({String.Join(sep, banswer.ShowConditions.Conditions.Select(DumpCondition))})");
					}
				}
			}*/
		}

		private static void DebugCondition(ref bool __result, ref Condition __instance)
		{
			try
			{
				if (__instance != null && Game.Instance?.DialogController?.DialogData != null) Log.Info($"[Condition({__instance})] = {__result}");
			}
			catch (Exception) { }
		}
	}
}
