using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Day12 : PuzzleBase
{
	protected override void ExecutePuzzle1()
	{
		ExecutePuzzle(false);
	}

	private void ExecutePuzzle(bool canRevisitOneSmallCave)
	{
		// Create cave systems and routes from input data
		CaveSystem caveSystem = new CaveSystem(_inputDataLines, canRevisitOneSmallCave);
		
		// Log results :ez:
		StringBuilder stringBuilder = new StringBuilder().AppendLine(caveSystem.CompletedRoutes.Count.ToString());
		foreach (Route completedRoute in caveSystem.CompletedRoutes)
		{
			stringBuilder.AppendLine(completedRoute.ToString());
		}
		LogResult("Completed routes", stringBuilder.ToString());

		stringBuilder.Clear().AppendLine(caveSystem.KilledRoutes.Count.ToString());
		foreach (Route killedRoute in caveSystem.KilledRoutes)
		{
			stringBuilder.AppendLine(killedRoute.ToString());
		}
		LogResult("Killed routes", stringBuilder.ToString());
	}

	public class CaveSystem
	{
		private readonly List<Cave> _caves = new List<Cave>();
		private Cave _startCave = null;
		private Cave _endCave = null;

		public bool CanRevisitOneSmallCave { get; }

		public List<Route> CalculatingRoutes { get; } = new List<Route>();
		public List<Route> CompletedRoutes { get; } = new List<Route>();
		public List<Route> KilledRoutes { get; } = new List<Route>();

		public CaveSystem(string[] inputDataLines, bool canRevisitOneSmallCave)
		{
			CanRevisitOneSmallCave = canRevisitOneSmallCave;
			
			// Setup cave system
			CreateCaves(inputDataLines);
			CreateConnections(inputDataLines);
			
			// Create the first route
			Route route = new Route();
			SetupRoute(route);
			
			// Start exploring
			route.VisitCave(_startCave);
		}

		private void SetupRoute(Route route)
		{
			route.OnRouteDiverged += OnRouteDiverged;
			route.OnRouteEnded += OnRouteEnded;
			CalculatingRoutes.Add(route);
		}

		private void OnRouteDiverged(Route newRoute)
		{
			SetupRoute(newRoute);
		}

		private void OnRouteEnded(Route route, bool reachedEnd)
		{
			route.OnRouteDiverged -= OnRouteDiverged;
			route.OnRouteEnded -= OnRouteEnded;
			CalculatingRoutes.Remove(route);

			if (reachedEnd)
			{
				CompletedRoutes.Add(route);
			}
			else
			{
				KilledRoutes.Add(route);
			}

			if (CalculatingRoutes.Count == 0)
			{
				Debug.Log("Finished calculating routes!");
			}
		}

		private void CreateCaves(string[] inputDataLines)
		{
			foreach (string line in inputDataLines)
			{
				foreach (string id in SplitString(line, "-"))
				{
					if (_caves.Any(cave => cave.ID == id))
					{
						// Skip if we've already added this cave
						continue;
					}
					
					if (id.Equals("start"))
					{
						_startCave = new StartCave(id, this);
						_caves.Add(_startCave);
					}
					else if (id.Equals("end"))
					{
						_endCave = new EndCave(id, this);
						_caves.Add(_endCave);
					}
					else if (char.IsUpper(id[0]))
					{
						_caves.Add(new LargeCave(id, this));
					}
					else
					{
						_caves.Add(new SmallCave(id, this));
					}
				}
			}
		}

		private void CreateConnections(string[] inputDataLines)
		{
			foreach (string line in inputDataLines)
			{
				string[] caveIds = SplitString(line, "-");
				ConnectCaves(caveIds[0], caveIds[1]);
			}
		}

		private void ConnectCaves(string caveAId, string caveBId)
		{
			Cave caveA = _caves.Find(cave => cave.ID.Equals(caveAId));
			Cave caveB = _caves.Find(cave => cave.ID.Equals(caveBId));
			caveA.AddConnectionToCave(caveB);
			caveB.AddConnectionToCave(caveA);
		}
	}

	public class StartCave : Cave
	{
		public StartCave(string id, CaveSystem caveSystem) : base(id, caveSystem) { }
		
		public override bool CanVisit(Route route)
		{
			return false;
		}
	}

	public class EndCave : Cave
	{
		public EndCave(string id, CaveSystem caveSystem) : base(id, caveSystem) { }
	}

	public class SmallCave : Cave
	{
		protected const int MAX_VISITS = 1;
		protected const int MAX_VISITS_IF_REVISITS_ALLOWED = 2;
		
		public SmallCave(string id, CaveSystem caveSystem) : base(id, caveSystem) { }
		
		public override bool CanVisit(Route route)
		{
			int timesVisitedThisCave = route.VisitedCaves.Count(cave => cave == this);
			
			if (_caveSystem.CanRevisitOneSmallCave)
			{
				// If this cave is below the soft limit, it can be visited
				if (timesVisitedThisCave < MAX_VISITS)
				{
					return true;
				}

				// If this cave is below the hard limit, it can be visited if no other small caves are over the soft limit
				if (timesVisitedThisCave < MAX_VISITS_IF_REVISITS_ALLOWED)
				{
					Cave revisitedSmallCave = route.VisitedCaves
						.Where(cave => cave is SmallCave)
						.FirstOrDefault(smallCave => route.VisitedCaves.Count(cave => cave == smallCave) > MAX_VISITS);

					return revisitedSmallCave == null || revisitedSmallCave == this;
				}
				
				return false;
			}
			
			return timesVisitedThisCave < MAX_VISITS;
		}
	}

	public class LargeCave : Cave
	{
		public LargeCave(string id, CaveSystem caveSystem) : base(id, caveSystem) { }
	}

	public abstract class Cave
	{
		public string ID { get; }
		public List<Cave> ConnectedCaves { get; } = new List<Cave>();

		protected readonly CaveSystem _caveSystem;

		protected Cave(string id, CaveSystem caveSystem)
		{
			ID = id;
			_caveSystem = caveSystem;
		}

		public void AddConnectionToCave(Cave connectedCave)
		{
			ConnectedCaves.Add(connectedCave);
		}

		public virtual bool CanVisit(Route route)
		{
			return true;
		}
	}

	public class Route
	{
		public Queue<Cave> VisitedCaves { get; private set; }
		
		public event Action<Route> OnRouteDiverged = delegate { };
		public event Action<Route, bool> OnRouteEnded = delegate { };

		public Route()
		{
			VisitedCaves = new Queue<Cave>();
		}

		public void VisitCave(Cave caveToVisit)
		{
			VisitedCaves.Enqueue(caveToVisit);

			if (caveToVisit is EndCave)
			{
				// Reached end cave!
				OnRouteEnded(this, true);
				return;
			}

			// Find all possible next caves
			List<Cave> nextCaves = caveToVisit.ConnectedCaves.Where(cave => cave.CanVisit(this)).ToList();
			if (nextCaves.Count == 0)
			{
				// Dead end!
				OnRouteEnded(this, false);
				return;
			}
			
			// Diverge routes if there are multiple possible next caves
			for (int i = 1; i < nextCaves.Count; i++)
			{
				Diverge(nextCaves[i]);
			}
			
			// Move onto the next cave
			VisitCave(nextCaves[0]);
		}
		
		public void Diverge(Cave caveToVisit)
		{
			Route newRoute = new Route { VisitedCaves = new Queue<Cave>(VisitedCaves) };
			
			OnRouteDiverged(newRoute);
			
			newRoute.VisitCave(caveToVisit);
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			foreach (Cave cave in VisitedCaves)
			{
				result.Append(cave.ID);
				result.Append(",");
			}

			return result.ToString();
		}
	}

	protected override void ExecutePuzzle2()
	{
		ExecutePuzzle(true);
	}
}
