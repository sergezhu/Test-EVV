namespace Code.AudioManagement
{
	using UnityEngine;
	using UnityEngine.Audio;

	public class AudioLibrary : MonoBehaviour
	{
		[SerializeField] private AudioMixer _audioMixer;
		
		[Space]
		[SerializeField] private AudioSource bgMusic;
		[SerializeField] private AudioSource spawn;
		[SerializeField] private AudioSource rollback;
		[SerializeField] private AudioSource merge;
		
		
		public AudioSource BgMusic => bgMusic;
		public AudioSource Spawn => spawn;
		public AudioSource Rollback => rollback;
		public AudioSource Merge => merge;
		

		public AudioMixer AudioMixer => _audioMixer;
	}
}