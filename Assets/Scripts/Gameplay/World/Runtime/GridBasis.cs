using UnityEngine;


namespace Gameplay.World.Runtime
{
	public readonly struct GridBasis
	{
		public Vector3 Origin   { get; }
		public Vector3 Right    { get; }
		public Vector3 Forward  { get; }
		public Vector3 Up       { get; }
		public float   CellSize { get; }

		public GridBasis(Vector3 origin, Vector3 right, Vector3 forward, Vector3 up, float cellSize)
		{
			Origin = origin;
			Right = right;
			Forward = forward;
			Up = up;
			CellSize = cellSize;
		}

		public Vector3 GetCellCenter(Vector2Int coordinates)
		{
			Vector3 corner = Origin + (Right * (coordinates.x * CellSize)) + (Forward * (coordinates.y * CellSize));
			return corner + ((Right + Forward) * (CellSize * 0.5f));
		}

		public Quaternion ToWorldRotation(Quaternion localRotation)
		{
			Quaternion gridRotation = Quaternion.LookRotation(Forward, Up);
			return gridRotation * localRotation;
		}
	}
}
