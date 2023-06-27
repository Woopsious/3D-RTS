using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
	public static CameraController instance;
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
		CameraController.instance = this;
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
		if (Input.GetKey(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindCameraForwardName]))
		{
			transform.position += moveSpeed * Time.unscaledDeltaTime * transform.forward;
		}
		if (Input.GetKey(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindCameraLeftName]))
		{
			transform.position -= moveSpeed * Time.unscaledDeltaTime * transform.right;
		}
		if (Input.GetKey(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindCameraBackwardsName]))
		{
			transform.position -= moveSpeed * Time.unscaledDeltaTime * transform.forward;
		}
		if (Input.GetKey(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindCameraRightName]))
		{
			transform.position += moveSpeed * Time.unscaledDeltaTime * transform.right;
		}
		if (Input.GetKey(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindCameraUpName]))
		{
			transform.position += moveSpeed * Time.unscaledDeltaTime * transform.up;
		}
		if (Input.GetKey(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindCameraDownName]))
		{
			transform.position -= moveSpeed * Time.unscaledDeltaTime * transform.up;
		}
	}
	public void RotateCamera()
	{
		if (Input.GetKey(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindCameraRotateLeftName]))
		{
			transform.eulerAngles -= new Vector3(transform.rotation.x, turnSpeed, transform.rotation.y) * Time.unscaledDeltaTime;
		}
		if (Input.GetKey(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindCameraRotateRightName]))
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
		}
	}

	//function to jump camera to pos based on event notifs
	public void SetNewCameraPosition(Vector3 movePos)
	{
		float offset = gameObject.transform.position.y - 10;

		if (movePos.z < offset + 10)
		{
			gameObject.transform.eulerAngles = new Vector3(0, 180, 0);
			gameObject.transform.position = new Vector3(movePos.x, gameObject.transform.position.y, movePos.z + offset);
		}
		else
		{
			gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
			gameObject.transform.position = new Vector3(movePos.x, gameObject.transform.position.y, movePos.z - offset);
		}
	}
}
