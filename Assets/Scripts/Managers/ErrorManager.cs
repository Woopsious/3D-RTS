using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour
{
	public static ErrorManager Instance;

	private static ILogger logger = Debug.unityLogger;
	private static string kTag = "3D-RTS Launched";

	public GameObject errorMessagePrefab;

	public GameObject errorPopUpObj;
	public Text errorText;

	public void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(Instance);
		}
		else
			Destroy(gameObject);
	}

	public void Start()
	{
		logger.logEnabled = true;
		logger.Log(kTag);
	}
	public void CheckForErrorMessageObj()
	{
		if (errorPopUpObj == null)
			errorPopUpObj = Instantiate(errorMessagePrefab, transform.position, Quaternion.identity);
		errorPopUpObj.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

		errorText = errorPopUpObj.GetComponentInChildren<Text>();
		errorPopUpObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-600, 240);
	}

	public void DisplayErrorMessage(string errorMessage, float displayTimeInSeconds)
	{
		errorText.text = errorMessage;
		errorPopUpObj.SetActive(true);
		StartCoroutine(HideErrorMessage(displayTimeInSeconds));
	}

	public IEnumerator HideErrorMessage(float displayTimeInSeconds)
	{
		yield return new WaitForSeconds(displayTimeInSeconds);
		errorPopUpObj.SetActive(false);
		errorText.text = "";
	}
}
