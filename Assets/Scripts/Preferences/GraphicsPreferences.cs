using Cysharp.Threading.Tasks;


namespace Preferences
{
	public class GraphicsPreferences : PreferenceCategory
	{

		public override void New()
		{
			
		}
		public override UniTask Load()
		{
			return UniTask.CompletedTask;
		}
		public override UniTask Save()
		{
			return UniTask.CompletedTask;
		}
	}
}