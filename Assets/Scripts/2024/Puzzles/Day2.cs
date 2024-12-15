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
			List<int[]> reports = GetReports();
			int numSafeReports = 0;
			for (int i = 0; i < reports.Count; i++)
			{
				bool isReportSafe = IsReportSafeUsingProblemDampener(reports[i]);
				if (isReportSafe)
				{
					numSafeReports++;
				}
				
				LogResult("Report " + i, isReportSafe ? "Safe" : "Unsafe");
			}

			LogResult("Total safe reports", numSafeReports);
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

		private bool IsReportSafe(IList<int> report)
		{
			// Determine if report levels are increasing or decreasing
			bool isIncreasing = report[0] < report[1];
			
			// Iterate over levels
			for (int i = 0; i < report.Count - 1; i++)
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

		private bool IsReportSafeUsingProblemDampener(int[] report)
		{
			if (IsReportSafe(report))
			{
				return true;
			}

			for (int badLevel = 0; badLevel < report.Length; badLevel++)
			{
				List<int> dampenedReport = new List<int>(report);
				dampenedReport.RemoveAt(badLevel);
				if (IsReportSafe(dampenedReport))
				{
					return true;
				}
			}

			return false;
		}
	}
}