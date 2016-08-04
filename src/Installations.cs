using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;

namespace rvm
{
    public static class Installations
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        public static void Create(string version)
        {
            string arch = Environment.Is64BitOperatingSystem ? "x64" : "i386";
            if (Program.Configuration["defaultArch"] != null)
                arch = Program.Configuration["defaultArch"];
            Create(version, arch);
        }

        public static void Create(string version, string arch)
        {
            if (arch != "x64" || arch != "i386")
            {
                Console.Error.WriteLine("INVALID ARGUMENT FORMAT ERROR: If used, arch should be set to x64 for 64-bit or i386 for 32-bit.");
                Environment.Exit((int)Exit.FAILURE_INVALID_ARG_FORMAT);
            }

            string remoteFile = "http://dl.bintray.com/oneclick/rubyinstaller/ruby-" + version + "-" + arch + "-mingw32.7z";
            string localFile = Program.RvmHome + "\\ruby-" + version + "-" + arch + "-mingw32";

            Console.WriteLine("Downloading Ruby version " + version + "...");
            DownloadAndVerify(remoteFile, localFile);
            Unpack(localFile, version, arch);
            LinkInstall(version, arch);
        }

        public static void Remove(string version)
        {
            string arch = Environment.Is64BitOperatingSystem ? "x64" : "i386";
            if (Program.Configuration["defaultArch"] != null)
                arch = Program.Configuration["defaultArch"];
            Remove(version, arch);
        }

        public static void Remove(string version, string arch)
        {
            if (arch != "x64" || arch != "i386")
            {
                Console.Error.WriteLine("INVALID ARGUMENT FORMAT ERROR: If used, arch should be set to x64 for 64-bit or i386 for 32-bit.");
                Environment.Exit((int)Exit.FAILURE_INVALID_ARG_FORMAT);
            }

            string dir = Program.RvmHome + "\\" + version + "-" + arch;
            Directory.Delete(dir, true);
        }

        public static void LinkInstall(string version, string arch)
        {
            if (!IsAdministrator())
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Program.RvmHome + "\\rvm.exe",
                        Arguments = "link " + version + " " + arch,
                        Verb = "runas"
                    }
                };

                process.Start();
                process.WaitForExit();
            }

            else
            {
                // Check if the user has specified their own install location.
                // NOTE: This application will NOT add this location to the PATH automatically.
                string RUBY_HOME = Program.Configuration["RUBY_HOME"] != null ? Program.Configuration["RUBY_HOME"] : "C:\\Ruby";

                // Create the symlink
                CreateSymbolicLink(Program.RvmHome + "\\" + version + "-" + arch, "C:\\Ruby", 0x1);

                // Update the configuration with the current version and architecture
                Program.Configuration["currentVersion"] = version;
                Program.Configuration["currentArch"] = arch;
            }
        }

        public static void List()
        {
            string[] versions = Directory.GetDirectories(Program.RvmHome);
            Console.WriteLine("");
            foreach(var version in versions)
            {
                if(version == (Program.Configuration["currentVersion"] + "-" + Program.Configuration["currentArch"]))
                    Console.WriteLine("   * " + version + " (current)");
                else
                    Console.WriteLine("     " + version);
            }
        }

        private static void DownloadAndVerify(string url, string filename)
        {
            // Create a WebClient for download interfacing
            WebClient client = new WebClient();

            // Set a custom event handler to display a progress bar
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);

            // Download Ruby and the associated MD5 file for verification
            client.DownloadFile(url, filename + ".7z");
            client.DownloadFile(url + ".md5", filename + ".7z.md5");

            // Define 2 byte arrays
            // One will hold the MD5 of the downloaded file
            // The other will hold the MD5 sum to check against
            byte[] fileMD5 = null;
            byte[] checkMD5 = File.ReadAllBytes(filename + ".7z.md5");

            Console.WriteLine("Verifying download...");
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename + ".7z"))
                {
                    fileMD5 = md5.ComputeHash(stream);
                }
            }

            if (fileMD5 != checkMD5)
            {
                Console.Error.WriteLine("FILE DOWNLOAD ERROR: There was an error downloading Ruby...");
                Environment.Exit((int)Exit.FAILURE_DOWNLOAD_FAILURE);
            }
        }

        private static void Unpack(string filename, string version, string arch)
        {
            Console.WriteLine("Unpacking Ruby Version {0} ({1})", version, arch);

            // Create a process to launch 7zr.exe to extract Ruby
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Program.RvmHome + "\\7zr.exe",
                    Arguments = "x " + filename + ".7z",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = false
                }
            };

            process.Start();
            process.WaitForExit();

            FileSystem.RenameDirectory(filename, Program.RvmHome + "\\" + version + "-" + arch);
        }
        
        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            drawTextProgressBar(e.ProgressPercentage, 100);
        }

        /// <summary>
        /// Draw a progress bar at the current cursor position.
        /// Be careful not to Console.WriteLine or anything whilst using this to show progress!
        /// </summary>
        /// <param name="progress">The position of the bar</param>
        /// <param name="total">The amount it counts</param>
        private static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }

        /// <summary>
        /// Check to see if the current user is running under an admin token or a restricted token.
        /// </summary>
        /// <returns>true if user is under admin token, false otherwise</returns>
        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
