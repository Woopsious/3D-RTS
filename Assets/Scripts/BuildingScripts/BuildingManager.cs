using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BuildingManager : MonoBehaviour
{
	public PlayerController playerController;

	[Header("Building Refs")]
	public bool isPlayerOneBuilding;
	public bool isPowered;
	public bool isSelected;
	public bool isLightVehProdBuilding;
	public bool isHeavyVehProdBuilding;
	public bool isVTOLProdBuilding;
	public bool isRefineryBuilding;
	public bool isGeneratorBuilding;

	public GameObject CenterPoint;
	public AudioSource buildingIdleSound;
	public GameObject selectedHighlighter;
	public GameObject BuildingUiObj;
	public GameObject refundBuildingButton;
	public UnityEngine.UI.Slider HealthSlider;
	public Text HealthText;

	[Header("Building Stats")]
	public int moneyCost;
	public int alloyCost;
	public int crystalCost;
	public int maxHealth;
	public int currentHealth;
	public int armour;

	[Header("Building Production Stats")]
	public int moneyProduction;
	public int alloyProduction;
	public int crystalProduction;
	public int unitBuildTimeBoost;

	public void Start()
	{
		if (buildingIdleSound != null)
		{
			UpdateAudioVolume();
			buildingIdleSound.Play();
		}
		BuildingUiObj.transform.SetParent(FindObjectOfType<GameUIManager>().gameObject.transform);
		BuildingUiObj.SetActive(false);
		UpdateHealthBar();
		BuildingUiObj.transform.rotation = Quaternion.identity;
	}
	public void Update()
	{
		if(isSelected)
		{
			BuildingUiObj.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position + new Vector3(0,10,0));
			if(!BuildingUiObj.activeInHierarchy)
			{
				BuildingUiObj.SetActive(true);
			}
		}
		if (!isSelected && BuildingUiObj.activeInHierarchy)
		{
			StartCoroutine(HideUi());
		}
	}
	public IEnumerator HideUi()
	{
		yield return new WaitForSeconds(0.1f);
		BuildingUiObj.SetActive(false);
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
			gameObject.GetComponent<CanPlaceBuilding>().RemoveBuildingRefs();
			if(isPowered)
				//UpdateProductionIncome(-moneyProduction, -alloyProduction, -crystalProduction);
			if (isRefineryBuilding)
				gameObject.GetComponent<RefineryController>().DeleteCargoShipsOnDeath();

			Destroy(BuildingUiObj);
			Destroy(gameObject);
		}
	}

	//UTILITY FUNCTIONS
	public void RefundBuilding()
	{
		currentHealth = 0;
		int refundMoney = (int)(moneyCost / 1.5);
		int refundAlloy = (int)(alloyCost / 1.5);
		int refundCrystal = (int)(crystalCost / 1.5);

		if (isPlayerOneBuilding)
		{
			GameManager.Instance.playerOneCurrentMoney += refundMoney;
			GameManager.Instance.playerOneCurrentAlloys += refundAlloy;
			GameManager.Instance.playerOneCurrentCrystals += refundCrystal;
		}
		else if (!isPlayerOneBuilding)
		{
			GameManager.Instance.aiCurrentMoney += refundMoney;
			GameManager.Instance.aiCurrentAlloys += refundAlloy;
			GameManager.Instance.aiCurrentCrystals += refundCrystal;
		}
		playerController.gameUIManager.UpdateCurrentResourcesUI();
		OnDeath();
	}
	public void UpdateAudioVolume()
	{
		buildingIdleSound.volume = AudioManager.Instance.gameSFX.volume;
	}
}
