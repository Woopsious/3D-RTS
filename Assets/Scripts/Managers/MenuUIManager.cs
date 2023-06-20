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
	public GameObject KeybindParentObj;
	public GameObject keybindPanelPrefab;
	public string keybindDictionaryString;

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
	public void Start()
	{
		GameManager.Instance.OnSceneLoad(0);
	}
	public void PlayButtonSound()
	{
		AudioManager.Instance.menuSFX.Play();
	}

	//FUNCTIONS FOR MAIN MENU BUTTONS
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
		UpdateKeybindButtonDisplay();
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
		GameManager.Instance.SavePlayerData();
	}
	public void QuitGame()
	{
		GameManager.Instance.SavePlayerData();
		Application.Quit();
	}

	//FUNCTIONS TO CHANGE KEYBINDS
	public void SetUpKeybindButtonNames()
	{
		for (int i = 0; i < InputManager.Instance.keyBindDictionary.Count; i++)
		{
			int closureIndex = i;
			GameObject keybindPanelObj = Instantiate(keybindPanelPrefab, KeybindParentObj.transform);
			keybindPanelObj.transform.GetChild(0).GetComponent<Text>().text = "Keybind for: " + InputManager.Instance.keybindNames[closureIndex];

			keybindPanelObj.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate 
				{ KeyToRebind(InputManager.Instance.keybindNames[closureIndex]); });
			keybindPanelObj.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { KeyToRebindButtonNum(closureIndex); });
			keybindPanelObj.transform.GetChild(1).GetComponentInChildren<Text>().text =
				InputManager.Instance.keyBindDictionary[InputManager.Instance.keybindNames[closureIndex]].ToString();
		}
	}
	public void UpdateKeybindButtonDisplay()
	{
		int i = 0;
		foreach (Transform child in KeybindParentObj.transform)
		{
			Text buttonText = child.GetChild(1).GetComponentInChildren<Text>();
			KeyCode keyCode = InputManager.Instance.keyBindDictionary[InputManager.Instance.keybindNames[i]];
			buttonText.text = InputManager.Instance.keyBindDictionary[InputManager.Instance.keybindNames[i]].ToString();
			i++;
		}
	}
	public void KeyToRebind(string buttonName)
	{
		InputManager.Instance.keyToRebind = buttonName;
	}
	public void KeyToRebindButtonNum(int buttonNum)
	{
		InputManager.Instance.buttonNumToRebind = buttonNum;
		KeybindParentObj.transform.GetChild(buttonNum).GetChild(1).GetComponentInChildren<Text>().text = "Press Key To Rebind";
	}
	public void UpdateKeybindButtonDisplay(int buttonNum, KeyCode key)
	{
		KeybindParentObj.transform.GetChild(buttonNum).GetChild(1).GetComponentInChildren<Text>().text = key.ToString();
	}
	public void CancelKeybindButtonDisplay(int buttonNum, KeyCode key)
	{
		KeybindParentObj.transform.GetChild(buttonNum).GetChild(1).GetComponentInChildren<Text>().text = key.ToString();
	}
	public void KeyBindsSave()
	{
		GameManager.Instance.SavePlayerData();
	}
	public void KeyBindsReset()
	{
		InputManager.Instance.ResetKeybindsToDefault();
		UpdateKeybindButtonDisplay();
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
