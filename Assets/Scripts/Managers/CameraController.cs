using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
	private readonly float moveSpeed = 50f;
	private readonly float turnSpeed = 100f;

	private float addOffset = -1f;
	private float minusOffset = +1f;

	public void Update()
	{
		MoveCamera();
		RotateCamera();
		CheckDirectionalBounds();
		CheckLookDirection();
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
	public void CheckLookDirection()
	{
		Debug.Log(gameObject.transform.eulerAngles.y);
	}
	public void CheckDirectionalBounds()
	{
		if (CheckIfInBoundsPositive(transform.position.x, 256))
		{
			transform.position = new Vector3(transform.position.x + minusOffset, transform.position.y, transform.position.z);
		}
		if (CheckIfInBoundsPositive(transform.position.z, 256))
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + minusOffset);
		}
		if (CheckIfInBoundsPositive(transform.position.y, 50))
		{
			transform.position = new Vector3(transform.position.x, transform.position.y + minusOffset, transform.position.z);
		}
		if (CheckIfInBoundsNegative(transform.position.x, 0))
		{
			transform.position = new Vector3(transform.position.x + addOffset, transform.position.y, transform.position.z);
		}
		if (CheckIfInBoundsNegative(transform.position.z, 0))
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + addOffset);
		}
		if (CheckIfInBoundsNegative(transform.position.y, 20))
		{
			transform.position = new Vector3(transform.position.x, transform.position.y + addOffset, transform.position.z);
		}
	}
	public bool CheckIfInBoundsPositive(float position, float bounds)
	{
		if (position < bounds)
			return true;
		else
			return false;
	}
	public bool CheckIfInBoundsNegative(float position, float bounds)
	{
		if (position > bounds)
			return true;
		else
			return false;
	}
	public bool CheckDirection(float lookDirection, float minBounds, float MaxBounds)
	{
		if (lookDirection > minBounds && lookDirection < MaxBounds)
			return true;
		else
			return false;
	}
}
