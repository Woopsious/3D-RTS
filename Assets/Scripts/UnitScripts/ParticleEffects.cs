using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffects : MonoBehaviour
{
	public AudioSource Audio;
	public float destroyInSeconds;

	public void Start()
	{
		Audio.volume = AudioManager.Instance.gameSFX.volume;
		Destroy(gameObject, destroyInSeconds);
	}
}
