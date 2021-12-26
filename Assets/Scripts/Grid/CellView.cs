using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellView : MonoBehaviour
{
	[SerializeField] protected Text _valueText = null;
	[SerializeField] protected Image _backgroundImage = null;

	public void Clear()
	{
		SetText("");
		SetBackgroundColor(Color.white);
	}
	
	public void SetText(string text)
	{
		if (_valueText != null)
		{
			_valueText.text = text;
		}
	}

	public void SetBackgroundColor(Color color)
	{
		_backgroundImage.color = color;
	}
}
