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
		SaveScreenResolution();

		FindObjectOfType<CanvasScaler>().referenceResolution = new Vector2(screenWidthResolution, screenHeightResolution);
	}

	public void SaveScreenResolution()
	{
		GameManager.Instance.LocalCopyOfPlayerData.screenWidthResolution = Instance.screenWidthResolution;
		GameManager.Instance.LocalCopyOfPlayerData.screenHeightResolution = Instance.screenHeightResolution;
	}
	public void LoadScreenResolution()
	{
		Instance.screenWidthResolution = GameManager.Instance.LocalCopyOfPlayerData.screenWidthResolution;
		Instance.screenHeightResolution = GameManager.Instance.LocalCopyOfPlayerData.screenHeightResolution;
	}
	public void ResetScreenResolutionLocally()
	{
		SetNewResolution(1920, 1080);
	}
}
