using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityModManagerNet;

namespace MoreInfo.Mod
{
	public class PluginSettings : UnityModManager.ModSettings, IDrawable
	{
		[Draw("Show Initiative Rolls")] public bool ShowInitiativeRolls = true;
		[Draw("Show All Dialog Options")] public bool ShowAllDialogOptions = false;

		public override void Save(UnityModManager.ModEntry modEntry)
		{
			Save(this, modEntry);
		}

		public void OnChange()
		{

		}
	}
}
