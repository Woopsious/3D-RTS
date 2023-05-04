using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNodes : MonoBehaviour
{
	public bool isCrystalNode;
	public int resourcesAmount;

	public bool isBeingMined;

	public bool isEmpty;

	public GameObject MineLocation;

	public void CheckResourceCount()
	{
		if (resourcesAmount <= 0)
		{
			isEmpty = true;
			isBeingMined = false;
		}
	}
}
