using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Leader
{
	public abstract class BasePlugin
	{
		private static readonly Type _conditionalPatchType = typeof(IConditionalPatch);
		private static readonly Type _patcherType = typeof(IPatch);
		private static readonly Type _harmonyPatchType = typeof(HarmonyPatch);

		private readonly string _patchesNamespace;
		private readonly List<PatchInstance> _patches = new();
		private readonly Harmony _harmony;

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
			var patchTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Namespace.StartsWith(_patchesNamespace));
			foreach (var patchType in patchTypes)
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
					}
				}
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
			return anyDisabled;
		}

		public BasePlugin(Harmony harmony, string patchesNamespace)
		{
			_harmony = harmony;
			_patchesNamespace = patchesNamespace;
		}
	}
}