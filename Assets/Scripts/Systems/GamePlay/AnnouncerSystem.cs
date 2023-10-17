using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnouncerSystem : MonoBehaviour
{
	public static AnnouncerSystem Instance;

	public AudioSource announcerSFX;

    public List<AudioClip> actionsPositive;
    public List<AudioClip> actionsNegative;

	public void Awake()
	{
		Instance = this;
	}

	public void PlayActionPositiveSFX()
	{
		CheckIfAudioIsPlaying();
		announcerSFX.clip = actionsPositive[GetRandomNumber(actionsPositive.Count)];
		announcerSFX.Play();
	}
	public void PlayActionNegativeSFX()
	{
		CheckIfAudioIsPlaying();
		announcerSFX.clip = actionsNegative[GetRandomNumber(actionsPositive.Count)];
		announcerSFX.Play();
	}
	public int GetRandomNumber(int max)
	{
		return Random.Range(0, max);
	}
	private void CheckIfAudioIsPlaying()
	{
		if (announcerSFX.isPlaying)
			announcerSFX.Stop();
	}
}
