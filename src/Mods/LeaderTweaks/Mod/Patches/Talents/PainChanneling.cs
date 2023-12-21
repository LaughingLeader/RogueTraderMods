using Kingmaker.UnitLogic.Mechanics.Actions;

namespace LeaderTweaks.Mod.Patches.Talents
{
	public static class PainChanneling
	{
		private static readonly string PainChannelingFeature = "dee534eedb7a4478b525630c39ec1c96";

		public static bool OnDamage(ContextActionDealDamage __instance, ref int __result)
		{
			if (__instance.Owner?.AssetGuid == PainChannelingFeature && __instance.TargetEntity == __instance.Context.MaybeOwner)
			{
				__result = 0;
				return false;
			}
			return true;
		}
	}
}
