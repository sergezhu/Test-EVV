namespace Code.FX
{
	using UnityEngine;
	using Utilities.Extensions;

	public class FXWrapper : MonoBehaviour
	{
		[SerializeField] private ParticleSystem ps;
		[SerializeField] private ParticleSystem trailPS;

		private Transform fxTransform;
		private bool isCached;
		private ParticleSystem.EmissionModule psEmission;
		private ParticleSystem.MainModule psMain;
		private ParticleSystemRenderer psRenderer;

		private TrailRenderer[] trails;
		private Transform wrapperTransform;

		public bool IsAlive => ps.IsAlive();
		public float LifeTime => ps.main.duration;
		public float TrailLifeTime => trailPS != null ? trailPS.main.duration : 0;

		private Transform FXTransform => fxTransform = fxTransform == null ? ps.transform : fxTransform;

		private void Awake()
		{
			trails = GetComponentsInChildren<TrailRenderer>();
		}


		public void Play()
		{
			if (!ps.isPlaying)
				ps.Play(true);
		}

		public void PlayProperly(bool withChildren = true)
		{
			//Debug.Log( $"FX [{name}] : PlayProperly" );

			Stop();

			ps.Play(withChildren);
		}

		public void Stop()
		{
			if (ps.isPlaying) ps.Stop(true);

			foreach (var trail in trails)
				try
				{
					trail.Clear();
				}
				catch (MissingReferenceException)
				{
					Debug.LogError($"Missed trail on {transform.parent.name}");
				}
		}

		public void Pause()
		{
			if (!ps.isPaused)
				ps.Pause(true);
		}

		public void SetPosition(Vector3 position)
		{
			wrapperTransform.position = position;
		}

		public void SetRotation(Quaternion rotation)
		{
			wrapperTransform.rotation = rotation;
		}

		public void Show()
		{
			MonoExtensions.Show(this);
		}

		public void Hide()
		{
			MonoExtensions.Hide(this);
		}

		public void ShowFX()
		{
			FXTransform.gameObject.SetActive(true);
		}

		public void HideFX()
		{
			FXTransform.gameObject.SetActive(false);
		}

		public void CacheValues()
		{
			if (isCached)
				return;

			CacheValuesInternal();

			isCached = true;
		}

		protected virtual void CacheValuesInternal()
		{
			wrapperTransform = transform;
			fxTransform = ps.transform;
			psMain = ps.main;
			psEmission = ps.emission;
			psRenderer = ps.GetComponent<ParticleSystemRenderer>();
		}
	}
}