using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NotifsEventManager : MonoBehaviour, IPointerClickHandler
{
	public Button button;
	public Text text;

	public UnityEvent leftClick;
	public UnityEvent rightClick;

	public Vector3 eventWorldPos;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
			leftClick.Invoke();
		else if (eventData.button == PointerEventData.InputButton.Right)
			rightClick.Invoke();
	}
	public void OnLeftClick()
	{
		GameObject cameraObj = CameraController.instance.gameObject;
		cameraObj.transform.position = new Vector3(eventWorldPos.x, cameraObj.transform.position.y, eventWorldPos.z);
		Destroy(gameObject);
	}
	public void OnRightClick()
	{
		Destroy(gameObject);
	}
}
