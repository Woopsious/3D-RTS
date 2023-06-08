using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{
	public UnitStateController unit;

	public void OnTriggerEnter(Collider other)
	{
		unit.AddTargetsOnFOVEnter(other.gameObject);
	}

	public void OnTriggerExit(Collider other)
	{
		unit.RemoveTargetsOnFOVExit(other.gameObject);
	}
}
