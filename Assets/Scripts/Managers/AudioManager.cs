using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	[Header("Audio Sources")]
	public AudioSource backgroundMusic;
	public AudioSource menuSFX;
	public AudioSource gameSFX;

	public Slider backgroundMusicSlider, menuSFXSlider, gameSFXSlider;

	public void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
			Destroy(gameObject);
	}
	public void AdjustAudioVolume()
	{
		Entities[] entities = FindObjectsOfType<Entities>();

		foreach (Entities entity in entities)
		{
			entity.UpdateEntityAudioVolume();
		}
	}
	public void UpdateBackgroundSounds()
	{
		GameManager.Instance.LocalCopyOfPlayerData.backgroundMusicVolumeData = backgroundMusicSlider.value;
		backgroundMusic.volume = backgroundMusicSlider.value;
	}
	public void UpdateMenuSFXSounds()
	{
		GameManager.Instance.LocalCopyOfPlayerData.menuSFXVolumeData = menuSFXSlider.value;
		menuSFX.volume = menuSFXSlider.value;
	}
	public void UpdateGameSFXSounds()
	{
		GameManager.Instance.LocalCopyOfPlayerData.gameSFXVolumeData = gameSFXSlider.value;
		gameSFX.volume = gameSFXSlider.value;
	}
	public void LoadSoundSettings()
	{
		backgroundMusic.volume = GameManager.Instance.LocalCopyOfPlayerData.backgroundMusicVolumeData;
		menuSFX.volume = GameManager.Instance.LocalCopyOfPlayerData.menuSFXVolumeData;
		gameSFX.volume = GameManager.Instance.LocalCopyOfPlayerData.gameSFXVolumeData;

		backgroundMusicSlider.value = GameManager.Instance.LocalCopyOfPlayerData.backgroundMusicVolumeData;
		menuSFXSlider.value = GameManager.Instance.LocalCopyOfPlayerData.menuSFXVolumeData;
		gameSFXSlider.value = GameManager.Instance.LocalCopyOfPlayerData.gameSFXVolumeData;
	}
}
