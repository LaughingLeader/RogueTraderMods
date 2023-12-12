using HarmonyLib;

using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;

using Owlcat.Runtime.UI.Tooltips;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Leader
{
    public static class InGameLog
	{
		public static LogThreadBase? GetThread<T>(LogChannelType logChannelType = LogChannelType.Dialog) where T : LogThreadBase
		{
			return LogThreadService.Instance.m_Logs[logChannelType].FirstOrDefault(x => x is T);
		}

		public static void AddMessage(LogThreadBase? thread, string msg, PrefixIcon icon = PrefixIcon.None, TooltipBaseTemplate? tooltip = null)
		{
			if (thread != null)
			{
				var message = new CombatLogMessage(msg, Color.black, icon, tooltip, tooltip != null);
				thread.AddMessage(message);
			}
		}

		public static void AddMessage<T>(string msg, LogChannelType logChannelType = LogChannelType.Dialog, PrefixIcon icon = PrefixIcon.None, TooltipBaseTemplate? tooltip = null) where T : LogThreadBase
		{
			AddMessage(GetThread<T>(logChannelType), msg, icon, tooltip);
		}

		public static void AddDialogMessage(string msg, PrefixIcon icon = PrefixIcon.None, TooltipBaseTemplate? tooltip = null) => AddMessage(GetThread<DialogLogThread>(LogChannelType.Dialog), msg, icon, tooltip);

		public static void DebugDumpThreads(LogChannelType logChannelType)
		{
			var threads = LogThreadService.Instance.m_Logs[logChannelType];
			if(threads != null)
			{
				foreach(var thread in threads)
				{
					Log.Info($"[DebugDumpThreads:{logChannelType}] Thread({thread.GetType()})");
				}
			}
		}
	}
}
