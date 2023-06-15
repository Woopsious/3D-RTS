using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
	public static MenuUIManager Instance;
	//references
	[Header("MenuObj UI Refs")]
	public GameObject mainMenuObj;
	public GameObject singlePlayerScreenObj;
	public GameObject multiPlayerScreenObj;
	public GameObject highScoreObj;
	public GameObject SettingsObj;
	public GameObject SettingsVolumeObj;
	public GameObject SettingsKeybindsObj;

	[Header("keybinds Ui")]
	public InputField keybindTestOneInputField;
	public InputField keybindTestTwoInputField;

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

		GameManager.Instance.errorManager.CheckForErrorMessageObj();
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
	public void ShowSettingsButton()
	{
		mainMenuObj.SetActive(false);
		SettingsObj.SetActive(true);
	}
	public void ShowSettingsKeybindsButton()
	{
		SettingsObj.SetActive(false);
		SettingsKeybindsObj.SetActive(true);
	}	
	public void ShowSettingsVolumeButton()
	{
		SettingsObj.SetActive(false);
		SettingsVolumeObj.SetActive(true);
	}
	public void SettingsBackButton()
	{
		SettingsObj.SetActive(true);
		SettingsVolumeObj.SetActive(false);
		SettingsKeybindsObj.SetActive(false);

	}
	public void QuitGame()
	{
		GameManager.Instance.SavePlayerData();
		Application.Quit();
	}

	//KEYBIND FUNCTIONS
	public void DisplayKeyBinds()
	{
		keybindTestOneInputField.placeholder.GetComponent<Text>().text = GameManager.Instance.keyBindTestOne;
		keybindTestTwoInputField.placeholder.GetComponent<Text>().text = GameManager.Instance.keyBindTestTwo;
	}
	public void SaveKeyBinds()
	{
		GameManager.Instance.keyBindTestOne = keybindTestOneInputField.textComponent.text;
		GameManager.Instance.keyBindTestTwo = keybindTestTwoInputField.textComponent.text;

		keybindTestOneInputField.text = "";
		keybindTestTwoInputField.text = "";

		DisplayKeyBinds();
	}
	public void ResetPlayerKeybinds()
	{
		GameManager.Instance.keyBindTestOne = KeyCode.J.ToString();
		GameManager.Instance.keyBindTestTwo = KeyCode.K.ToString();
	}
	public void CheckIfValidKeybind(char newKeybind)
	{
		if (char.IsLower(newKeybind))
		{

		}
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
