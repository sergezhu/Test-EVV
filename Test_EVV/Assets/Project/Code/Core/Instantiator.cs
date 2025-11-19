namespace Code.Core
{
	using Code.MergeSystem;
	using UnityEngine;

	public interface IInstantiator
	{
		T Instantiate<T>( T original ) where T : UnityEngine.Component;
		T Instantiate<T>( T original, Vector3 position, Quaternion rotation ) where T : UnityEngine.Component;
		//object InstantiatePrefab( object prefab );
	}

	public class Instantiator : IInstantiator
	{
		public T Instantiate<T>( T original ) where T : UnityEngine.Component
		{
			return UnityEngine.Object.Instantiate( original );
		}

		public T Instantiate<T>( T original, Vector3 position, Quaternion rotation ) where T : UnityEngine.Component
		{
			var clone = UnityEngine.Object.Instantiate( original );
			clone.transform.position = position;
			clone.transform.rotation = rotation;
			return clone;
		}
	}
}