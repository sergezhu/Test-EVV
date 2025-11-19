namespace Utilities.Core
{
	public interface ICloneable<out TData>
	{
		public TData Clone();
	}
}