using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour
{
	public RectTransform tipWindow;
	public Text tipText;

	public static Action<string, Vector2> OnMouseHover;
	public static Action OnMouseLoseFocus;

	private void OnEnable()
	{
		OnMouseHover += ShowTip;
		OnMouseLoseFocus += HideTip;
	}

	private void OnDisable()
	{
		OnMouseHover -= ShowTip;
		OnMouseLoseFocus -= HideTip;
	}

	private void Start()
	{
		HideTip();
	}

	private void ShowTip(string tip, Vector2 mousePos)
	{
		tipText.text = tip;

		tipWindow.gameObject.SetActive(true);
		tipWindow.transform.position = new Vector2(mousePos.x, mousePos.y + 150);
	}

	private void HideTip()
	{
		tipText.text = null;
		tipWindow.gameObject.SetActive(false);
	}
}
