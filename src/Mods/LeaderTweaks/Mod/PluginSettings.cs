using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using UnityModManagerNet;

namespace LeaderTweaks.Mod
{
	public class PluginSettings : UnityModManager.ModSettings, IDrawable
	{
		[Header("Combat")]
		[Draw("Relentless Blaze: Automatically Apply Burning")] public bool AutoRelentlessBlaze = false;
		[Draw("Relentless Blaze: No Burning Damage")] public bool RelentlessBlazeBurningDealsZeroDamage = false;

		[Header("Party")]
		[Draw("Max Party Size", Min = 1, Max = 24)] public int MaxPartySize = 12;

		public override void Save(UnityModManager.ModEntry modEntry)
		{
			Save(this, modEntry);
		}

		public void OnChange()
		{

		}
	}
}
