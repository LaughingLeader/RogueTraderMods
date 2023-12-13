using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using UnityModManagerNet;

namespace MoreInfo.Mod
{
	public class PluginSettings : UnityModManager.ModSettings, IDrawable
	{
		[Header("Combat Log")]
		[Draw("Show Initiative Rolls")] public bool ShowInitiativeRolls = true;

		[Header("Dialog")]
		[Draw("Show All Answers")] public bool ShowAllDialogOptions = false;
		[Draw("Show Previously Selected Answers")] public bool IgnoreShowOnce = false;

		public override void Save(UnityModManager.ModEntry modEntry)
		{
			Save(this, modEntry);
		}

		public void OnChange()
		{

		}
	}
}
