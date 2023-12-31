﻿using HarmonyLib;

using Kingmaker;

using Leader;
using LeaderTweaks.Mod.Patches.QOL;
using System.Linq;

using UnityEngine;

using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace LeaderTweaks.Mod
{

#if DEBUG
    [EnableReloading]
#endif
    public class Plugin : BasePlugin
    {
		private readonly PluginSettings _settings;
		public PluginSettings Settings => _settings;

		private static void FixInitiative()
        {
			foreach(var unit in Game.Instance.Player.Party)
			{
				if(unit.Features.IsFirstInFight.Value == true)
				{
					Log.Info($"[{unit.Name}] IsFirstInFight Buffs({unit.Features.IsFirstInFight.AssociatedBuffs.Buffs.Count()})");
					if (unit.Features.IsFirstInFight.AssociatedBuffs.Buffs.Count() == 0)
					{
						unit.Features.IsFirstInFight.ReleaseAll();
						Log.Info($"Fixed IsFirstInFight being set for [{unit.Name}]");
					}
				}
			}
		}

		private void OnGUI(UnityModManager.ModEntry modEntry)
		{
			Settings.Draw(modEntry);
			/*if(GUILayout.Button("Apply Buffs", GUILayout.ExpandWidth(false)))
			{
				foreach (var unit in Game.Instance.Player.Party)
				{
					RelentlessBlaze.ApplyCombatStartBuffs(unit);
				}
			}
			if(GUILayout.Button("Remove Buffs", GUILayout.ExpandWidth(false)))
			{
				foreach(var unit in Game.Instance.Player.Party)
				{
					RelentlessBlaze.RemoveCombatStartBuffs(unit);
				}
			}*/
		}

		private void OnSaveGUI(UnityModManager.ModEntry modEntry)
		{
			Settings.Save(modEntry);
		}

		public Plugin(ModEntry modEntry, Harmony harmony) : base(modEntry, harmony, "LeaderTweaks.Mod.Patches")
        {
			_settings = PluginSettings.Load<PluginSettings>(modEntry);
			modEntry.OnGUI = OnGUI;
			modEntry.OnSaveGUI = OnSaveGUI;
        }
    }
}
