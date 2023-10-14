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

		int i = Random.Range(0, actionsPositive.Count);
		announcerSFX.clip = actionsPositive[i];
		announcerSFX.Play();
	}
	private void CheckIfAudioIsPlaying()
	{
		if (announcerSFX.isPlaying)
			announcerSFX.Stop();
	}
}
