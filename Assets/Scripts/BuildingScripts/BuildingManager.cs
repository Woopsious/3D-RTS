using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

public class BuildingManager : MonoBehaviour
{
	public PlayerController playerController;
	public CapturePointController capturePointController;

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
	public GameObject buildingDeathObj;
	public AudioSource buildingIdleSound;
	public GameObject miniMapRenderObj;
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

		//FoVMeshObj.SetActive(true);
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
		Debug.Log("building dmg");
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
			RemoveBuildingRefs();
			if (isRefineryBuilding)
				gameObject.GetComponent<RefineryController>().DeleteCargoShipsOnDeath();

			Instantiate(buildingDeathObj, transform.position, Quaternion.identity);

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
	public void ShowBuilding()
	{
		miniMapRenderObj.layer = 13;
	}
	//track buildings refs
	public void AddBuildingRefs()
	{
		if (isGeneratorBuilding)
		{
			capturePointController.energyGeneratorBuilding = this;
		}
		else if (isRefineryBuilding)
		{
			if (!capturePointController.RefinaryBuildings.Contains(this))
				capturePointController.RefinaryBuildings.Add(this);
		}
		else if (isLightVehProdBuilding)
		{
			if (!capturePointController.lightVehProdBuildings.Contains(this))
				capturePointController.lightVehProdBuildings.Add(this);
		}
		else if (isHeavyVehProdBuilding)
		{
			if (!capturePointController.heavyVehProdBuildings.Contains(this))
				capturePointController.heavyVehProdBuildings.Add(this);
		}
	}
	public void RemoveBuildingRefs()
	{
		if (isGeneratorBuilding)
		{
			capturePointController.energyGeneratorBuilding = null;
			GetComponent<EnergyGenController>().UnpowerBuildings();
		}
		else if (isRefineryBuilding)
		{
			capturePointController.RefinaryBuildings.Remove(this);
		}
		else if (isLightVehProdBuilding)
		{
			capturePointController.lightVehProdBuildings.Remove(this);
			playerController.lightVehProdBuildingsList.Remove(this);
		}
		else if (isHeavyVehProdBuilding)
		{
			capturePointController.heavyVehProdBuildings.Remove(this);
			playerController.heavyVehProdBuildingsList.Remove(this);
		}
		else if (isVTOLProdBuilding)
		{
			capturePointController.heavyVehProdBuildings.Remove(this);
			playerController.heavyVehProdBuildingsList.Remove(this);
		}
	}
	public void UpdateAudioVolume()
	{
		buildingIdleSound.volume = AudioManager.Instance.gameSFX.volume;
	}
}
