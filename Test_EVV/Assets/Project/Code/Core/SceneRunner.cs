namespace Code.Core
{
	using System.Collections;
	using Code.MergeSystem;
	using UnityEngine;

	public class SceneRunner
	{
		private MergeBoardController mergeBoardController;
		private ICoroutineRunner coroutineRunner;

		public SceneRunner(MergeBoardController mergeBoardController, ICoroutineRunner coroutineRunner)
		{
			this.mergeBoardController = mergeBoardController;
			this.coroutineRunner = coroutineRunner;
		}

		public void Run()
		{
			coroutineRunner.StartCoroutine(RunRoutine());
		}
		
		private IEnumerator RunRoutine()
		{
			yield return new WaitForSeconds(1f);
			
			mergeBoardController.Activate();
		}
	}
}