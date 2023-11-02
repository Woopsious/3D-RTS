using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ResourceNodes : NetworkBehaviour
{
	public CapturePointController capturePoint;

	public NetworkVariable<bool> isCrystalNode = new NetworkVariable<bool>();
	public NetworkVariable<int> resourcesAmount = new NetworkVariable<int>();
	private int startingResourceAmount;

	public NetworkVariable<bool> isBeingMined = new NetworkVariable<bool>();
	public NetworkVariable<bool> isEmpty = new NetworkVariable<bool>();

	public bool canPOneMine;
	public bool canPTwoMine;

	public GameObject UiObj;
	public GameObject mineResourceButtonObj;
	public GameObject resourceCounterObj;
	public Text resourceCounter;

	public void Start()
	{
		UiObj.transform.SetParent(FindObjectOfType<GameUIManager>().gameObject.transform);
		UiObj.transform.rotation = Quaternion.identity;

		startingResourceAmount = resourcesAmount.Value;
		SyncUiClientRPC(resourcesAmount.Value);
	}
	public void Update()
	{
		if (UiObj != null && UiObj.activeInHierarchy)
			UiObj.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position + new Vector3(0, 5, 0));
	}
	public void MineThisResourceNodeButton()
	{
		GameManager.Instance.gameUIManager.playerController.unitSelectionManager.TryMineResourceNode(gameObject);
	}
	public void ShowResourceCounterUi()
	{
		resourceCounterObj.SetActive(true);
	}
	public void HideResourceCounterUi()
	{
		resourceCounterObj.SetActive(false);
	}
	public void ShowMineResourceButtonUi()
	{
		if (GameManager.Instance.gameUIManager.playerController.isPlayerOne == capturePoint.isPlayerOnePoint)
			mineResourceButtonObj.SetActive(true);
		else if (!GameManager.Instance.gameUIManager.playerController.isPlayerOne == !capturePoint.isPlayerOnePoint)
			mineResourceButtonObj.SetActive(true);
		else
			mineResourceButtonObj.SetActive(false);
	}
	public void HideMineResourceButtonUi()
	{
		mineResourceButtonObj.SetActive(false);
	}

	[ServerRpc(RequireOwnership = false)]
	public void CheckResourceCountServerRpc(int newResourceCount)
	{
		SyncUiClientRPC(newResourceCount);
		if (resourcesAmount.Value <= 0)
		{
			isEmpty.Value = true;
			isBeingMined.Value = false;
		}
	}
	[ClientRpc]
	public void SyncUiClientRPC(int newResourceCount)
	{
		resourceCounter.text = newResourceCount.ToString() + " / " + startingResourceAmount;
	}
	[ServerRpc(RequireOwnership = false)]
	public void IsBeingMinedServerRPC()
	{
		isBeingMined.Value = true;
	}
	[ServerRpc(RequireOwnership = false)]
	public void IsntBeingMinedServerRPC()
	{
		isBeingMined.Value = false;
	}
}
