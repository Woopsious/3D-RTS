using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Entities : MonoBehaviour
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
	public int moneyCost;
	public int alloyCost;
	public int crystalCost;
	public int maxHealth;
	public int currentHealth;
	public int armour;

	public float spottedCooldown;
	public float spottedTimer;
	public float hitCooldown;
	public float hitTimer;

	[Header("Entity Bools")]
	public bool isPlayerOneEntity;
	public bool wasRecentlyHit;
	public bool wasRecentlySpotted;
	public bool isSelected;
	public bool isSpotted;

	public virtual void Start()
	{
		spottedTimer = spottedCooldown;
		hitTimer = hitCooldown;
		UpdateEntityAudioVolume();

		UiObj.transform.SetParent(FindObjectOfType<GameUIManager>().gameObject.transform);
		HideUIHealthBar();
		UpdateHealthBar();
		UiObj.transform.rotation = Quaternion.identity;

		if (isPlayerOneEntity)
			miniMapRenderObj.layer = 11;
		else if (!isPlayerOneEntity)
			miniMapRenderObj.layer = 12;

		//FoVMeshObj.SetActive(true);
	}

	public virtual void Update()
	{
		if (UiObj.activeInHierarchy)
			UiObj.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position + new Vector3(0, 5, 0));

		IsEntityHitTimer();
		IsEntitySpottedTimer();
	}

	//HEALTH FUNCTIONS
	public void RecieveDamage(int dmg)
	{
		dmg -= armour;
		if (dmg < 0)
			dmg = 0;
		currentHealth -= dmg;
		UpdateHealthBar();
		OnDeath();
	}
	public void UpdateHealthBar()
	{
		float healthPercentage = (float)currentHealth / (float)maxHealth * 100;
		HealthSlider.value = healthPercentage;
		HealthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
	}
	public void OnDeath()
	{
		if (currentHealth <= 0)
		{
			//remove relevent refs, check to make sure it is powered before updating income incase building is destroyed whilst never having been powered
			RemoveEntityRefs();
			Instantiate(DeathObj, transform.position, Quaternion.identity);
			Destroy(UiObj);
			Destroy(gameObject);
		}
	}

	//SPOTTING AND UI FUNCTIONS
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

	//RECENTLY HIT + HEALTH UI FUNCTIONS
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
		ShowUIHealthBar();
		wasRecentlyHit = true;
		hitTimer = hitCooldown;
	}
	public void ShowUIHealthBar()
	{
		UiObj.SetActive(true);
	}
	public void HideUIHealthBar()
	{
		UiObj.SetActive(false);
	}

	//UTILITY FUNCTIONS
	public bool ShouldDisplayEventNotifToPlayer()
	{
		if (playerController.isPlayerOne != isPlayerOneEntity)
			return false;
		else
			return true;
	}
	public virtual void RemoveEntityRefs()
	{

	}
	public void RefundEntity()
	{
		currentHealth = 0;
		int refundMoney = (int)(moneyCost / 1.5);
		int refundAlloy = (int)(alloyCost / 1.5);
		int refundCrystal = (int)(crystalCost / 1.5);

		if (isPlayerOneEntity)
		{
			GameManager.Instance.playerOneCurrentMoney += refundMoney;
			GameManager.Instance.playerOneCurrentAlloys += refundAlloy;
			GameManager.Instance.playerOneCurrentCrystals += refundCrystal;
		}
		else if (!isPlayerOneEntity)
		{
			GameManager.Instance.aiCurrentMoney += refundMoney;
			GameManager.Instance.aiCurrentAlloys += refundAlloy;
			GameManager.Instance.aiCurrentCrystals += refundCrystal;
		}
		playerController.gameUIManager.UpdateCurrentResourcesUI();
		OnDeath();
	}
	public void UpdateEntityAudioVolume()
	{
		if (audioSFXs.Count != 0)
		{
			foreach (AudioSource audio in audioSFXs)
				audio.volume = AudioManager.Instance.gameSFX.volume;
		}
	}
}
