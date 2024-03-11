using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace GetUsernameValue
{
    class GetUsername()
    {
        // Import the required Win32 API functions
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetUserName(System.Text.StringBuilder lpBuffer, ref int lpLength);

        static void Main(string[] args)
        {
            // Get all drives on the system
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                Console.WriteLine($"Watching drive: {drive.Name}");
                SetupWatcher(drive.Name);
            }

            // Exit the GetUserName Application
            Console.WriteLine("Press 'q' to quit the application.");
            while (Console.ReadKey().Key != ConsoleKey.Q) ;
        }

        static void SetupWatcher(string directoryPath)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(directoryPath);

            watcher.Filter = "*.log";
            watcher.IncludeSubdirectories = true;

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            try
            {
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType} at {DateTime.Now}");

            try
            {
                // Get the Group who changed the file
                string username = GetFileUsername(e.FullPath);
                Console.WriteLine($"User: {username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            try
            {
                // Get the user who changed the file
                string user = Environment.UserName;
                Console.WriteLine($"User: {user} {e.ChangeType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath} at {DateTime.Now}");

            try
            {
                // Get the Group who changed the file
                string username = GetFileUsername(e.FullPath);
                Console.WriteLine($"User: {username}");
            }
            catch (Exception ex )
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            try
            {
                // Get the user who changed the file
                string user = Environment.UserName;
                Console.WriteLine($"User: {user} renamed a file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Function to get the username of the user who changed the file
        private static string GetFileUsername(string filePath)
        {
            // Get the security information of the file
            FileInfo fileInfo = new FileInfo(filePath);
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            // Get the owner of the file
            string owner = fileSecurity.GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();

            // Extract the username
            string[] split = owner.Split('\\');
            string username = split[1];
            return username;
        }
    }
}