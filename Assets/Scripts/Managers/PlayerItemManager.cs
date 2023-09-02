using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class PlayerItemManager : MonoBehaviour
{
	public string playerId;
	public string playerName;
	public string localPlayerNetworkedId;

	public Text playerNameUi;

	public GameObject kickPlayerButton;

	public void Initialize(string playerId, string playerName, string localPlayerNetworkedId)
	{
		this.playerId = playerId;
		this.playerName = playerName;
		this.localPlayerNetworkedId = localPlayerNetworkedId;
		playerNameUi.text = playerName;
	}

	public void KickPlayerFromLobby()
	{
		MultiplayerManager.Instance.kickPlayerFromLobby(playerId, localPlayerNetworkedId);
	}
}
