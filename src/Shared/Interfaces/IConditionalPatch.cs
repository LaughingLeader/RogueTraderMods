namespace Leader
{
	public interface IConditionalPatch : IPatch
	{
		bool CanEnablePatch { get; }
	}
}
