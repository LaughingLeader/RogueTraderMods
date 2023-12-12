using HarmonyLib;
using MoreInfo.Mod;
using UnityModManagerNet;

namespace Leader
{
#if DEBUG
    [EnableReloading]
#endif
	public static class Main
	{
		private static Plugin? _mod;

		private static bool Unload(UnityModManager.ModEntry modEntry)
		{
			_mod?.OnUnload(modEntry);
			return true;
		}

		private static bool Load(UnityModManager.ModEntry modEntry)
		{
			modEntry.OnUnload = Unload;

			var harmony = new Harmony(modEntry.Info.Id);

			_mod = new Plugin(modEntry, harmony);

			return true;
		}
	}
}
