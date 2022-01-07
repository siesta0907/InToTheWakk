using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundElement
{
	public string soundName;
	public AudioClip[] soundLists;
}


public class SfxSoundManager : MonoBehaviour
{
	public static SfxSoundManager instance;

	[SerializeField] AudioSource[] sources;
	[SerializeField] private SoundElement[] sounds;

	Dictionary<string, AudioClip[]> soundData = new Dictionary<string, AudioClip[]>();

	// Start is called before the first frame update
	void Awake()
    {
		#region singleton
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this);
		}
		else
		{
			Destroy(this.gameObject);
		}
		#endregion

		sources = GetComponents<AudioSource>();
		for (int i = 0; i < sounds.Length; i++)
		{
			string sName = sounds[i].soundName;
			AudioClip[] sl = sounds[i].soundLists;

			if (sl != null)
				soundData[sName] = sl;
		}
	}


	private AudioSource GetEmptyAudioSource()
	{
		for (int i = 0; i < sources.Length; i++)
		{
			if (sources[i].isPlaying == false)
			{
				return sources[i];
			}
		}
		return null;
	}

	public void PlaySound(string soundName)
	{
		if (soundName != "" && soundData[soundName].Length <= 0) return;
		AudioClip clip = soundData[soundName][Random.Range(0, soundData[soundName].Length)];
		//AudioClip clip = soundData[soundName];
		AudioSource emptySource = GetEmptyAudioSource();

		if (emptySource != null && clip != null)
		{
			emptySource.clip = clip;
			emptySource.Play();
		}
	}

	public void SetSfxVolume(float volume)
	{
		foreach (AudioSource a in sources)
		{
			a.volume = volume;
		}
	}

}
