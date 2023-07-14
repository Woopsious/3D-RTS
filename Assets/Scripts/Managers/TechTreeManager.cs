using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeManager : MonoBehaviour
{
	public GameObject techTemplate;
	public GameObject techTemplateEmpty;

	public GameObject buildingTechTreeParentObj;
	public GameObject unitTechTreeParentObj;

	[Header("Building Tech Tree Info")]
	public int buildingTechCount;
	public List<Technology> buildingTechList;

	Technology buildingTechHealthOne = new Technology
	{
		TechName = "BuildingTech HealthOne",
		TechInfo = "Increases Health by 5%",
		TimeToResearchSec = 10,
		hasPrerequisiteTech = false,
		isResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology buildingTechArmourOne = new Technology
	{
		TechName = "BuildingTech ArmourOne",
		TechInfo = "Increases Armour by 5%",
		TimeToResearchSec = 10,
		hasPrerequisiteTech = false,
		isResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology buildingTechRefineryBoost = new Technology
	{
		TechName = "BuildingTech RefineryBoost",
		TechInfo = "Increases Resource Income by 10%",
		TimeToResearchSec = 20,
		hasPrerequisiteTech = true,
		isResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology buildingTechHeavyMechs = new Technology
	{
		TechName = "BuildingTech HeavyMechs",
		TechInfo = "Unlocks Heavy Mechs",
		TimeToResearchSec = 20,
		hasPrerequisiteTech = true,
		isResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology buildingTechVTOLS = new Technology
	{
		TechName = "BuildingTech VTOLS",
		TechInfo = "Unlocks Flying VTOL Gunships",
		TimeToResearchSec = 20,
		hasPrerequisiteTech = true,
		isResearched = false,
		hasSpaceBetweenNextTech = 0
	};
	Technology buildingTechHealthTwo = new Technology
	{
		TechName = "BuildingTech HealthTwo",
		TechInfo = "Increases Health by 5%",
		TimeToResearchSec = 15,
		hasPrerequisiteTech = true,
		isResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology buildingTechArmourTwo = new Technology
	{
		TechName = "BuildingTech ArmourTwo",
		TechInfo = "Increases Armour by 5%",
		TimeToResearchSec = 15,
		hasPrerequisiteTech = true,
		isResearched = false,
		hasSpaceBetweenNextTech = 0
	};

	[Header("Unit Tech Tree Info")]
	public int unitTechCount;
	public Dictionary<string, Technology> unitTechList;

	public readonly string unitTechHealthOne = "UnitTech HealthOne";
	public readonly string unitTechAttackRangeOne = "UnitTech RangeOne";
	public readonly string unitTechAttackDamageOne = "UnitTech DamageOne";
	public readonly string unitTechArmourOne = "UnitTech ArmourOne";
	public readonly string unitTechHealthTwo = "UnitTech HealthTwo";
	public readonly string unitTechAttackRangeTwo = "UnitTech RangeOne";
	public readonly string unitTechAttackDamageTwo = "UnitTech DamageTwo";

	//SET UP TECH TREE AND THE UI
	public void Start()
	{
		
	}
	public void SetUpTechTrees()
	{
		SetUpBuildingTechTree();
		SetUpUnitTechTree();
	}
	public void SetUpBuildingTechTree()
	{
		buildingTechList = new List<Technology>
		{
			buildingTechHealthOne,
			buildingTechArmourOne,
			buildingTechRefineryBoost,
			buildingTechHeavyMechs,
			buildingTechVTOLS,
			buildingTechHealthTwo,
			buildingTechArmourTwo
		};

		for (int i = 0; i < buildingTechList.Count; i++)
		{
			GameObject go = Instantiate(techTemplate, buildingTechTreeParentObj.transform);
			SetUpTechTemplate(go, buildingTechList[i]);

			if (buildingTechList[i].hasSpaceBetweenNextTech == 1)
			{
				Instantiate(techTemplateEmpty, buildingTechTreeParentObj.transform);
			}
			if (buildingTechList[i].hasSpaceBetweenNextTech == 2)
			{
				Instantiate(techTemplateEmpty, buildingTechTreeParentObj.transform);
				Instantiate(techTemplateEmpty, buildingTechTreeParentObj.transform);
			}
		}
	}
	public void SetUpUnitTechTree()
	{

	}
	public void SetUpTechTemplate(GameObject obj, Technology tech)
	{
		obj.transform.GetChild(0).GetComponent<Text>().text = tech.TechName;
		obj.transform.GetChild(1).GetComponent<Text>().text = tech.TechInfo;
		obj.transform.GetChild(2).GetComponent<Button>(); //add delegate function for button later
	}

	[System.Serializable]
	public class Technology
	{
		public string TechName;
		public string TechInfo;
		public float TimeToResearchSec;
		public bool hasPrerequisiteTech;
		public bool isResearched;
		public int hasSpaceBetweenNextTech;
	}
}
