using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemManager : MonoBehaviour
{
	public string playerName;
	public string playerId;
	public string localPlayerNetworkedId;

	public bool isThisPlayerHost;

	public Text playerNameUi;
	public Text playerHostUi;

	public GameObject kickPlayerButton;

	public void Initialize(string playerName, string playerId, string localPlayerNetworkedId)
	{
		this.playerName = playerName;
		this.playerId = playerId;
		this.localPlayerNetworkedId = localPlayerNetworkedId;
		playerNameUi.text = playerName;
	}
	public void UpdateInfo(string playerId, string playerName, string localPlayerNetworkedId)
	{
		if (this.playerName != playerName)
			this.playerName = playerName;

		if (this.playerId != playerId)
			this.playerId = playerId;

		if (this.localPlayerNetworkedId != localPlayerNetworkedId)
			this.localPlayerNetworkedId = localPlayerNetworkedId;
	}

	public void KickPlayerFromLobby()
	{
		MultiplayerManager.Instance.idOfKickedPlayer = localPlayerNetworkedId;
		MultiplayerManager.Instance.RemoveClientFromLobby(playerId);
		MultiplayerManager.Instance.RemoveClientFromRelayServerRPC(localPlayerNetworkedId);
	}
}
