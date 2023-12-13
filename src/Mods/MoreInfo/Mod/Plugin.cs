using HarmonyLib;

using Kingmaker;

using Leader;

using System.Linq;

using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace MoreInfo.Mod
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
		}

		private void OnSaveGUI(UnityModManager.ModEntry modEntry)
		{
			Settings.Save(modEntry);
		}

		public Plugin(ModEntry modEntry, Harmony harmony) : base(modEntry, harmony, "MoreInfo.Mod.Patches")
        {
			_settings = PluginSettings.Load<PluginSettings>(modEntry);
			modEntry.OnGUI = OnGUI;
			modEntry.OnSaveGUI = OnSaveGUI;
        }
    }
}
