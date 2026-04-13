using System;


namespace Gameplay.Navigation
{
	[Flags]
	public enum NavCellFlags
	{
		None           = 0,
		BlocksMovement = 1 << 0,
		BlocksTrace    = 1 << 1
	}
}
