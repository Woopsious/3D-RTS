using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInSeconds : MonoBehaviour
{
	public float destroyInSeconds;
	void Start()
	{
		Destroy(gameObject, destroyInSeconds);
	}
}
