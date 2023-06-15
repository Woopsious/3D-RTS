using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	public static InputManager Instance;

	public Dictionary<string, KeyCode> keyBindDictionary;
	public List<string> keybindNames;

	[Header("hotKey Name Refs")]
    public readonly string keyBindTestOneName = "TestOne";
	public readonly string keyBindTestTwoName = "TestTwo";
	public readonly string keyBindTestThreeName = "TestThree";
	public readonly string keyBindTestFourName = "TestFour";
	public readonly string keyBindShopBuildingsName;
	public readonly string keyBindShopLightUnitsName;
	public readonly string keyBindShopHeavyUnitsName;
	public readonly string keyBindMiniMapName;

	public readonly string keyBindCameraForwardName;
	public readonly string keyBindCameraBackwardsName;
	public readonly string keyBindCameraLeftName;
	public readonly string keyBindCameraRightName;
	public readonly string keyBindCameraUpName;
	public readonly string keyBindCameraDownName;
	public readonly string keyBindCameraRotateLeftName;
	public readonly string keyBindCameraRotateRightName;

	[Header("Dynamic Refs")]
	public string keyToRebind = "";
	public int buttonNumToRebind = -1;

	public void Awake()
	{
		Instance = this;
	}
	public void SetUpKeybindDictionary()
	{
		keybindNames= new List<string> 
		{
			keyBindTestOneName, 
			keyBindTestTwoName, 
			keyBindTestThreeName, 
		};

		ResetKeybindsToDefault();
	}
	public void ResetKeybindsToDefault()
	{
		keyBindDictionary = new Dictionary<string, KeyCode>
		{
			[keyBindTestOneName] = KeyCode.Alpha1,
			[keyBindTestTwoName] = KeyCode.Alpha2,
			[keyBindTestThreeName] = KeyCode.Alpha3,
		};
	}
	public void SavePlayerKeybinds()
	{
		//GameManager.Instance.LocalCopyOfPlayerData.KeyBindDictionary.Clear();
		GameManager.Instance.LocalCopyOfPlayerData.KeyBindDictionary = InputManager.Instance.keyBindDictionary;
		Debug.Log("after overwrite" + GameManager.Instance.LocalCopyOfPlayerData.KeyBindDictionary.Count);
	}
	public void LoadPlayerKeybinds()
	{
		//InputManager.Instance.keyBindDictionary.Clear();
		for (int i = 0; i < InputManager.Instance.keyBindDictionary.Count; i++)
		{
			Debug.Log(InputManager.Instance.keyBindDictionary.Keys + " : " + InputManager.Instance.keyBindDictionary.Values);
		}

		InputManager.Instance.keyBindDictionary = GameManager.Instance.LocalCopyOfPlayerData.KeyBindDictionary;

		for (int i = 0; i < InputManager.Instance.keyBindDictionary.Count; i++)
		{
			Debug.Log(InputManager.Instance.keyBindDictionary.Keys + " : " + InputManager.Instance.keyBindDictionary.Values);
		}
	}

	//KEYBIND FUNCTIONS
	public void Update()
	{
		if (keyToRebind != "" && Input.anyKeyDown)
		{
			foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKeyDown(key))
					TrySetNewKeybind(keyToRebind, buttonNumToRebind, key);
			}
		}
	}
	public void TrySetNewKeybind(string buttonName, int buttonNum, KeyCode key)
	{
		//add in check to make sure new keybidn is valid later

		keyBindDictionary[buttonName] = key;
		MenuUIManager.Instance.UpdateKeybindButtonDisplay(buttonNum, key);

		keyToRebind = "";
		buttonNumToRebind = -1;
	}
	public void CheckIfValidKeybind(char newKeybind)
	{
		//foreach 
	}
}
