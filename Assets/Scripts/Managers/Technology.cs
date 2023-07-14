using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Technology
{
	public string TechName;
	public string TechInfo;
	public float TimeToResearchSec;
	public bool hasPrerequisiteTech;
	public bool isResearched;
	public int numOfTechInColumn;
}
