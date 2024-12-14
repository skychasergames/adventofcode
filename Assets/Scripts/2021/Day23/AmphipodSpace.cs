using NaughtyAttributes;
using UnityEngine;

namespace AoC2021
{
	public enum AmphipodSpaceType
	{
		Hallway,
		Junction,
		Room,
	}

	public class AmphipodSpace : MonoBehaviour
	{
		[SerializeField] private AmphipodSpace _upAdjacent = null;
		[SerializeField] private AmphipodSpace _downAdjacent = null;
		[SerializeField] private AmphipodSpace _leftAdjacent = null;
		[SerializeField] private AmphipodSpace _rightAdjacent = null;
		[SerializeField] private AmphipodSpaceType _spaceType = AmphipodSpaceType.Hallway;

		[ShowIf("_spaceType", AmphipodSpaceType.Room)]
		[SerializeField] private AmphipodType _roomType = AmphipodType.Amber;

		public AmphipodSpace UpAdjacent => _upAdjacent;
		public AmphipodSpace DownAdjacent => _downAdjacent;
		public AmphipodSpace LeftAdjacent => _leftAdjacent;
		public AmphipodSpace RightAdjacent => _rightAdjacent;
		public AmphipodSpaceType SpaceType => _spaceType;
		public AmphipodType RoomType => _roomType;

		public Amphipod OccupyingAmphipod { get; private set; }
		public bool IsFree { get; private set; }

		public int X => Mathf.RoundToInt(transform.position.x);

		public void Initialize(Amphipod occupyingAmphipod, bool lockSpace)
		{
			OccupyingAmphipod = occupyingAmphipod;
			IsFree = occupyingAmphipod == null && !lockSpace;
		}

		public void ClearSpace()
		{
			OccupyingAmphipod = null;
			IsFree = true;
		}

		public void OccupySpace(Amphipod occupyingAmphipod)
		{
			OccupyingAmphipod = occupyingAmphipod;
			IsFree = false;
		}
	}
}
