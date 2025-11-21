namespace Code.AudioManagement
{
	using System;
	using Code.Core;
	using Code.MergeSystem;
	using UniRx;
	using UnityEngine;

	public class AudioController : IDisposable, IInitializable
	{
		private readonly AudioLibrary lib;
		private readonly MergeBoardController boardController;

		private readonly CompositeDisposable disposables;
		private int ticToc;

		public AudioController( AudioLibrary lib, MergeBoardController boardController )
		{
			disposables = new CompositeDisposable();
			
			this.lib = lib;
			this.boardController = boardController;

			Subscribe();
			
			SoundOff();
		}

		public void Dispose()
		{
			disposables?.Dispose();
		}

		public void Initialize()
		{
			Subscribe();
		}

		private void Subscribe()
		{
			boardController.IsItemSpawned
				.Subscribe(_ => PlaySpawn())
				.AddTo(disposables);
			
			boardController.IsItemRollback
				.Subscribe(_ => PlayRollback())
				.AddTo(disposables);
			
			boardController.IsItemsMergedOnBoard
				.Subscribe(_ => PlayMerge())
				.AddTo(disposables);
		}

		private void SoundOff()
		{
			Debug.Log( "Sound Off" );
			SetVolume( 0.001f );
		}

		private void SoundOn()
		{
			Debug.Log( "Sound On" );
			SetVolume( 1 );
		}

		private void SetVolume( float volume )
		{
			var logVolume = Mathf.Log10( volume ) * 20;
			Debug.Log( $"SetVolume : {logVolume}" );
			
			lib.AudioMixer.SetFloat( "MasterVolume", Mathf.Log10( volume ) * 20 );
		}

		private void PlayBgMusic()
		{
			if ( !lib.BgMusic.isPlaying )
				lib.BgMusic.Play();
		}

		private void StopBgMusic()
		{
			if ( lib.BgMusic.isPlaying )
				lib.BgMusic.Stop();
		}

		private void PlaySpawn()
		{
			lib.Spawn.Play();
		}

		private void PlayMerge()
		{
			lib.Merge.Play();
		}

		private void PlayRollback()
		{
			lib.Rollback.Play();
		}
	}
}