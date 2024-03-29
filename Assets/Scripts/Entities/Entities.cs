using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class Entities : NetworkBehaviour
{
	[Header("Entity Refs")]
	public PlayerController playerController;
	public List<AudioSource> audioSFXs = new List<AudioSource>();
	public GameObject UiObj;
	public Slider HealthSlider;
	public Text HealthText;
	public GameObject CenterPoint;
	public GameObject DeathObj;
	public GameObject selectedHighlighter;
	public GameObject miniMapRenderObj;

	[Header("Entity Stats")]
	public string entityName;
	public int moneyCost;
	public int alloyCost;
	public int crystalCost;
	public NetworkVariable<int> maxHealth = new NetworkVariable<int>();
	public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
	public NetworkVariable<int> armour = new NetworkVariable<int>();

	public float spottedCooldown;
	public float spottedTimer;
	public float hitCooldown;
	public float hitTimer;

	public ulong EntityNetworkObjId;

	[Header("Entity Bools")]
	public bool isPlayerOneEntity;
	public bool wasRecentlyHit;
	public bool wasRecentlySpotted;
	public bool isSelected;
	public bool isSpotted;

	public virtual void Start()
	{
		EntityNetworkObjId = GetComponent<NetworkObject>().NetworkObjectId;
		spottedTimer = 0;
		hitTimer = 0;
		UpdateEntityAudioVolume();
		UiObj.transform.SetParent(FindObjectOfType<GameUIManager>().gameObject.transform);
		UiObj.transform.rotation = Quaternion.identity;
		HideUIHealthBar();
	}
	public virtual void Update()
	{
		if (UiObj != null && UiObj.activeInHierarchy)
			UiObj.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position + new Vector3(0, 5, 0));

		IsEntityHitTimer();
		IsEntitySpottedTimer();
	}

	//SPOTTING + UI FUNCTIONS
	public void ShowEntity()
	{
		miniMapRenderObj.layer = 13;
		isSpotted = true;
	}
	public void HideEntity()
	{
		if (isPlayerOneEntity)
			miniMapRenderObj.layer = 11;

		else if (!isPlayerOneEntity)
			miniMapRenderObj.layer = 12;

		isSpotted = false;
	}
	public void IsEntitySpottedTimer()
	{
		if (spottedTimer > 0)
			spottedTimer -= Time.deltaTime;

		else if (wasRecentlySpotted && spottedTimer < 0) //check if player can get notified of a unit being spotted
			wasRecentlySpotted = false;

		if (spottedTimer <= spottedCooldown - 3 && isSpotted) //hide unit on minimap after 3 secs of not being spotted
			HideEntity();
	}
	public void ResetEntitySpottedTimer()
	{
		wasRecentlySpotted = true;
		spottedTimer = spottedCooldown;
	}

	//HEALTH + DEATH/HIT AND UI FUNCTIONS
	//hit and ui
	public void IsEntityHitTimer()
	{
		if (hitTimer > 0)
			hitTimer -= Time.deltaTime;

		else if (wasRecentlyHit && hitTimer < 0)
		{
			wasRecentlyHit = false;
			if (!isSelected)
				HideUIHealthBar();
		}
	}
	public void ResetIsEntityHitTimer()
	{
		TryDisplayEntityHitNotif();
		ShowUIHealthBar();
		wasRecentlyHit = true;
		hitTimer = hitCooldown;
	}
	public virtual void TryDisplayEntityHitNotif()
	{
		GameManager.Instance.playerNotifsManager.DisplayEventMessage("Event Not Set Up", gameObject.transform.position);
	}
	public void ShowUIHealthBar()
	{
		UiObj.SetActive(true);
		UpdateHealthBar();
	}
	public void HideUIHealthBar()
	{
		UiObj.SetActive(false);
	}

	//health and death functions
	[ServerRpc(RequireOwnership = false)]
	public void RecieveDamageServerRPC(float dmg, ServerRpcParams serverRpcParams = default)
	{
		RecieveDamage(dmg);
		UpdateHealthBar();
		RecieveDamageClientRPC(currentHealth.Value, HealthSlider.value);
	}
	[ClientRpc]
	public void RecieveDamageClientRPC(int healthValue, float healthBarPercentage)
	{
		ResetIsEntityHitTimer();
		HealthText.text = healthValue.ToString() + " / " + maxHealth.Value.ToString();
		HealthSlider.value = healthBarPercentage;

		if (healthValue <= 0)
			OnEntityDeath();
	}
	public void RecieveDamage(float dmg)
	{
		dmg -= armour.Value;
		if (dmg < 0)
			dmg = 0;
		currentHealth.Value -= (int)dmg;
	}
	public void UpdateHealthBar()
	{
		float health = currentHealth.Value;
		float healthPercentage = health / maxHealth.Value * 100;
		HealthSlider.value = healthPercentage;
		HealthText.text = health.ToString() + " / " + maxHealth.Value.ToString();
	}
	public virtual void RemoveEntityRefs()
	{

	}
	public virtual void OnEntityDeath()
	{
		if (IsServer)
			GameManager.Instance.RemoveEntityServerRPC(GetComponent<NetworkObject>().NetworkObjectId);
	}

	//UTILITY FUNCTIONS
	public bool IsPlayerControllerNull()
	{
		if (playerController == null)
			return true;
		else
			return false;
	}
	public void RefundEntity()
	{
		GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Refunded 75% of resources", 1f);
		RemoveEntityRefs();
		playerController.gameUIManager.gameManager.UpdateResourcesServerRPC(isPlayerOneEntity, false, true, false, false,
			GetComponent<NetworkObject>().NetworkObjectId, 0, 0, 0);
		playerController.gameUIManager.gameManager.RemoveEntityServerRPC(GetComponent<NetworkObject>().NetworkObjectId);
	}
	public void UpdateEntityAudioVolume()
	{
		if (audioSFXs.Count != 0)
		{
			foreach (AudioSource audio in audioSFXs)
				audio.volume = AudioManager.Instance.gameSFXVolume;
		}
	}
}
