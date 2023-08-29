using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEditor;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
	public static MultiplayerManager Instance;

	private int maxConnections = 2;
	private string joinCode;

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

	public void StartHost()
	{
		AuthenticatePlayer();

		CreateRelay();
	}
	public void StartClient()
	{
		AuthenticatePlayer();

		JoinRelay();
	}
	public async void AuthenticatePlayer()
	{
		await UnityServices.InitializeAsync();

		await AuthenticationService.Instance.SignInAnonymouslyAsync();
		AuthenticationService.Instance.SignedIn += () => { Debug.Log($"Player Id{AuthenticationService.Instance.PlayerId}"); };
	}

	public async void CreateRelay()
	{
		Allocation allocation;
		try
		{
			allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
			joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

			NetworkManager.Singleton.StartHost();
		}
		catch (RelayServiceException e)
		{
			Debug.LogError("Failed to allocate relay" + e);
		}
	}
	public async void JoinRelay()
	{
		JoinAllocation joinAllocation;
		try
		{
			Debug.LogWarning($"Joining relay with code: {joinCode}");
			joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

			RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

			NetworkManager.Singleton.StartClient();
		}
		catch (RelayServiceException e)
		{
			Debug.LogError($"Failed to join relay with code: {e}");
		}
	}
}
