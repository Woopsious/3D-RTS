using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNotifsManager : MonoBehaviour
{
	[Header("Prefabs")]
	public GameObject notifsWindowPrefab;
	public GameObject notifsLogMessagePrefab;

	[Header("Scene Change Refs")]
	public GameObject playerNotifsWindowParent;
	public GameObject playerNotifsWindowObj;
	public Scrollbar errorLogScrollBar;

	public void CheckForPlayerNotifsObj()
	{
		if (playerNotifsWindowParent == null)
			playerNotifsWindowParent = Instantiate(notifsWindowPrefab, transform.position, Quaternion.identity);

		playerNotifsWindowParent.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
		playerNotifsWindowParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -238.7457f);

		SetUpPlayerNotification();
	}
	public void SetUpPlayerNotification()
	{
		playerNotifsWindowObj = playerNotifsWindowParent.transform.GetChild(0).gameObject;
		errorLogScrollBar = playerNotifsWindowParent.transform.GetChild(1).GetComponent<Scrollbar>();
	}

	//FUNCTIONS FOR PLAYER NOTIFICATIONS
	public void DisplayNotificationMessage(string errorMessage, float displayTimeInSeconds)
	{
		//playerNotifText.text = errorMessage;
		//playerNotifObj.SetActive(true);
		StartCoroutine(HidePopUpMessage(displayTimeInSeconds));
	}
	public IEnumerator HidePopUpMessage(float displayTimeInSeconds)
	{
		yield return new WaitForSeconds(displayTimeInSeconds);
		//playerNotifObj.SetActive(false);
		//playerNotifText.text = "";
	}
}
