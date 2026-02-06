using System.Windows;
using System.Windows.Threading;

namespace GamesDat.Demo.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            SentrySdk.Init(o =>
            {
                // Tells which project in Sentry to send events to:
                o.Dsn = "https://e426ffdfc3eff9d187a0f3f180d5379d@o4510804701216768.ingest.de.sentry.io/4510804829929552";
                // When configuring for the first time, to see what the SDK is doing:
                o.Debug = true;
                o.TracesSampleRate = 1.0;
                o.SampleRate = 1.0f;
            });
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);

            // If you want to avoid the application from crashing:
            e.Handled = true;
        }
    }
}
