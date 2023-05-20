using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour
{
	public static ErrorManager Instance;

	private static ILogger logger = Debug.unityLogger;
	private static string kTag = "3D-RTS Launched";

	public GameObject errorWindowPrefab;
	public GameObject errorLogMessagePrefab;
	public GameObject errorLogWindowAndPlayerNotifParent;

	public GameObject errorLogWindowObj;
	public Button errorLogClearButton;
	public Button errorLogCloseButton;

	public GameObject playerNotifObj;
	public Text playerNotifText;

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
		if (errorLogWindowAndPlayerNotifParent == null)
			errorLogWindowAndPlayerNotifParent = Instantiate(errorWindowPrefab, transform.position, Quaternion.identity);

		errorLogWindowAndPlayerNotifParent.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
		errorLogWindowAndPlayerNotifParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

		SetUpErrorLogNotification();
		SetUpPlayerNotification();
	}
	public void SetUpPlayerNotification()
	{
		playerNotifObj = errorLogWindowAndPlayerNotifParent.transform.GetChild(3).gameObject; //set up player pop up notifs
		playerNotifText = playerNotifObj.GetComponentInChildren<Text>();
	}	
	public void SetUpErrorLogNotification()
	{
		errorLogWindowObj = errorLogWindowAndPlayerNotifParent.transform.GetChild(0).gameObject;
		errorLogClearButton = errorLogWindowAndPlayerNotifParent.transform.GetChild(1).GetComponent<Button>();
		errorLogCloseButton = errorLogWindowAndPlayerNotifParent.transform.GetChild(2).GetComponent<Button>();

		errorLogClearButton.onClick.AddListener(delegate { ClearErrorLog(); });
		errorLogCloseButton.onClick.AddListener(delegate { CloseErrorLog(); });
	}

	//FUNCTIONS FOR PLAYER NOTIFICATIONS
	public void DisplayNotificationMessage(string errorMessage, float displayTimeInSeconds)
	{
		playerNotifText.text = errorMessage;
		playerNotifObj.SetActive(true);
		StartCoroutine(HidePopUpMessage(displayTimeInSeconds));
	}
	public IEnumerator HidePopUpMessage(float displayTimeInSeconds)
	{
		yield return new WaitForSeconds(displayTimeInSeconds);
		playerNotifObj.SetActive(false);
		playerNotifText.text = "";
	}

	//FUNCTIONS FOR ERROR LOG
	public void DisplayErrorLogMessage(string errorMessage)
	{
		ShowErrorLog();

		GameObject obj = Instantiate(errorLogMessagePrefab, errorLogWindowObj.transform);
		Text text = obj.GetComponent<Text>();

		text.text = errorMessage;
	}
	//button functions
	public void ClearErrorLog()
	{
		for (int i = errorLogWindowObj.transform.childCount - 1; i >= 0; i--)
			Destroy(errorLogWindowObj.transform.GetChild(i).gameObject);
	}
	public void ShowErrorLog()
	{
		errorLogWindowObj.SetActive(true);
		errorLogClearButton.gameObject.SetActive(true);
		errorLogCloseButton.gameObject.SetActive(true);
	}
	public void CloseErrorLog()
	{
		errorLogWindowObj.SetActive(false);
		errorLogClearButton.gameObject.SetActive(false);
		errorLogCloseButton.gameObject.SetActive(false);
	}
}
