using HarmonyLib;
using LeaderTweaks.Mod;

using System;

using UnityModManagerNet;

namespace Leader
{
#if DEBUG
    [EnableReloading]
#endif
	public static class Main
	{
		private static Plugin? _mod;
		private static Harmony? _harmony;

		public static PluginSettings Settings => _mod!.Settings;
		public static bool IsEnabled => _mod?.IsEnabled == true;

		private static bool Unload(UnityModManager.ModEntry modEntry)
		{
			if(_mod != null)
			{
				_mod.OnUnload(modEntry);
			}
			else
			{
				_harmony?.UnpatchAll(modEntry.Info.Id);
			}
			return true;
		}

		private static bool Load(UnityModManager.ModEntry modEntry)
		{
			modEntry.OnUnload = Unload;

			try
			{
				_harmony = new Harmony(modEntry.Info.Id);
				_mod = new Plugin(modEntry, _harmony);
			}
			catch(Exception ex)
			{
				modEntry.Logger.LogException(ex);
				_harmony?.UnpatchAll(modEntry.Info.Id);
			}
			return true;
		}
	}
}
