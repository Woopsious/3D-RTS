using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
	public List<AudioSource> shootingAudioClips = new List<AudioSource>();
	public List<AudioSource> hitAudioClips = new List<AudioSource>();

	public List<float> attackSpeedTimer = new List<float>();
	public List<float> attackSpeed = new List<float>();

	public void ShootAtEnemy(UnitStateController unit, UnitStateController targetUnit, BuildingManager targetBuilding)
	{

	}
}
