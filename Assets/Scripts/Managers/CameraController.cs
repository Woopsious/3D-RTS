using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
	public TerrainCollider terrainCollider;

	private readonly float moveSpeed = 50f;
	private readonly float turnSpeed = 100f;

	private readonly float minBounds = 0;
	private readonly float maxBounds = 256;
	private readonly float minHeightBounds = 25;
	private readonly float maxHeightBounds = 50;

	private float prevTerrainHeight;

	public void Start()
	{
		if (terrainCollider != null)
			prevTerrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
	}

	public void Update()
	{
		MoveCamera();
		RotateCamera();
		ClampPosition();

		if (terrainCollider != null)
			AdjustHeight();
	}
	public void ClampPosition()
	{
		gameObject.transform.position = new Vector3(Mathf.Clamp(transform.position.x, minBounds, maxBounds), 
			Mathf.Clamp(transform.position.y, minHeightBounds, maxHeightBounds), Mathf.Clamp(transform.position.z, minBounds, maxBounds));
	}
	public void MoveCamera()
	{
		if (Input.GetKey(KeyCode.W))
		{
			transform.position += moveSpeed * Time.unscaledDeltaTime * transform.forward;
		}
		if (Input.GetKey(KeyCode.A))
		{
			transform.position -= moveSpeed * Time.unscaledDeltaTime * transform.right;
		}
		if (Input.GetKey(KeyCode.S))
		{
			transform.position -= moveSpeed * Time.unscaledDeltaTime * transform.forward;
		}
		if (Input.GetKey(KeyCode.D))
		{
			transform.position += moveSpeed * Time.unscaledDeltaTime * transform.right;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			transform.position += moveSpeed * Time.unscaledDeltaTime * transform.up;
		}
		if (Input.GetKey(KeyCode.LeftControl))
		{
			transform.position -= moveSpeed * Time.unscaledDeltaTime * transform.up;
		}
	}
	public void RotateCamera()
	{
		if (Input.GetKey(KeyCode.Q))
		{
			transform.eulerAngles -= new Vector3(transform.rotation.x, turnSpeed, transform.rotation.y) * Time.unscaledDeltaTime;
		}
		if (Input.GetKey(KeyCode.E))
		{
			transform.eulerAngles -= new Vector3(transform.rotation.x, -turnSpeed, transform.rotation.y) * Time.unscaledDeltaTime;
		}
	}

	public void AdjustHeight()
	{
		float terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
		float changeInHeight = 0;

		if (prevTerrainHeight != terrainHeight)
		{
			changeInHeight = prevTerrainHeight - terrainHeight;
			transform.position = new Vector3(transform.position.x, transform.position.y - changeInHeight, transform.position.z);
			prevTerrainHeight = terrainHeight;
			//Debug.LogWarning("change Height");
		}
		/*
		float terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
		float changeInHeight = 0;

		if (prevTerrainHeight != terrainHeight)
		{
			changeInHeight = prevTerrainHeight - terrainHeight;

			float yPos = Mathf.SmoothDamp(transform.position.y, transform.position.y - changeInHeight, ref velocity, 1f);

			transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
			prevTerrainHeight = terrainHeight;
			Debug.LogWarning("change Height");
		}

		/*
		float terrainHeight;
		terrainHeight = terrainCollider.terrainData.GetHeight((int)transform.position.x, (int)transform.position.z);

		Debug.Log(terrainHeight);

		float changeInHeight = 0;

		if (prevTerrainHeight != terrainHeight)
		{
			changeInHeight = prevTerrainHeight - terrainHeight;
			prevTerrainHeight = terrainHeight;
		}

		if (changeInHeight > 0.5f)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
			Debug.Log("Increase Height");
		}
		else if (changeInHeight < -0.5f)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
			Debug.Log("Decrease Height");
		}

		/*
		Vector3 cameraPosOffset = new Vector3(transform.position.x, transform.position.y - 100, transform.position.z);

		Debug.DrawRay(cameraPosOffset, new Vector3(transform.position.x, transform.position.y + 100, transform.position.z));

		if (Physics.Raycast(cameraPosOffset, Vector3.up, out RaycastHit hit, 200))
		{
			float distance = Vector3.Distance(transform.position, hit.transform.position);

			float storeDistance = 0;

			if (!Mathf.Approximately(distance, storeDistance))
			{
				storeDistance = distance;
				float offset = distance - storeDistance;
				transform.position = new Vector3(transform.position.x, transform.position.y + offset, transform.position.z);
			}
			else
				return;
		}
		*/
	}
}
