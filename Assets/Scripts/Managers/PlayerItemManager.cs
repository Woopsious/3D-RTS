using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemManager : MonoBehaviour
{
	public string playerId;
	public string playerName;

	public Text playerNameUi;

	public GameObject kickPlayerButton;

	public void Initialize(string playerId, string playerName)
	{
		this.playerId = playerId;
		this.playerName = playerName;
		playerNameUi.text = playerName;
	}

	public void KickPlayerFromLobby()
	{

	}
}
