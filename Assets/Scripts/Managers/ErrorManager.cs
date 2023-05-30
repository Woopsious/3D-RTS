using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

	public string lastLogMessage;
	public Text lastLogCounterText;

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
		errorLogWindowObj = errorLogWindowAndPlayerNotifParent.transform.GetChild(0).gameObject;
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
		Application.logMessageReceived += HandleLogs;
	}

	void OnDisable()
	{
		Application.logMessageReceived -= HandleLogs;
	}
	//compare last message string with new message string if they match do the following
	//grab number at start of string which will either not exist yet, or be from 2 to 10
	//if its 10, check for a + after 10, if its 10+ discard rest of code, else
	//if it doesnt exist yet add 2 to start of string
	//if a number between 2 and 9 exist, add + 1 for ever repeated log message
	//if number is 10 make it 10+
	public void HandleLogs(string logString, string stackTrace, LogType type)
	{
		string log = stackTrace + "\n" + logString;

		if (CheckForRepeatingLogMessages(log))
		{
			HandleRepeatingLogMessages();
		}
		else
		{
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
	}
	void RecordLogMessage(string logString, Color color)
	{
		GameObject obj = Instantiate(errorLogMessagePrefab, errorLogWindowObj.transform.GetChild(0).gameObject.transform);
		Text counter = obj.transform.GetChild(0).GetComponent<Text>();
		Text text = obj.transform.GetChild(1).GetComponent<Text>();

		text.color = color;
		text.text = logString;
		counter.text = "";

		errorLogScrollBar.size = 0.1f;
		SaveLastMessage(counter, logString);
	}
	//in case of repeating log messages instead of overflowing the window add to a counter that goes to 10+
	void HandleRepeatingLogMessages()
	{
		if (lastLogCounterText.text.StartsWith("1"))
		{
			Debug.Log("error counter starts with 1");
			string num = "10+";
			UpdateRepeatedLogMessage(num);
			return;
		}
		else if (int.TryParse(lastLogCounterText.text, out int num))
		{
			if (num > 1 && num < 10)
			{
				Debug.Log("error counter starts with a 2-9");
				num++;
				UpdateRepeatedLogMessage(num.ToString());
			}
		}
		else
		{
			Debug.Log("error counter is empty");
			num = 2;
			UpdateRepeatedLogMessage(num.ToString());
		}
	}
	public void MakeFakeNullException()
	{
		try
		{
			fakeNullException.transform.position = this.transform.position;
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}
	}
	//UTILITY FUNCTIONS
	bool CheckForRepeatingLogMessages(string message)
	{
		if (lastLogMessage == message)
		{
			Debug.Log("last and current error log match");
			return true;
		}
		else
		{
			Debug.Log("last and current error log DONT match");
			return false;
		}
	}
	void UpdateRepeatedLogMessage(string num)
	{
		lastLogCounterText.text = num;
	}
	void SaveLastMessage(Text counter, string message)
	{
		lastLogCounterText = counter;
		lastLogMessage = message;
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
