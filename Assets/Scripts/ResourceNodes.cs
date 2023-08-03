using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ResourceNodes : NetworkBehaviour
{
	public NetworkVariable<bool> isCrystalNode = new NetworkVariable<bool>();
	public NetworkVariable<int> resourcesAmount = new NetworkVariable<int>();

	public NetworkVariable<bool> isBeingMined = new NetworkVariable<bool>();
	public NetworkVariable<bool> isEmpty = new NetworkVariable<bool>();

	[ServerRpc(RequireOwnership = false)]
	public void CheckResourceCountServerRpc()
	{
		if (resourcesAmount.Value <= 0)
		{
			isEmpty.Value = true;
			isBeingMined.Value = false;
		}
	}
}
