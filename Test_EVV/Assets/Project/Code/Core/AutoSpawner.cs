namespace Code.Core
{
	using System.Collections;
	using Code.MergeSystem;
	using UnityEngine;

	public class AutoSpawner : IInitializable
	{
		private readonly MergeConfig config;
		private readonly MergeBoardController boardController;
		private readonly ICoroutineRunner coroutineRunner;
		private readonly WaitForSeconds autoSpawnWait;

		public AutoSpawner(MergeConfig config, MergeBoardController boardController, ICoroutineRunner coroutineRunner)
		{
			this.config = config;
			this.boardController = boardController;
			this.coroutineRunner = coroutineRunner;
			
			autoSpawnWait = new WaitForSeconds(config.AutoSpawnDelay);
		}

		public void Initialize()
		{
			coroutineRunner.StartCoroutine(SpawnRoutine());
		}

		private IEnumerator SpawnRoutine()
		{
			yield return new WaitForSeconds(3f); // start delay
			
			while (true)
			{
				while (boardController.HasEmptyCell == false)
				{
					yield return null;
				}
				
				yield return autoSpawnWait;

				boardController.SpawnRandomItem();
			}
		}
	}
}