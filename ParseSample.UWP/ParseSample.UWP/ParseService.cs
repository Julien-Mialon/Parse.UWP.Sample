using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Parse;

namespace ParseSample.UWP
{
	public static class ParseService
	{
		public const string APPLICATION_ID = "<App id>";

		public const string NETCLIENT_KEY = "<.net key>";

		public static void Initialize()
		{
			ParseClient.Initialize(APPLICATION_ID, NETCLIENT_KEY);
		}

		public static async Task OnLaunchedAsync(ILaunchActivatedEventArgs args)
		{
			await ParseInstallation.CurrentInstallation.SaveAsync();

			await ParseAnalytics.TrackAppOpenedAsync(args);
		}
	}
}
