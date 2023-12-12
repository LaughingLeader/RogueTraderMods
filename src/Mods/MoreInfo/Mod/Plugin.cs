using HarmonyLib;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager.ModEntry;

namespace MoreInfo.Mod
{

#if DEBUG
    [EnableReloading]
#endif
    public class Plugin : BasePlugin
    {
        private readonly ModLogger _logger;

		public ModLogger Logger => _logger;
		public static Plugin? Instance { get; private set; }

        private static bool _isEnabled = false;
        public static bool IsEnabled => _isEnabled;

        private bool triedToPrint = false;

        private void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (!triedToPrint)
            {
                InGameLog.AddDialogMessage("[MoreInfo] Test");
                triedToPrint = true;
            }
        }

        private bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            _isEnabled = value;
            try
            {
                if (_isEnabled)
                {
                    return ApplyPatches();
                }
                else
                {
                    return DisablePatches();
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            return false;
        }

        public bool OnUnload(UnityModManager.ModEntry modEntry)
        {
			HarmonyInstance.UnpatchAll(modEntry.Info.Id);
            Patches.Clear();
            return true;
        }

        public Plugin(UnityModManager.ModEntry modEntry, Harmony harmony) : base(harmony, "MoreInfo.Mod.Patches")
        {
            Instance = this;
            _logger = modEntry.Logger;
            Log.Configure(new LogCallbacks
            {
                Info = _logger.Log,
                Warning = _logger.Warning,
                Error = _logger.Error,
                Exception = _logger.LogException,
                Exception2 = _logger.LogException,
                Critical = _logger.Critical,
                NativeLog = _logger.NativeLog
            });

            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate;
        }
    }
}
