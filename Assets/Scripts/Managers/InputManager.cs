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
	public readonly string keyBindShopBaseBuildingsName = "Building Shop";
	public readonly string keyBindShopVehProdBuildingsName = "Veh Prod Shop";
	public readonly string keyBindShopLightUnitsName = "Light Unit Shop";
	public readonly string keyBindShopHeavyUnitsName = "Heavy Unit Shop";
	public readonly string keyBindMiniMapName = "Minimap";
	public readonly string keyBindUnitProdQueue = "Unit Prod Queue";
	public readonly string keyBindUnitGroupsList = "Unit Group List";

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
			keyBindShopBaseBuildingsName,
			keyBindShopVehProdBuildingsName,
			keyBindShopLightUnitsName,
			keyBindShopHeavyUnitsName,
			keyBindMiniMapName,
			keyBindUnitProdQueue,
			keyBindUnitGroupsList,

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
			[keyBindShopBaseBuildingsName] = KeyCode.F1,
			[keyBindShopVehProdBuildingsName] = KeyCode.F2,
			[keyBindShopLightUnitsName] = KeyCode.F3,
			[keyBindShopHeavyUnitsName] = KeyCode.F4,
			[keyBindMiniMapName] = KeyCode.M,
			[keyBindUnitProdQueue] = KeyCode.Tab,
			[keyBindUnitGroupsList] = KeyCode.CapsLock,

			[keyBindCameraForwardName] = KeyCode.W,
			[keyBindCameraBackwardsName] = KeyCode.S,
			[keyBindCameraLeftName] = KeyCode.A,
			[keyBindCameraRightName] = KeyCode.D,
			[keyBindCameraUpName] = KeyCode.R,
			[keyBindCameraDownName] = KeyCode.C,
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
		//if key is escape cancel rebinding
		if (key == KeyCode.Escape)
		{
			MenuUIManager.Instance.CancelKeybindButtonDisplay(buttonNum, InputManager.Instance.keyBindDictionary[Instance.keybindNames[buttonNum]]);
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("key Rebinding Canceled", 2f);
		}
		else if (CheckIfValidKeybind(key))
		{
			InputManager.Instance.keyBindDictionary[buttonName] = key;
			MenuUIManager.Instance.UpdateKeybindButtonDisplay(buttonNum, key);
		}
		else
		{
			MenuUIManager.Instance.CancelKeybindButtonDisplay(buttonNum, InputManager.Instance.keyBindDictionary[Instance.keybindNames[buttonNum]]);
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("key already bound", 2f);
		}
		keyToRebind = "";
		buttonNumToRebind = -1;
	}
	public bool CheckIfValidKeybind(KeyCode newKeybind)
	{
		Boolean keyExists = keyBindDictionary.ContainsValue(newKeybind);
		if (keyExists)
			return false;
		else
			return true;
	}
}
