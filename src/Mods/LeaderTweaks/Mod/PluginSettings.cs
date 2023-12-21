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
		[Header("Cheats")]
		[Draw("Non-Combat Roll Checks", Collapsible = true)]
		public RollCheckSettings RollChecks = new();

		[Header("Tweaks")]
		[Draw("Max Party Size", Tooltip="This will expand the max party size in the recruitment screen as well", Min = 1, Max = 24)]
		public int MaxPartySize = 10;

		[Draw("Talents", Collapsible = true)]
		public TalentSettings Talents = new();

		public override void Save(UnityModManager.ModEntry modEntry)
		{
			Save(this, modEntry);
		}

		public void OnChange()
		{
			
		}

		public PluginSettings() : base()
		{
			var t = GetType();
			//Expand by default
			UnityModManager.UI.collapsibleStates.Add(t.GetField(nameof(Talents)).MetadataToken);
			UnityModManager.UI.collapsibleStates.Add(t.GetField(nameof(RollChecks)).MetadataToken);
		}
	}

	public class TalentSettings
	{
		[Draw("Relentless Blaze: Automatically Apply Warp Burn/Orchestrate Flames", Tooltip = "When combat starts, automatically apply the following buffs if the related abilities are available:\nIgnite: Warp Burn\nOrchestrate Flames: Orchestrate Flames buff\nThis will trigger the +1 Psy Rating bonus")]
		public bool AutoRelentlessBlaze = false;

		[Draw("Relentless Blaze: No Burning Damage", Tooltip = "Disable damage from Burning/Warp Burn if the character has the Relentless Blaze talent and the Orchestrate Flames buff")]
		public bool RelentlessBlazeBurningDealsZeroDamage = false;

		[Draw("Pain Channeling: Prevent Self Damage", Tooltip = "Fix damaging yourself with Pain Channeling when killing the last nearby enemy")]
		public bool DisablePainChannelingSelfDamage = true;
	}

	public class RollCheckSettings
	{
		[Draw("Always Pass Skill Checks", Tooltip = "Always succeed in skill checks out of combat, even if you have a 0% chance")]
		public bool NonCombatSkillChecks = false;
		[Draw("Always Pass Attribute Checks", Tooltip = "Always succeed in attribute checks out of combat, even if you have a 0% chance")]
		public bool NonCombatAttributeChecks = false;

		public bool IsEnabled => NonCombatAttributeChecks || NonCombatSkillChecks;
	}
}
