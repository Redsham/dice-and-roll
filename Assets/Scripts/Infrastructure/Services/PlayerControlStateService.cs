namespace Infrastructure.Services
{
	public sealed class PlayerControlStateService
	{
		public bool HasControl { get; private set; }

		public void SetControl(bool hasControl)
		{
			HasControl = hasControl;
		}
	}
}
