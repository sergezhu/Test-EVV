namespace Code.Core
{
	using UnityEngine;
	using System.Collections;
	
	public interface ICoroutineRunner
	{
		Coroutine StartCoroutine(IEnumerator routine);
		Coroutine StartCoroutine(string methodName);
		Coroutine StartCoroutine(string methodName, object value);
		void StopCoroutine(IEnumerator routine);
		void StopCoroutine(Coroutine routine);
		void StopCoroutine(string methodName);
		void StopAllCoroutines();
	}

	public class CoroutineRunner : MonoBehaviour, ICoroutineRunner
	{
		public new Coroutine StartCoroutine(IEnumerator routine) => base.StartCoroutine(routine);
		public new Coroutine StartCoroutine(string methodName) => base.StartCoroutine(methodName);
		public new Coroutine StartCoroutine(string methodName, object value) => base.StartCoroutine(methodName, value);
		public new void StopCoroutine(IEnumerator routine) => base.StopCoroutine(routine);
		public new void StopCoroutine(Coroutine routine) => base.StopCoroutine(routine);
		public new void StopCoroutine(string methodName) => base.StopCoroutine(methodName);
		public new void StopAllCoroutines() => base.StopAllCoroutines();
	}
}