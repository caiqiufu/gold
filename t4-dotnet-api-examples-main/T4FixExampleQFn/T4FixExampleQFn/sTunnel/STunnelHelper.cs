using System;
using System.IO;
using System.Text;

namespace T4FixExampleQFn.STunnel

{

public static class STunnelHelper
    {
        public static void WriteStunnelConf(string directoryPath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("directoryPath is required", nameof(directoryPath));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("fileName is required", nameof(fileName));

            // Ensure directory exists
            Directory.CreateDirectory(directoryPath);

            string fullPath = Path.Combine(directoryPath, fileName);

            string content = $"""
                ; **cts** Debug Logging. Maximum messaging of STUNNEL operations
                debug = 7

                ; **cts** Debug Logging. Location and name
                output = "{Path.Combine(directoryPath, "Output.txt")}"

                ; Example SSL client mode services
                [ctsfixapi]

                ; **cts** This is CTS FIX API client application
                client = yes

                ; **cts** Accept incoming connection on this port (from CTS FIX Client)
                accept = 443

                ; **cts** CTS FIX API Order Routing Server connection (to CTS FIX API Server)
                connect = fix-sim.t4login.com:10443
                """;

            File.WriteAllText(fullPath, content, Encoding.UTF8);
        }

    }

}
