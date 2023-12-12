using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTips : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public string tipToShow;
	private float timeToWait = 0.5f;
	public void OnPointerEnter(PointerEventData eventData)
	{
		StopAllCoroutines();
		StartCoroutine(StartTimer());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StopAllCoroutines();
		ToolTipManager.OnMouseLoseFocus();
	}

	public virtual void ShowMessage()
	{
		ToolTipManager.OnMouseHover(tipToShow, Input.mousePosition);
	}
	public IEnumerator StartTimer()
	{
		yield return new WaitForSeconds(timeToWait);
		ShowMessage();
	}
}
