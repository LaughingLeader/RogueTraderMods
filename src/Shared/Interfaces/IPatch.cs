using HarmonyLib;

using System;

namespace Leader
{
	public interface IPatch
	{
		bool IsEnabled { get; set; }
		void Enable(Type thisType, Harmony harmony);
		void Disable(Type thisType, Harmony harmony);
	}
}
