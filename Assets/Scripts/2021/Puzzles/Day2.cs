using UnityEngine;

public class Day2 : PuzzleBase
{
	protected override void ExecutePuzzle1()
	{
		int horizontalPos = 0;
	    int depth = 0;
	    
	    foreach (string line in _inputDataLines)
	    {
		    string[] substrings = SplitString(line, " ");
		    if (substrings.Length == 2)
		    {
			    string command = substrings[0];
			    if (int.TryParse(substrings[1], out int distance))
			    {
				    switch (command)
				    {
				    case "forward":
					    horizontalPos += distance;
					    break;
				    
				    case "down":
					    depth += distance;
					    break;
				    
				    case "up":
					    depth -= distance;
					    break;
				    
				    default:
					    LogError("Failed to parse command", command);
					    break;
				    }
				    
			    }
			    else
			    {
				    LogError("Failed to parse distance", substrings[1]);
			    }
		    }
		    else
		    {
			    LogError("Incorrect number substrings", substrings.Length);
		    }
	    }

	    LogResult("Final horizontal pos", horizontalPos);
	    LogResult("Final depth", depth);
	    LogResult("Final product", (horizontalPos * depth));
    }
	
	protected override void ExecutePuzzle2()
	{
		int horizontalPos = 0;
		int aim = 0;
		int depth = 0;
	    
		foreach (string line in _inputDataLines)
		{
			string[] substrings = SplitString(line, " ");
			if (substrings.Length == 2)
			{
				string command = substrings[0];
				if (int.TryParse(substrings[1], out int value))
				{
					switch (command)
					{
					case "forward":
						horizontalPos += value;
						depth += value * aim;
						break;
				    
					case "down":
						aim += value;
						break;
				    
					case "up":
						aim -= value;
						break;
				    
					default:
						LogError("Failed to parse command", command);
						break;
					}
				    
				}
				else
				{
					LogError("Failed to parse distance", substrings[1]);
				}
			}
			else
			{
				LogError("Incorrect number substrings", substrings.Length);
			}
		}

		LogResult("Final horizontal pos", horizontalPos);
		LogResult("Final depth", depth);
		LogResult("Final product", (horizontalPos * depth));
    }
}
