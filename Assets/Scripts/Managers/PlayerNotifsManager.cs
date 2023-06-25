using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNotifsManager : MonoBehaviour
{
	[Header("Prefabs")]
	public GameObject notifsWindowPrefab;
	public GameObject notifsEventMessagePrefab;

	[Header("Scene Change Refs")]
	public GameObject playerNotifsWindowParent;
	public GameObject playerMessageNotifsWindowObj;
	public Text playerMessageNotifsText;

	public GameObject playerEventNotifsWindowObj;
	public Scrollbar playerEventNotifsScrollBar;

	public void CheckForPlayerNotifsObj()
	{
		if (playerNotifsWindowParent == null)
		{
			playerNotifsWindowParent = Instantiate(notifsWindowPrefab, transform.position, Quaternion.identity);
			playerNotifsWindowParent.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
			playerNotifsWindowParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -238.7457f);

			SetUpPlayerNotifis();
		}
	}
	public void SetUpPlayerNotifis()
	{
		playerMessageNotifsWindowObj = playerNotifsWindowParent.transform.GetChild(0).gameObject;
		playerMessageNotifsText = playerMessageNotifsWindowObj.GetComponentInChildren<Text>();

		playerEventNotifsWindowObj = playerNotifsWindowParent.transform.GetChild(1).gameObject;
		playerEventNotifsScrollBar = playerNotifsWindowParent.transform.GetChild(2).GetComponent<Scrollbar>();
	}

	//FUNCTIONS FOR PLAYER NOTIFICATIONS
	public void DisplayNotifisMessage(string message, float displayTimeInSeconds)
	{
		playerMessageNotifsText.text = message;
		playerMessageNotifsWindowObj.SetActive(true);
		StartCoroutine(HidePopUpMessage(displayTimeInSeconds));
	}
	public IEnumerator HidePopUpMessage(float displayTimeInSeconds)
	{
		yield return new WaitForSeconds(displayTimeInSeconds);
		playerMessageNotifsWindowObj.SetActive(false);
		playerMessageNotifsText.text = "";
	}
	public void DisplayEventMessage(string message, Vector3 eventWorldPos)
	{
		GameObject go = Instantiate(notifsEventMessagePrefab, playerEventNotifsWindowObj.transform.GetChild(0).gameObject.transform);
		NotifsEventManager eventManager = go.GetComponent<NotifsEventManager>();
		eventManager.text.text = message;
		eventManager.eventWorldPos = eventWorldPos;
	}
}
