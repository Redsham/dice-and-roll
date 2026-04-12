using System;


namespace Gameplay.Navigation
{
	[Flags]
	public enum NavCellFlags
	{
		None     = 0,
		Walkable = 1 << 0,
		Hittable = 1 << 1
	}
}
