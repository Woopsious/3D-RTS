using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Unity.VisualScripting;
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
	public readonly string keyBindTechTreeName = "Tech Tree";
	public readonly string keyBindUnitProdQueue = "Unit Prod Queue";
	public readonly string keyBindUnitGroupsList = "Unit Group List";
	public readonly string keyBindMiniMapName = "Minimap";
	public readonly string keyBindTacViewName = "Tactical View";

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
		GameManager.Instance.LocalCopyOfPlayerData.keybindNames = InputManager.Instance.keybindNames;
		GameManager.Instance.LocalCopyOfPlayerData.KeyBindDictionary = InputManager.Instance.keyBindDictionary;
	}
	public void LoadPlayerKeybinds()
	{
		try
		{
			InputManager.Instance.keybindNames = GameManager.Instance.LocalCopyOfPlayerData.keybindNames;
			InputManager.Instance.keyBindDictionary = GameManager.Instance.LocalCopyOfPlayerData.KeyBindDictionary;
		}
		catch
		{
			Debug.LogError("failed to load keybinds player data");
			ResetKeybinds();
		}
	}

	public async Task CreateKeyBindDictionary()
	{
		await CreateNamesList();
		await CreateDictionary();
	}
	public Task CreateNamesList()
	{
		keybindNames = new List<string>
		{
			keyBindShopBaseBuildingsName,
			keyBindShopVehProdBuildingsName,
			keyBindShopLightUnitsName,
			keyBindShopHeavyUnitsName,
			keyBindTechTreeName,
			keyBindUnitProdQueue,
			keyBindUnitGroupsList,
			keyBindMiniMapName,
			keyBindTacViewName,

			keyBindCameraForwardName,
			keyBindCameraBackwardsName,
			keyBindCameraLeftName,
			keyBindCameraRightName,
			keyBindCameraUpName,
			keyBindCameraDownName,
			keyBindCameraRotateLeftName,
			keyBindCameraRotateRightName
		};
		return Task.CompletedTask;
	}
	public Task CreateDictionary()
	{
		keyBindDictionary = new Dictionary<string, KeyCode>
		{
			[keyBindShopBaseBuildingsName] = KeyCode.F1,
			[keyBindShopVehProdBuildingsName] = KeyCode.F2,
			[keyBindShopLightUnitsName] = KeyCode.F3,
			[keyBindShopHeavyUnitsName] = KeyCode.F4,
			[keyBindTechTreeName] = KeyCode.T,
			[keyBindUnitProdQueue] = KeyCode.Tab,
			[keyBindUnitGroupsList] = KeyCode.CapsLock,
			[keyBindMiniMapName] = KeyCode.M,
			[keyBindTacViewName] = KeyCode.V,

			[keyBindCameraForwardName] = KeyCode.W,
			[keyBindCameraBackwardsName] = KeyCode.S,
			[keyBindCameraLeftName] = KeyCode.A,
			[keyBindCameraRightName] = KeyCode.D,
			[keyBindCameraUpName] = KeyCode.R,
			[keyBindCameraDownName] = KeyCode.C,
			[keyBindCameraRotateLeftName] = KeyCode.Q,
			[keyBindCameraRotateRightName] = KeyCode.E
		};
		return Task.CompletedTask;
	}
	public Task CheckForKeyBindChangesInData()
	{
		if (GameManager.Instance.LocalCopyOfPlayerData.keybindNames.Count != keybindNames.Count)
		{
			Debug.LogError("count change detected");
			ResetKeybinds();
			GameManager.Instance.SavePlayerData();

			LoadPlayerKeybinds();
			return Task.CompletedTask;
		}
		int i = 0;
		foreach (string name in GameManager.Instance.LocalCopyOfPlayerData.keybindNames)
		{
			if (name != keybindNames[i])
			{
				Debug.LogError(name);
				Debug.LogError(keybindNames[i]);

				Debug.LogError("name change detected");
				ResetKeybinds();
				GameManager.Instance.SavePlayerData();

				LoadPlayerKeybinds();
				return Task.CompletedTask;
			}
			i++;
		}

		LoadPlayerKeybinds();
		return Task.CompletedTask;
	}
	public async void ResetKeybinds()
	{
		await CreateNamesList();
		await CreateDictionary();
		GameManager.Instance.SavePlayerData();
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
			MenuUIManager.Instance.CancelKeybindButtonDisplay(buttonNum, Instance.keyBindDictionary[Instance.keybindNames[buttonNum]]);
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("key Rebinding Canceled", 2f);
		}
		else if (CheckIfValidKeybind(key))
		{
			InputManager.Instance.keyBindDictionary[buttonName] = key;
			MenuUIManager.Instance.UpdateKeybindButtonDisplay(buttonNum, key);
		}
		else
		{
			MenuUIManager.Instance.CancelKeybindButtonDisplay(buttonNum, Instance.keyBindDictionary[Instance.keybindNames[buttonNum]]);
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
