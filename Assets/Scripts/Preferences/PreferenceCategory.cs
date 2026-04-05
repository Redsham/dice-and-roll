using Cysharp.Threading.Tasks;


namespace Preferences
{
	public abstract class PreferenceCategory
	{
		public abstract void New();
		public abstract UniTask Load();
		public abstract UniTask Save();
	}
}