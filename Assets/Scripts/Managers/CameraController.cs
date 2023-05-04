using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	private readonly float moveSpeed = 50f;
	private readonly float turnSpeed = 100f;

	public void Update()
	{
		MoveCamera();
		RotateCamera();
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
}
