using HarmonyLib;

using Leader;
using MoreInfo.Utils;

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
    public class Plugin
    {
        private readonly Harmony _harmony;
        private readonly string _modId;
        private readonly ModLogger _logger;

        private const string PatchesNamespace = "MoreInfo.Mod.Patches";

		public ModLogger Logger => _logger;
		public static Plugin? Instance { get; private set; }

		public static void Log(string message) => Instance?.Logger.Log(message);
		public static void LogException(string message, Exception ex) => Instance?.Logger.LogException(message, ex);
		public static void LogException(Exception ex) => Instance?.Logger.LogException(ex);

		private static readonly Type _conditionalPatchType = typeof(IConditionalPatch);
        private static readonly Type _patcherType = typeof(IPatch);
        private static readonly Type _harmonyPatchType = typeof(HarmonyPatch);

        private readonly List<PatchInstance> _patches = [];

        private static bool _isEnabled = false;
        public static bool IsEnabled => _isEnabled;

        private static bool CanEnablePatch(IPatch instance, Type patchType)
        {
            if (patchType.IsAssignableFrom(_conditionalPatchType) && instance is IConditionalPatch conditionalPatch)
            {
                return conditionalPatch.CanEnablePatch;
            }
            return true;
        }

        private bool ApplyPatches()
        {
            var anyEnabled = false;
            var patchTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Namespace.StartsWith(PatchesNamespace));
            foreach (var patchType in patchTypes)
            {
                if (_patcherType.IsAssignableFrom(patchType))
                {
                    var instance = (IPatch)Activator.CreateInstance(patchType);
                    if (instance != null && CanEnablePatch(instance, patchType))
                    {
                        Log($"Activating IPatcher patch {patchType.Name}");
                        instance.Enable(patchType, _harmony);
                        _patches.Add(new PatchInstance { PatchType = patchType, Instance = instance });
                        anyEnabled = true;
                    }
                }
                else if (patchType.GetCustomAttributes(true).Any(x => x.GetType() == _harmonyPatchType))
                {
                    var processor = _harmony.CreateClassProcessor(patchType);
                    if (processor != null)
                    {
                        Log($"Activating harmony patch {patchType.Name}");
                        processor.Patch();
                        anyEnabled = true;
                    }
                }
            }
            return anyEnabled;
        }

        private bool DisablePatches()
        {
            var anyDisabled = false;
            foreach (var patch in _patches)
            {
                Log($"[MoreInfo] Deactivating patch {patch.PatchType.Name}");
                patch.Instance.Disable(patch.PatchType, _harmony);
                anyDisabled = true;
            }
            _patches.Clear();
            return anyDisabled;
        }

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
                LogException(ex);
            }
            return false;
        }

        public bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            _harmony.UnpatchAll(modEntry.Info.Id);
            _patches.Clear();
            return true;
        }

        public Plugin(UnityModManager.ModEntry modEntry, Harmony harmony)
        {
            Instance = this;
            _modId = modEntry.Info.Id;
            _logger = modEntry.Logger;

            _harmony = harmony;

            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate;
        }
    }
}
