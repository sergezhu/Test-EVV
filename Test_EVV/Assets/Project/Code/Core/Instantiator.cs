namespace Code.Core
{
	using UnityEngine;

	public interface IInstantiator
	{
		T Instantiate<T>(T original) where T : Component;
		T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Component;
		T Instantiate<T>(T original, Transform parent) where T : Component;
	}

	public class Instantiator : IInstantiator
	{
		public T Instantiate<T>(T original) where T : Component
		{
			return Object.Instantiate(original);
		}

		public T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Component
		{
			T clone = Object.Instantiate(original);
			clone.transform.position = position;
			clone.transform.rotation = rotation;
			return clone;
		}

		public T Instantiate<T>(T original, Transform parent) where T : Component
		{
			T clone = Object.Instantiate(original, parent);
			return clone;
		}
	}
}