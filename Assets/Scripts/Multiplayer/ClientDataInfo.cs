using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct ClientDataInfo : INetworkSerializable, IEquatable<ClientDataInfo>
{
	public FixedString64Bytes clientName;
	public FixedString64Bytes clientId;
	public ulong clientNetworkedId;

	public ClientDataInfo(string playerName = "not set", string clientId = "No Id Token", ulong clientNetworkedId = 0)
	{
		this.clientName = playerName;
		this.clientId = clientId;
		this.clientNetworkedId = clientNetworkedId;
	}
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref clientName);
		serializer.SerializeValue(ref clientId);
		serializer.SerializeValue(ref clientNetworkedId);
	}
	public bool Equals(ClientDataInfo other)
	{
		return clientName == other.clientName && clientId == other.clientId && clientNetworkedId == other.clientNetworkedId;
	}
}
