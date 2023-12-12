using HarmonyLib;

using Leader;

using System;

namespace MoreInfo.Mod.Patches
{
	public class ShowHiddenStatsPatch : IPatch
    {
        public bool IsEnabled { get; set; }

        //TODO: Hook into the character sheet to display Critical Hit chance, if possible

        public void Enable(Type thisType, Harmony harmony)
        {
           
        }

        public void Disable(Type thisType, Harmony harmony)
        {
            
        }
    }
}
