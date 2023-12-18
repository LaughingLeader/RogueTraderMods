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
		[Draw("Max Party Size", Min = 1, Max = 24)]
		public int MaxPartySize = 12;

		[Header("Cheats")]
		[Draw("Always Succeed in Non-Combat Skill Checks", Tooltip = "Allows always succeeding in skill checks out of combat, where otherwise you might have a 0% chance")]
		public bool AlwaysSucceedNonCombatSkillChecks = false;

		[Draw("Fixes", Collapsible = true)]
		public FixSettings Fixes = new();

		public override void Save(UnityModManager.ModEntry modEntry)
		{
			Save(this, modEntry);
		}

		public void OnChange()
		{

		}
	}

	[DrawFields(DrawFieldMask.Public)]
	public class FixSettings
	{
		[Draw("Prevent Pain Channeling Self Damage", Tooltip = "Fix damaging yourself with Pain Channeling when killing the last nearby enemy")]
		public bool DisablePainChannelingSelfDamage = true;
	}
}
