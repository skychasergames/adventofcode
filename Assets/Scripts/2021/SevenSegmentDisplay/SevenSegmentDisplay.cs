using UnityEngine;
using UnityEngine.UI;

namespace AoC2021
{
	public class SevenSegmentDisplay : MonoBehaviour
	{
		[SerializeField] private Image[] _segments = null;

		public void ShowDigit(string digitString, Color color)
	    {
		    bool[] states = new bool[7];
		    foreach (char c in digitString)
		    {
			    switch (c)
			    {
			    case 'a': states[0] = true; break;
			    case 'b': states[1] = true; break;
			    case 'c': states[2] = true; break;
			    case 'd': states[3] = true; break;
			    case 'e': states[4] = true; break;
			    case 'f': states[5] = true; break;
			    case 'g': states[6] = true; break;
			    default:
				    Debug.LogError("[SevenSegmentDisplay] Invalid segment string: " + digitString);
				    return;
			    }
		    }
		    
		    for (int i = 0; i < 7; i++)
		    {
			    _segments[i].gameObject.SetActive(states[i]);
			    _segments[i].color = color;
		    }
	    }
	}
}