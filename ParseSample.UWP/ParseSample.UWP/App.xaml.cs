using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ParseSample.UWP
{
	sealed partial class App
	{
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

			ParseService.Initialize();
        }
		
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
	        await ParseService.OnLaunchedAsync(e);

            Frame rootFrame = Window.Current.Content as Frame;
			if (rootFrame == null)
            {
                rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    
                }
				
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
