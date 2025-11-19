namespace Utilities
{
	public class ElapsedTimer
	{
		private float elapsedTime;
		private bool isRunning;

		public float ElapsedTime => elapsedTime;
		public bool IsRunning => isRunning;

		public ElapsedTimer()
		{
			Reset();
		}

		public void Start()
		{
			isRunning = true;
		}

		public void Stop()
		{
			isRunning = false;
		}

		public void Reset()
		{
			elapsedTime = 0f;
			isRunning = false;
		}

		public void Restart()
		{
			Reset();
			Start();
		}

		public void Tick( float deltaTime )
		{
			if ( !isRunning ) return;

			elapsedTime += deltaTime;
		}
	}
}