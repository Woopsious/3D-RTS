using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	[Header("Audio Sources")]
	public List<AudioSource> backgroundAudioSFXs;
	//public AudioSource backgroundMusic;
	public AudioSource menuSFX;
	public AnnouncerSystem announcerSystem;
	//public AudioSource gameSFX;
	//public AudioSource AnnouncerSFX;

	[Header("Audio Volumes")]
	public float backgroundMusicVolume;
	public float menuSFXVolume;
	public float AnnouncerSFXVolume;
	public float gameSFXVolume;

	[Header("Audio Sliders")]
	public Slider backgroundMusicSlider, menuSFXSlider, announcerSFXSlider, gameSFXSlider;

	public void Awake()
	{
		Instance = this;
	}
	//store/save value of new audio volumes
	public void UpdateBackgroundVolume()
	{
		GameManager.Instance.LocalCopyOfPlayerData.backgroundMusicVolumeData = backgroundMusicSlider.value;
		backgroundMusicVolume = backgroundMusicSlider.value;
		AdjustBackgroundSFXAudio();
	}
	public void UpdateMenuSFXVolume()
	{
		GameManager.Instance.LocalCopyOfPlayerData.menuSFXVolumeData = menuSFXSlider.value;
		menuSFXVolume = menuSFXSlider.value;
		AdjustMenuSFXAudio();
	}
	public void UpdateAnnouncerSFXVolume()
	{
		GameManager.Instance.LocalCopyOfPlayerData.announcerSFXVolumeData = announcerSFXSlider.value;
		AnnouncerSFXVolume = announcerSFXSlider.value;
		AdjustAnnouncerSFXAudio();
	}
	public void UpdateGameSFXVolume()
	{
		GameManager.Instance.LocalCopyOfPlayerData.gameSFXVolumeData = gameSFXSlider.value;
		gameSFXVolume = gameSFXSlider.value;
	}

	//update audio sources to match new audio volumes when closing settings menu
	public void AdjustAudioVolumes()	//only gameSFXs called here as preformance may drop from foreach loops on slider.OnValueChange
	{
		AdjustGameSFXAudio();
	}
	private void AdjustBackgroundSFXAudio()
	{
		foreach (AudioSource audio in backgroundAudioSFXs)
			audio.volume = backgroundMusicVolume;
	}
	private void AdjustMenuSFXAudio()
	{
		menuSFX.volume = menuSFXVolume;
	}
	private void AdjustGameSFXAudio()
	{
		Entities[] entities = FindObjectsOfType<Entities>();
		foreach (Entities entity in entities)
			entity.UpdateEntityAudioVolume();
	}
	private void AdjustAnnouncerSFXAudio()
	{
		if (announcerSystem != null)
			announcerSystem.announcerSFX.volume = AnnouncerSFXVolume;
	}

	//load/reset audio volumes
	public void LoadSoundSettings()
	{
		backgroundMusicVolume = GameManager.Instance.LocalCopyOfPlayerData.backgroundMusicVolumeData;
		menuSFXVolume = GameManager.Instance.LocalCopyOfPlayerData.menuSFXVolumeData;
		AnnouncerSFXVolume = GameManager.Instance.LocalCopyOfPlayerData.announcerSFXVolumeData;
		gameSFXVolume = GameManager.Instance.LocalCopyOfPlayerData.gameSFXVolumeData;

		backgroundMusicSlider.value = GameManager.Instance.LocalCopyOfPlayerData.backgroundMusicVolumeData;
		menuSFXSlider.value = GameManager.Instance.LocalCopyOfPlayerData.menuSFXVolumeData;
		announcerSFXSlider.value = GameManager.Instance.LocalCopyOfPlayerData.announcerSFXVolumeData;
		gameSFXSlider.value = GameManager.Instance.LocalCopyOfPlayerData.gameSFXVolumeData;
	}
	public void ResetAudioSettingsLocally()
	{
		backgroundMusicSlider.value = 0.5f;
		menuSFXSlider.value = 0.5f;
		announcerSFXSlider.value = 0.5f;
		gameSFXSlider.value = 0.5f;

		backgroundMusicVolume = 0.5f;
		menuSFXVolume = 0.5f;
		AnnouncerSFXVolume = 0.5f;
		gameSFXVolume = 0.5f;

		GameManager.Instance.LocalCopyOfPlayerData.backgroundMusicVolumeData = backgroundMusicVolume;
		GameManager.Instance.LocalCopyOfPlayerData.menuSFXVolumeData = menuSFXVolume;
		GameManager.Instance.LocalCopyOfPlayerData.announcerSFXVolumeData = AnnouncerSFXVolume;
		GameManager.Instance.LocalCopyOfPlayerData.gameSFXVolumeData = gameSFXVolume;
	}
}
