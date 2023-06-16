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
	public readonly string keyBindShopBuildingsName = "Building Shop";
	public readonly string keyBindShopLightUnitsName = "Light Unit Shop";
	public readonly string keyBindShopHeavyUnitsName = "Heavy Unit Shop";
	public readonly string keyBindMiniMapName = "Minimap";

	public readonly string keyBindCameraForwardName = "Camera Forward";
	public readonly string keyBindCameraBackwardsName = "Camera Backwards";
	public readonly string keyBindCameraLeftName = "Camera Left";
	public readonly string keyBindCameraRightName = "Camera Right";
	public readonly string keyBindCameraUpName = "Camera Up";
	public readonly string keyBindCameraDownName = "Camera Down";
	public readonly string keyBindCameraRotateLeftName = "Camera Rotate Left";
	public readonly string keyBindCameraRotateRightName = "Camera Rotate Right";

	[Header("Dynamic Refs")]
	public string keyToRebind = "";
	public int buttonNumToRebind = -1;

	public void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}
	public void Update()
	{
		CheckForInputWhenRebindingKey();
	}

	//save load player keybinds
	public void SavePlayerKeybinds()
	{
		GameManager.Instance.LocalCopyOfPlayerData.KeyBindDictionary = InputManager.Instance.keyBindDictionary;
	}
	public void LoadPlayerKeybinds()
	{
		InputManager.Instance.keyBindDictionary = GameManager.Instance.LocalCopyOfPlayerData.KeyBindDictionary;
	}

	//KEY REBINDING FUNCTIONS
	public void SetUpKeybindDictionary()
	{
		keybindNames = new List<string>
		{
			keyBindTestOneName,
			keyBindTestTwoName,
			keyBindTestThreeName,
			keyBindShopBuildingsName,
			keyBindShopLightUnitsName,
			keyBindShopHeavyUnitsName,
			keyBindMiniMapName,
			keyBindCameraForwardName,
			keyBindCameraBackwardsName,
			keyBindCameraLeftName,
			keyBindCameraRightName,
			keyBindCameraUpName,
			keyBindCameraDownName,
			keyBindCameraRotateLeftName,
			keyBindCameraRotateRightName
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
			[keyBindShopBuildingsName] = KeyCode.B,
			[keyBindShopLightUnitsName] = KeyCode.G,
			[keyBindShopHeavyUnitsName] = KeyCode.H,
			[keyBindMiniMapName] = KeyCode.M,
			[keyBindCameraForwardName] = KeyCode.W,
			[keyBindCameraBackwardsName] = KeyCode.S,
			[keyBindCameraLeftName] = KeyCode.A,
			[keyBindCameraRightName] = KeyCode.D,
			[keyBindCameraUpName] = KeyCode.Space,
			[keyBindCameraDownName] = KeyCode.LeftControl,
			[keyBindCameraRotateLeftName] = KeyCode.Q,
			[keyBindCameraRotateRightName] = KeyCode.E
		};
	}
	public void CheckForInputWhenRebindingKey()
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
		//add in check to make sure new keybind is valid later
		CheckIfValidKeybind(key);

		keyBindDictionary[buttonName] = key;
		MenuUIManager.Instance.UpdateKeybindButtonDisplay(buttonNum, key);

		keyToRebind = "";
		buttonNumToRebind = -1;
	}
	public void CheckIfValidKeybind(KeyCode newKeybind)
	{
		Boolean keyExists = keyBindDictionary.ContainsValue(newKeybind);
		if (keyExists)
		{
			Debug.Log("key exists");
		}
		else
		{
			Debug.Log("key doesnt exists");
		}

	}
}
