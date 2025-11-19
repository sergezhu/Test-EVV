namespace Utilities
{

	using System;

	public class ReverseTimer
	{
		private float duration;      // Длительность таймера
		private float timeRemaining; // Остаток времени
		private bool isRunning;      // Запущен ли таймер
		private bool isCompleted;    // Завершён ли таймер

		public float Duration => duration;
		public float TimeRemaining => timeRemaining;
		public bool IsRunning => isRunning;
		public bool IsCompleted => isCompleted;

		public Action OnTimerCompleted; // Колбэк по завершению

		public ReverseTimer( float duration )
		{
			this.duration = Math.Max( 0f, duration );
			Reset();
		}

		public void Start()
		{
			if ( timeRemaining > 0f )
			{
				isRunning = true;
				isCompleted = false;
			}
		}

		public void Stop()
		{
			isRunning = false;
		}

		public void Reset()
		{
			timeRemaining = duration;
			isRunning = false;
			isCompleted = false;
		}

		public void Restart()
		{
			Reset();
			Start();
		}

		public void Tick( float deltaTime )
		{
			if ( !isRunning || isCompleted )
				return;

			timeRemaining -= deltaTime;

			if ( timeRemaining <= 0f )
			{
				timeRemaining = 0f;
				isRunning = false;
				isCompleted = true;
				OnTimerCompleted?.Invoke();
			}
		}

		// Установка нового времени
		public void SetDuration( float newDuration )
		{
			duration = Math.Max( 0f, newDuration );
			Reset();
		}
	}

}