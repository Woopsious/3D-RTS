using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MiniMapManager : MonoBehaviour, IPointerClickHandler
{
	public GameObject miniMapUiObj;

	public bool isMiniMapEnlarged;

	public Vector2 terrainSize = new Vector2(256, 256);
	public Vector2 currentMinimapSize;

	public Vector2 cameraJumpVector;

	public void Start()
	{
		isMiniMapEnlarged = false;
		currentMinimapSize = miniMapUiObj.GetComponent<RectTransform>().sizeDelta;
	}
	public void Update()
	{

	}

	//MINIMAP FUNCTIONS
	public void ChangeAndUpdateMiniMapSize()
	{
		RectTransform rectTransform = miniMapUiObj.GetComponent<RectTransform>();

		if (isMiniMapEnlarged)
		{
			isMiniMapEnlarged = false;
			rectTransform.anchoredPosition = new Vector2(803f, 383f);
			rectTransform.sizeDelta = new Vector2(320, 320);
			currentMinimapSize = new Vector2(320, 320);
		}
		else if (!isMiniMapEnlarged)
		{
			isMiniMapEnlarged = true;
			rectTransform.anchoredPosition = new Vector2(580.5f, 40.5f);
			rectTransform.sizeDelta = new Vector2(770, 770);
			currentMinimapSize = new Vector2(770, 770);
		}
	}
	public void OnPointerClick(PointerEventData pointerEventData)
	{
		ScreenPointToMiniMapSize(pointerEventData);
		/*
		Debug.Log(pointerEventData.position);

		RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapUiObj.GetComponent<RectTransform>(), pointerEventData.position,
		Camera.main, out Vector2 localPoint);

		Debug.Log(localPoint);
		*/
	}
	public void ScreenPointToMiniMapSize(PointerEventData pointerEventData)
	{
		float miniMapSizeX = 310;
		float miniMapSizeY = 310;

		Vector2 size; //get local position of minimap
		size.x = Screen.width - miniMapSizeX;
		size.y = Screen.height - miniMapSizeY;

		Debug.Log("Map Size is: " + size);

		Vector2 newPoint; //get local point
		newPoint = pointerEventData.position - size;

		Debug.Log("point is :" + newPoint);

		//get ratio of minimap to terrain size
		Vector2 ratio;
		ratio.x = miniMapSizeX / terrainSize.x;
		ratio.y = miniMapSizeY / terrainSize.y;
		Debug.Log(miniMapSizeX);
		Debug.Log(miniMapSizeY);
		Debug.Log("Ratio is: " + ratio);

		Vector2 cameraMoveVector;
		cameraMoveVector = newPoint / ratio;

		Debug.Log(cameraMoveVector);
	}
}
