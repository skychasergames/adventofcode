using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AoC2024
{
	public class ColorBlock : MonoBehaviour
	{
		[SerializeField] private Image _backgroundImage = null;
		[SerializeField] private Image _stripePrefab = null;
		
		public void Initialize(Color backgroundColor, List<Color> stripeColors)
		{
			_backgroundImage.color = backgroundColor;
			
			foreach (Color stripeColor in stripeColors)
			{
				Image stripe = Instantiate(_stripePrefab, transform);
				stripe.color = stripeColor;
			}
		}

		public void SetBackgroundColor(Color backgroundColor)
		{
			_backgroundImage.color = backgroundColor;
		}
	}
}
