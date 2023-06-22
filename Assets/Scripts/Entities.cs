using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Entities : MonoBehaviour
{
	[Header("Entity Refs")]
	public PlayerController playerController;

	public GameObject UiObj;
	public UnityEngine.UI.Slider HealthSlider;
	public Text HealthText;

	public GameObject CenterPoint;
	public GameObject unitDeathObj;
	public GameObject selectedHighlighter;
	public GameObject miniMapRenderObj;

	[Header("Entity Stats")]
	public int moneyCost;
	public int alloyCost;
	public int crystalCost;
	public int maxHealth;
	public int currentHealth;
	public int armour;

	[Header("Entity Bools")]
	public bool isSelected;
	public bool isSpotted;
}
