namespace Code.Core
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;

	public class LifetimeController
	{
		private List<ITickable> tickables;
		private List<IInitializable> initializables;


		public LifetimeController()
		{
			tickables = new List<ITickable>();
			initializables = new List<IInitializable>();
			
			CompositeDisposable disposable = new CompositeDisposable();

			Observable.EveryUpdate().Subscribe(_ => Tick(Time.deltaTime)).AddTo(disposable);
		}

		public void AddTickable( ITickable tickable )
		{
			tickables.Add( tickable );
		}

		public void AddInitializable( IInitializable initializable )
		{
			initializables.Add( initializable );
		}

		public void Initialize()
		{
			initializables.ForEach( i => i.Initialize() );
		}

		private void Tick(float deltaTime)
		{
			tickables.ForEach( t => t.Tick(deltaTime) );
		}
	}
}