using System.Collections.Generic;
using UnityEngine;

namespace AoC2024
{
	public class Day2 : PuzzleBase
	{
		protected override void ExecutePuzzle1()
		{
			List<int[]> reports = GetReports();
			int numSafeReports = 0;
			for (int i = 0; i < reports.Count; i++)
			{
				bool isReportSafe = IsReportSafe(reports[i]);
				if (isReportSafe)
				{
					numSafeReports++;
				}
				
				LogResult("Report " + i, isReportSafe ? "Safe" : "Unsafe");
			}

			LogResult("Total safe reports", numSafeReports);
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		private List<int[]> GetReports()
		{
			List<int[]> reports = new List<int[]>();
			foreach (string line in _inputDataLines)
			{
				reports.Add(ParseIntArray(SplitString(line, " ")));
			}
			return reports;
		}

		private bool IsReportSafe(int[] report)
		{
			// Determine if report levels are increasing or decreasing
			bool isIncreasing = report[0] < report[1];
			
			// Iterate over levels
			for (int i = 0; i < report.Length - 1; i++)
			{
				// Ensure increasing/decreasing trend is followed
				if ((isIncreasing && report[i] >= report[i+1]) ||
				    (!isIncreasing && report[i] <= report[i+1]))
				{
					return false;
				}
				
				// Ensure increase/decrease is within tolerance
				if (Mathf.Abs(report[i] - report[i + 1]) > 3)
				{
					return false;
				}
			}

			return true;
		}
	}
}