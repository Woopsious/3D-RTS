using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MiniMapManager : MonoBehaviour, IPointerClickHandler
{
	public GameUIManager gameUIManager;
	public GameObject miniMapUiObj;

	private RectTransform miniMapRectTransform;
	private bool isMiniMapEnlarged;

	private Vector2 terrainSize = new Vector2(256, 256);
	private Vector2 currentMinimapSize;

	public void Start()
	{
		miniMapRectTransform = miniMapUiObj.GetComponent<RectTransform>();
		isMiniMapEnlarged = false;
		miniMapRectTransform.anchoredPosition = new Vector2(803f, 383f);
		miniMapRectTransform.sizeDelta = new Vector2(320, 320);
		currentMinimapSize = new Vector2(320, 320);
		currentMinimapSize = miniMapUiObj.GetComponent<RectTransform>().sizeDelta;
	}

	//MINIMAP FUNCTIONS
	public void ChangeAndUpdateMiniMapSize()
	{
		if (isMiniMapEnlarged)
		{
			isMiniMapEnlarged = false;
			miniMapRectTransform.anchoredPosition = new Vector2(803f, 383f);
			miniMapRectTransform.sizeDelta = new Vector2(320, 320);
			currentMinimapSize = new Vector2(320, 320);
		}
		else if (!isMiniMapEnlarged)
		{
			isMiniMapEnlarged = true;
			miniMapRectTransform.anchoredPosition = new Vector2(580.5f, 40.5f);
			miniMapRectTransform.sizeDelta = new Vector2(770, 770);
			currentMinimapSize = new Vector2(770, 770);
		}
	}
	public void OnPointerClick(PointerEventData pointerEventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapRectTransform, pointerEventData.position, null, out Vector2 localPoint);

		Vector2 offset = miniMapRectTransform.sizeDelta / 2;
		localPoint += offset;

		Vector2 ratio;
		ratio = miniMapRectTransform.sizeDelta / terrainSize;

		Vector2 cameraJumpVector;
		cameraJumpVector = localPoint / ratio;

		gameUIManager.playerController.mainCameraParent.transform.position = 
			new Vector3(cameraJumpVector.x, Camera.main.transform.position.y, cameraJumpVector.y);
	}
}
