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
		//get clicked point on minimap, convert vector2 result to world vector2

		RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapRectTransform, pointerEventData.position, null, out Vector2 localPoint);

		//minimap size delta is 320,320 or 770, 770
		//ratio of terrain size to minimap size is either 1.25 or 3.008
		Vector2 offset = miniMapRectTransform.sizeDelta / 2;

		localPoint += offset;

		Debug.Log("point offset :" + localPoint);

		//get ratio
		Vector2 ratio;
		ratio = miniMapRectTransform.sizeDelta / terrainSize;

		Vector2 cameraJumpVector;
		cameraJumpVector = localPoint / ratio;

		gameUIManager.playerController.mainCameraParent.transform.position = 
			new Vector3(cameraJumpVector.x, Camera.main.transform.position.y, cameraJumpVector.y);


		/*
		Debug.Log(pointerEventData.position);

		RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapUiObj.GetComponent<RectTransform>(), pointerEventData.position,
		Camera.main, out Vector2 localPoint);

		Debug.Log(localPoint);
		*/
	}
	/*
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
	*/
}
