using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour
{
	public static ErrorManager Instance;

	private static ILogger logger = Debug.unityLogger;
	private static string kTag = "3D-RTS Launched";

	public GameObject fakeNullException;

	public GameObject errorWindowPrefab;
	public GameObject errorLogMessagePrefab;
	public GameObject errorLogWindowAndPlayerNotifParent;

	public GameObject errorLogWindowObj;
	public Button errorLogClearButton;
	public Button errorLogCloseButton;
	public Scrollbar errorLogScrollBar;

	public Text logText;

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
	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.BackQuote))
		{
			if (errorLogWindowObj.activeInHierarchy)
				CloseErrorLog();
			else
				ShowErrorLog();
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Debug.Log("This is a test log");
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Debug.LogError("This is a test error log");
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				GameManager.Instance.errorManager.MakeFakeNullException();
			}
		}
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
		playerNotifObj = errorLogWindowAndPlayerNotifParent.transform.GetChild(4).gameObject; //set up player pop up notifs
		playerNotifText = playerNotifObj.GetComponentInChildren<Text>();
	}	
	public void SetUpErrorLogNotification()
	{
		errorLogWindowObj = errorLogWindowAndPlayerNotifParent.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
		errorLogClearButton = errorLogWindowAndPlayerNotifParent.transform.GetChild(1).GetComponent<Button>();
		errorLogCloseButton = errorLogWindowAndPlayerNotifParent.transform.GetChild(2).GetComponent<Button>();
		errorLogScrollBar = errorLogWindowAndPlayerNotifParent.transform.GetChild(3).GetComponent<Scrollbar>();

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
	void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}
	public void HandleLog(string logString, string stackTrace, LogType type)
	{
		string log = stackTrace + "\n" + logString + logString + logString + logString;

		if (type == LogType.Error)
		{
			RecordLogMessage(log, Color.red);
			ShowErrorLog();
		}
		else if (type == LogType.Warning)
		{
			RecordLogMessage(log, Color.yellow);
		}
	}
	void RecordLogMessage(string logString, Color color)
	{
		GameObject obj = Instantiate(errorLogMessagePrefab, errorLogWindowObj.transform);
		Text text = obj.GetComponentInChildren<Text>();

		text.color = color;
		text.text = logString;

		errorLogScrollBar.size = 0.1f;
	}
	public void MakeFakeNullException()
	{
		fakeNullException.transform.position = this.transform.position;
	}
	//button functions
	void ClearErrorLog()
	{
		for (int i = errorLogWindowObj.transform.childCount - 1; i >= 0; i--)
			Destroy(errorLogWindowObj.transform.GetChild(i).gameObject);
	}
	public void ShowErrorLog()
	{
		errorLogWindowObj.SetActive(true);
		errorLogClearButton.gameObject.SetActive(true);
		errorLogCloseButton.gameObject.SetActive(true);
		errorLogScrollBar.gameObject.SetActive(true);
	}
	void CloseErrorLog()
	{
		errorLogWindowObj.SetActive(false);
		errorLogClearButton.gameObject.SetActive(false);
		errorLogCloseButton.gameObject.SetActive(false);
		errorLogScrollBar.gameObject.SetActive(false);
	}
}
