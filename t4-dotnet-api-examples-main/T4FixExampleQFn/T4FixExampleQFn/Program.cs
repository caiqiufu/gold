using QuickFix;
using QuickFix.Fields;
using QuickFix.Transport;
using System.Diagnostics;
using System.Security;
using System.Security.Authentication;
using T4FixExampleQFn;
using T4FixExampleQFn.QuickFix;
using T4FixExampleQFn.STunnel;


namespace FixInitiator
{
    internal class Program
    {

        static T4QuickFixApp application = null;

        static SessionID sessionID = null;

        static Session session = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main()
        {

            // Subscribe to the AppDomain.ProcessExit event
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExitHandler);

            // Initialize the startup parameters via user input.
            var startupParams = new StartupParams();
            
            // Or, hardcode values for simplicity.
            // var startupParams = new StartupParams("","","","","CME_Eq","ES", "XCME_Eq ES(U24)",CommandType.MarketDataIncremental);

            try
            {



                // Attempt to kill any existing stunnel processes.  This is fine whether using sTunnel or not.
                foreach (var process in Process.GetProcessesByName("stunnel"))
                {
                    process.Kill();
                    process.WaitForExit();
                }

                if (startupParams.UseStunnel)
                {

                    // Create the sTunnel configuration file.
                   STunnelHelper.WriteStunnelConf(Path.Combine(AppContext.BaseDirectory,"STunnel"), "local.conf");

                    // Create quickfix sTunnel compatible initiator file.
                    QuickFixHelper.WriteStunnelInitiator(Path.Combine(AppContext.BaseDirectory, "QuickFIX"), "initiator.cfg");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = Path.Combine(AppContext.BaseDirectory,"STunnel", "local.conf"),
                        Arguments = Path.Combine (AppContext.BaseDirectory, "STunnel", "local.conf"),
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process.Start(psi);

                    Console.WriteLine("sTunnel Started Waiting 3 Seconds");
                    Thread.Sleep(3000);

                }
                else
                {

                    // Create quickfix ssl socket compatible initiator file.
                    QuickFixHelper .WriteSSLSocketInitiator (Path.Combine(AppContext.BaseDirectory, "QuickFIX"), "initiator.cfg");

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error stopping / starting sTunnel: " + ex.Message);
            }

            // Parse the startup parameters.
            application = new T4QuickFixApp(startupParams);

            // Create the quickfix initiator file.
            SessionSettings settings = new SessionSettings(startupParams.QuickFixInitiatorConfigFilePath);
            Console.WriteLine("Using Initiator Config: " + startupParams.QuickFixInitiatorConfigFilePath);

            // Create the initiator dependancies.
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);

            // Create the initiator.
            SocketInitiator initiator = new SocketInitiator(application, storeFactory, settings, logFactory);

            // Start the initiator.
            initiator.Start();

            // Start the application.
            application.Start();

            Console.ReadLine();

            // Stop the initiator.
            initiator.Stop();
        }


        // Event handler for ProcessExit event
        static void ProcessExitHandler(object sender, EventArgs e)
        {

            application.Stop();

            // Perform cleanup or graceful shutdown logic here
            Console.WriteLine("Application is closing...");
        }

    

    }


}
