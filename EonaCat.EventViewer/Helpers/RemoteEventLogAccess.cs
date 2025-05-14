using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace EonaCat.EventViewer.Helpers
{
    public static class RemoteEventLogAccess
    {
        /// <summary>
        /// Retrieves event logs from a remote computer.
        /// </summary>
        /// <param name="remoteComputer">The remote computer name or IP address.</param>
        /// <param name="username">The username to authenticate on the remote computer.</param>
        /// <param name="password">The password for the given username.</param>
        /// <returns>A list of ManagementObject representing the event logs.</returns>
        public static List<ManagementObject> GetRemoteEventLogs(string remoteComputer, string username, string password)
        {
            List<ManagementObject> logs = new List<ManagementObject>();

            try
            {
                // Set up the connection options with the provided username and password
                var options = new ConnectionOptions
                {
                    Username = username,
                    Password = password
                };

                // Define the WMI namespace for event logs (cimv2)
                var scope = new ManagementScope($@"\\{remoteComputer}\root\cimv2", options);

                // Connect to the management scope (remote machine)
                scope.Connect();

                // WMI query to get event logs (e.g., Application event log)
                string query = "SELECT * FROM Win32_NTLogEvent WHERE Logfile = 'Application'";  // Modify 'Application' if needed

                // Create the ManagementObjectSearcher with the query
                var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));

                // Execute the query and get the result
                foreach (var queryObj in searcher.Get())
                {
                    logs.Add((ManagementObject)queryObj);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return logs;
        }
    }
}
