namespace Goblfin
{
	public static class ChannelBag<T> where T : struct, IElementChannel
	{
		public static readonly T Channel = new();
	}
}