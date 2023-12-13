using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Leader
{
	public abstract class BasePlugin
	{
		private static readonly Type _conditionalPatchType = typeof(IConditionalPatch);
		private static readonly Type _patcherType = typeof(IPatch);
		private static readonly Type _harmonyPatchType = typeof(HarmonyPatch);

		private readonly string _patchesNamespace;
		private readonly List<Type> _matchedPatchTypes = new();
		private readonly List<PatchInstance> _patches = new();
		private readonly Harmony _harmony;
		private readonly string _modId;

		/// <summary>
		/// Types that use the regular harmony attributes.
		/// </summary>
		private int _harmonyPatches = 0;

		public bool IsEnabled { get; private set; }

		internal List<PatchInstance> Patches => _patches;
		internal Harmony HarmonyInstance => _harmony;

		private static bool CanEnablePatch(IPatch instance, Type patchType)
		{
			if (patchType.IsAssignableFrom(_conditionalPatchType) && instance is IConditionalPatch conditionalPatch)
			{
				return conditionalPatch.CanEnablePatch;
			}
			return true;
		}

		protected bool ApplyPatches()
		{
			var anyEnabled = false;
			if(_matchedPatchTypes.Count > 0)
			{
				foreach (var patchType in _matchedPatchTypes)
				{
					if (_patcherType.IsAssignableFrom(patchType))
					{
						var instance = (IPatch)Activator.CreateInstance(patchType);
						if (instance != null && CanEnablePatch(instance, patchType))
						{
							Log.Info($"Activating IPatcher patch {patchType.Name}");
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
							Log.Info($"Activating harmony patch {patchType.Name}");
							processor.Patch();
							anyEnabled = true;
							_harmonyPatches++;
						}
					}
				}
			}
			if(!anyEnabled)
			{
				Log.Error($"No patches found in namespace '{_patchesNamespace}'");
			}
			return anyEnabled;
		}

		protected bool DisablePatches()
		{
			var anyDisabled = false;
			foreach (var patch in _patches)
			{
				Log.Info($"[MoreInfo] Deactivating patch {patch.PatchType.Name}");
				patch.Instance.Disable(patch.PatchType, _harmony);
				anyDisabled = true;
			}
			_patches.Clear();
			if(_harmonyPatches > 0)
			{
				_harmony.UnpatchAll(_modId);
				_harmonyPatches = 0;
			}
			return anyDisabled;
		}

		protected virtual bool OnToggle(ModEntry modEntry, bool value)
		{
			IsEnabled = value;
			try
			{
				if (IsEnabled)
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

		/// <summary>
		/// Called from Main, in case an error occurs.
		/// </summary>
		/// <param name="modEntry"></param>
		/// <returns></returns>
		internal virtual bool OnUnload(ModEntry modEntry)
		{
			HarmonyInstance.UnpatchAll(modEntry.Info.Id);
			Patches.Clear();
			return true;
		}

		protected virtual void OnUpdate(ModEntry modEntry, float dt)
		{

		}

		protected void ConfigureLog(ModEntry modEntry)
		{
			var log = modEntry.Logger;
			Log.Configure(new LogCallbacks
			{
				Info = log.Log,
				Warning = log.Warning,
				Error = log.Error,
				Exception = log.LogException,
				Exception2 = log.LogException,
				Critical = log.Critical,
				NativeLog = log.NativeLog
			});
		}

		public BasePlugin(ModEntry modEntry, Harmony harmony, string patchesNamespace)
		{
			_harmony = harmony;
			_patchesNamespace = patchesNamespace;
			_modId = modEntry.Info.Id;

			ConfigureLog(modEntry);

			modEntry.OnToggle = OnToggle;
			modEntry.OnUpdate = OnUpdate;

			_matchedPatchTypes.AddRange(Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Namespace.StartsWith(_patchesNamespace)));
		}
	}
}