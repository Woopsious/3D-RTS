using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
	public static PlayerData Instance;

	public float backgroundMusicVolumeData;
	public float menuSFXVolumeData;
	public float gameSFXVolumeData;

	public string hotKeyTestOne;
	public string hotKeyTestTwo;
	public string hotKeyShopBuildings;
	public string hotKeyShopLightUnits;
	public string hotKeyShopHeavyUnits;
	public string hotKeyMiniMap;

	public string hotKeyCameraForward;
	public string hotKeyCameraBackwards;
	public string hotKeyCameraLeft;
	public string hotKeyCameraRight;
	public string hotKeyCameraUp;
	public string hotKeyCameraDown;
	public string hotKeyCameraRotateLeft;
	public string hotKeyCameraRotateRight;
}

[System.Serializable]
public class GameData
{
	public static GameData Instance;

	public List<BuildingData> BuildingData;
	public List<UnitData> UnitData;
}
[System.Serializable]
public class BuildingData
{
	public static BuildingData Instance;

	public int costData;
	public float xPos, yPos, zPos;
	public float xRot, yRot, zRot, wRot;
}
[System.Serializable]
public class UnitData
{
	public static UnitData Instance;

	public int costData;
	public float xPos, yPos, zPos;
	public float xRot, yRot, zRot, wRot;
}
