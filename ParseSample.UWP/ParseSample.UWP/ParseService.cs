using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Networking.PushNotifications;
using Parse;

namespace ParseSample.UWP
{
	public static class ParseService
	{
		public static void Initialize()
		{
			ParseClient.Initialize(Constants.APPLICATION_ID, Constants.NETCLIENT_KEY);

			ParsePush.PushNotificationReceived += ParsePushOnPushNotificationReceived;
		}

		private static void ParsePushOnPushNotificationReceived(object sender, PushNotificationReceivedEventArgs args)
		{
			Debug.WriteLine("Notification received !");
		}

		public static async Task OnLaunchedAsync(ILaunchActivatedEventArgs args)
		{
			await ParseInstallation.CurrentInstallation.SaveAsync();

			await ParseAnalytics.TrackAppOpenedAsync(args);
		}
	}
}