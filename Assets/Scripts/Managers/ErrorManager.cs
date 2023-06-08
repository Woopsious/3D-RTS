using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour
{
	public static ErrorManager Instance;

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
		OnStartUpHandleLogFiles();
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
			/*
			if (Input.GetKeyUp(KeyCode.F1))
			{
				OnStartUpHandleLogFiles();
			}
			*/
		}
	}
	//SET UP ERROR LOGGER AND PLAYER NOTIFICATIONS
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
	//FUNCTIONS TO HANDLE LOGS AND CONSOLE WINDOW
	//handle all logs that are warnings and errors, writing to both a player.log file and in game console window
	public void HandleLogs(string logString, string stackTrace, LogType type)
	{
		string log = stackTrace + "\n" + logString;

		if (type == LogType.Error || type == LogType.Warning)
		{
			if (CheckForRepeatingLogMessages(log))
			{
				HandleRepeatingLogMessages(type, logString);
			}
			else
			{
				if (type == LogType.Error)
				{
					RecordLogMessageToUi(log, Color.red);
					WriteToLogFile(type, "0", logString);
					ShowErrorLog();
				}
				else if (type == LogType.Warning)
				{
					RecordLogMessageToUi(log, Color.yellow);
					WriteToLogFile(type, "0", logString);
				}
			}
		}
	}
	//in case of repeating log messages update counter if last and prev log message is the same, do this for in game ui and player.log file
	void HandleRepeatingLogMessages(LogType logType, string logString)
	{
		if (lastLogCounterText.text == "10+")
			return;

		else if (lastLogCounterText.text == "10")
		{
			string num = "10+";
			UpdateRepeatedLogMessage(num);
			WriteToLogFile(logType, num, logString);
			return;
		}
		else if (int.TryParse(lastLogCounterText.text, out int num))
		{
			if (num >= 2 && num < 10)
			{
				num++;
				int newNum = num - 1;
				UpdateRepeatedLogMessage(num.ToString());
				WriteToLogFile(logType, newNum.ToString(), logString);
			}
		}
		else
		{
			num = 2;
			UpdateRepeatedLogMessage(num.ToString());
			WriteToLogFile(logType, "1", logString);
		}
	}
	void RecordLogMessageToUi(string logString, Color color)
	{
		GameObject obj = Instantiate(errorLogMessagePrefab, errorLogWindowObj.transform.GetChild(0).gameObject.transform);
		Text counter = obj.transform.GetChild(0).GetComponent<Text>();
		Text text = obj.transform.GetChild(1).GetComponent<Text>();

		text.color = color;
		text.text = logString;
		counter.text = "";

		SaveLastMessage(counter, logString);
	}

	//FUNCTIONS TO HANDLE player.log FILES
	public void OnStartUpHandleLogFiles()
	{
		string playerLogPath = Application.persistentDataPath + "/playerError.log";
		string prevPlayeLogPath = Application.persistentDataPath + "/prevPlayerError.log";
		Debug.Log("handling log files");
		if (File.Exists(playerLogPath))
		{
			if (File.Exists(prevPlayeLogPath)) //delete prevPlayerError.log, rename playerError.log to prevPlayerError.log
				FileUtil.ReplaceFile(playerLogPath, prevPlayeLogPath);

			else
				FileUtil.MoveFileOrDirectory(playerLogPath, prevPlayeLogPath);

			CreateLogFile(playerLogPath);
		}
		else
			CreateLogFile(playerLogPath);
	}
	public void CreateLogFile(string path) //create log file and system info of user
	{
		string pcInfo = "GPU: " + SystemInfo.graphicsDeviceName + " Memory: " + SystemInfo.graphicsMemorySize + "MB\nCPU: " + SystemInfo.processorType + 
			"processor Count: " + SystemInfo.processorCount;

		File.WriteAllText(path, pcInfo);
	}
	public void WriteToLogFile(LogType logType, string counter, string logString)
	{
		string playerLogPath = Application.persistentDataPath + "/playerError.log";

		File.AppendAllText(playerLogPath, "\n\n[BEGINING OF LOG] | LogType: " + logType + " | Times Repeated: " + counter + "\n" +  logString);
	}

	//UTILITY FUNCTIONS
	bool CheckForRepeatingLogMessages(string message)
	{
		if (lastLogMessage == message)
			return true;

		else
			return false;
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
