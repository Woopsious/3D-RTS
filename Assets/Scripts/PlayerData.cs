using Newtonsoft.Json.Linq;
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
	public Dictionary<string, KeyCode> KeyBindDictionary;
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
