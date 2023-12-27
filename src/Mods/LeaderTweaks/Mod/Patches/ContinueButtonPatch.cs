using HarmonyLib;

using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Localization;
using Kingmaker.UI.Sound;

using Leader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderTweaks.Mod.Patches
{
	//LocalizedString title, Action command, Func<bool> condition = null, UISounds.ButtonSoundsEnum clickSoundType = UISounds.ButtonSoundsEnum.NormalSound
	[HarmonyPatch(typeof(ContextMenuCollectionEntity), MethodType.Constructor, typeof(LocalizedString), typeof(Action), typeof(Func<bool>), typeof(UISounds.ButtonSoundsEnum))]
	public static class ContinueButtonPatch
	{
		private static LocalizedString _continueText => Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.MainMenu.Continue;

		private static void Prefix(LocalizedString title, Action command, ref Func<bool> condition, UISounds.ButtonSoundsEnum clickSoundType)
		{
			if (!Main.IsEnabled || !Main.Settings.FixContinueButton || title != _continueText) return;

			//condition = () => Game.Instance.SaveManager.GetLatestSave() != null;
			condition = () => Game.Instance.SaveManager.HasAnySaves();
		}
	}
}
