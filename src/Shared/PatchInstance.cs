using System;

namespace Leader
{
	public struct PatchInstance
	{
		public Type PatchType { get; set; }
		public IPatch Instance { get; set; }
	}
}
