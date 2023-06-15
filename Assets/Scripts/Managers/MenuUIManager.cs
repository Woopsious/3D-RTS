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
	public GameObject keybindButtonPrefab;
	public List<Button> keybindButtonList;

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

	//FUNCTIONS TO CHANGE KEYBINDS
	public void SetUpKeybindButtonNames()
	{
		int i = 0;
		foreach (Transform child in KeybindParentObj.transform)
		{
			Debug.Log("index is: " + i);
			int closureIndex = i;
			child.GetComponentInChildren<Text>().text = "Keybind for: " + InputManager.Instance.keybindNames[closureIndex];

			GameObject go = Instantiate(keybindButtonPrefab);
			go.transform.SetParent(child.transform, false);
			Button btn = go.GetComponent<Button>();

			btn.onClick.AddListener(delegate { KeyToRebind(InputManager.Instance.keybindNames[closureIndex]); });
			btn.onClick.AddListener(delegate { KeyToRebindButtonNum(closureIndex); });

			KeyCode keyCode = InputManager.Instance.keyBindDictionary[InputManager.Instance.keybindNames[closureIndex]];
			go.GetComponentInChildren<Text>().text = InputManager.Instance.keyBindDictionary[InputManager.Instance.keybindNames[closureIndex]].ToString();

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
	public void KeyBindsSave()
	{
		InputManager.Instance.SavePlayerKeybinds();
	}
	public void KeyBindsReset()
	{
		InputManager.Instance.ResetKeybindsToDefault();
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
