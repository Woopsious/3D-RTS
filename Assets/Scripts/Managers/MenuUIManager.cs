using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
	public static MenuUIManager Instance;
	//references
	[Header("MenuObj UI Refs")]
	public GameObject mainMenuObj;
	public GameObject highScoreObj;
	public GameObject SettingsObj;
	public GameObject singlePlayerScreenObj;
	public GameObject multiPlayerScreenObj;

	[Header("Menu Button Refs")]
	public Button newSinglePlayerButton, loadSinglePlayerButton;
	public Button newMultiPlayerButton, loadMultiPlayerButton;

	[Header("EasyMode refs")]
	public Text highscoreEasyOne;
	public Text highscoreEasyTwo;
	public Text highscoreEasyThree;
	public Text highscoreEasyFour;
	public Text highscoreEasyFive;
	public Text highscoreEasyOneInfo;
	public Text highscoreEasyTwoInfo;
	public Text highscoreEasyThreeInfo;
	public Text highscoreEasyFourInfo;
	public Text highscoreEasyFiveInfo;

	public void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}
	public void PlayButtonSound()
	{
		AudioManager.Instance.menuSFX.Play();
	}

	//MAIN MENU BUTTONS FUNCTIONS
	public void ShowHighScoreButton()
	{
		mainMenuObj.SetActive(false);
		highScoreObj.SetActive(true);
	}
	public void ShowSettingsButton()
	{
		mainMenuObj.SetActive(false);
		SettingsObj.SetActive(true);
	}
	public void ShowSinglePlayerScreen()
	{
		mainMenuObj.SetActive(false);
		singlePlayerScreenObj.SetActive(true);
	}
	public void ShowMultiPlayerScreen()
	{
		mainMenuObj.SetActive(false);
		multiPlayerScreenObj.SetActive(true);
	}
	public void BackButton()
	{
		mainMenuObj.SetActive(true);
		SettingsObj.SetActive(false);
		highScoreObj.SetActive(false);
		singlePlayerScreenObj.SetActive(false);
		multiPlayerScreenObj.SetActive(false);
		GameManager.Instance.SavePlayerData();
	}
	public void QuitGame()
	{
		GameManager.Instance.SavePlayerData();
		Application.Quit();
	}

	//single player button functions
	public void PlayNewSinglePlayerGame()
	{
		StartCoroutine(GameManager.Instance.WaitForSceneLoad(1));
	}
	public void LoadSinglePlayerGame()
	{
		StartCoroutine(GameManager.Instance.WaitForSceneLoad(1));
	}

	//multi player button functions
	public void PlayNewMultiPlayerGame()
	{
		StartCoroutine(GameManager.Instance.WaitForSceneLoad(1));
	}
	public void LoadMultiPlayerGame()
	{
		StartCoroutine(GameManager.Instance.WaitForSceneLoad(1));
	}

}
