using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataItem : MonoBehaviour
{
	public string playerUsername;
	public string playerId;
	public string localPlayerNetworkedId;

	public bool isThisPlayerHost;

	public Text playerNameUi;
	public Text playerHostUi;

	public GameObject kickPlayerButton;

	public void Initialize(string playerUsername, string playerId, string localPlayerNetworkedId)
	{
		this.playerUsername = playerUsername;
		this.playerId = playerId;
		this.localPlayerNetworkedId = localPlayerNetworkedId;
		playerNameUi.text = playerUsername;
	}
	public void UpdateInfo(string playerId, string playerUsername, string localPlayerNetworkedId)
	{
		if (this.playerUsername != playerUsername)
			this.playerUsername = playerUsername;

		if (this.playerId != playerId)
			this.playerId = playerId;

		if (this.localPlayerNetworkedId != localPlayerNetworkedId)
			this.localPlayerNetworkedId = localPlayerNetworkedId;
	}

	public void KickPlayerFromLobby()
	{
		HostManager.Instance.networkIdOfKickedPlayer = localPlayerNetworkedId;
		HostManager.Instance.RemoveClientFromRelay(localPlayerNetworkedId);
	}
}
