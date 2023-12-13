using HarmonyLib;

using Leader;

using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace MoreInfo.Mod
{

#if DEBUG
	[EnableReloading]
#endif
    public class Plugin : BasePlugin
    {
        public Plugin(ModEntry modEntry, Harmony harmony) : base(modEntry, harmony, "MoreInfo.Mod.Patches")
        {
           
        }
    }
}
