using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ResolutionManager : MonoBehaviour
{
	public static ResolutionManager Instance;

	public int screenWidthResolution;
	public int screenHeightResolution;

	public GameObject resolutionListContainerObj;

	public void Awake()
	{
		Instance = this;
	}

	//change screen resolution
	public void ChangeScreenResolutionButton()
	{
		resolutionListContainerObj.SetActive(true);
	}
	public void SetResolutionOneButton()
	{
		SetNewResolution(1280, 720);
	}
	public void SetResolutionTwoButton()
	{
		SetNewResolution(1920, 1080);
	}
	public void SetResolutionThreeButton()
	{
		SetNewResolution(2560, 1440);
	}

	public void SetNewResolution(int width, int height)
	{
		screenWidthResolution = width; screenHeightResolution = height;
		resolutionListContainerObj.SetActive(false);
		GameManager.Instance.SavePlayerData();

		ChangeScreenToNewResolution(width, height);
	}
	public void ChangeScreenToNewResolution(int width, int height)
	{
		FindObjectOfType<CanvasScaler>().referenceResolution = new Vector2(width, height);
	}

	public void SaveScreenResolution()
	{
		GameManager.Instance.LocalCopyOfPlayerData.screenWidthResolution = screenWidthResolution;
		GameManager.Instance.LocalCopyOfPlayerData.screenHeightResolution = screenHeightResolution;
	}
	public void LoadScreenResolution()
	{
		screenWidthResolution = GameManager.Instance.LocalCopyOfPlayerData.screenWidthResolution;
		screenHeightResolution = GameManager.Instance.LocalCopyOfPlayerData.screenHeightResolution;

		ChangeScreenToNewResolution(screenWidthResolution, screenHeightResolution);
	}
	public void ResetScreenResolutionLocally()
	{
		SetNewResolution(1920, 1080);
	}
}
