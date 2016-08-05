using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace rvm
{
    public enum Exit : int
    {
        SUCCESS,
        FAILURE_INVALID_NUMBER_ARGS,
        FAILURE_INVALID_ARGS,
        FAILURE_DOWNLOAD_FAILURE,
        FAILURE_INVALID_ARG_FORMAT
    }

    public static class Program
    {
        public static IConfiguration Configuration { get; private set; }
        public static string Version { get; private set; }
        public static string RvmHome { get; private set; }

        static void Main(string[] args)
        {
            // Run the setup
            Setup();

            if (args.Count() < 2)
            {
                PrintHelp();
                Environment.Exit(0);
            }

            switch(args[1])
            {
                case "arch":
                    string osArch = Environment.Is64BitOperatingSystem ? "Windows Architecture: 64-bit" : "Windows Architecture: 32-bit";
                    string rubyArch;
                    if (Configuration["currentArch"] != null)
                        rubyArch = (Configuration["currentArch"] == "x64") ? "Ruby Architecture: 64-bit" : "Ruby Architecture: 32-bit";
                    else
                        rubyArch = "Ruby Architecture: NOT INSTALLED";

                    Console.WriteLine("\t" + osArch);
                    Console.WriteLine("\t" + rubyArch);
                    break;
                case "install":
                    if(args.Count() == 3)
                        Installations.Create(args[2]);
                    else if (Environment.Is64BitOperatingSystem && args.Count() == 4)
                        Installations.Create(args[2], args[3]);
                    else if (!Environment.Is64BitOperatingSystem && args.Count() == 4)
                    {
                        Console.Error.WriteLine("INVALID ARGUMENT ERROR: You can only specify an architecture on 64-bit operating systems.");
                        Environment.Exit((int)Exit.FAILURE_INVALID_ARGS);
                    }
                    else
                    {
                        Console.Error.WriteLine("INVALID NUMBER OF ARGUMENTS ERROR: Valid this option takes 1 or 2 arguments.");
                        Environment.Exit((int)Exit.FAILURE_INVALID_NUMBER_ARGS);
                    }
                    break;
                case "list":
                    Installations.List();
                    break;
                case "uninstall":
                    if (args.Count() == 3)
                        Installations.Remove(args[2]);
                    else if (Environment.Is64BitOperatingSystem && args.Count() == 4)
                        Installations.Remove(args[2], args[3]);
                    else if (!Environment.Is64BitOperatingSystem && args.Count() == 4)
                    {
                        Console.Error.WriteLine("INVALID ARGUMENT ERROR: You can only specify an architecture on 64-bit operating systems.");
                        Environment.Exit((int)Exit.FAILURE_INVALID_ARGS);
                    }
                    else
                    {
                        Console.Error.WriteLine("INVALID NUMBER OF ARGUMENTS ERROR: Valid this option takes 1 or 2 arguments.");
                        Environment.Exit((int)Exit.FAILURE_INVALID_NUMBER_ARGS);
                    }
                    break;
                case "use":
                    if (args.Count() == 3)
                    {
                        string arch = Environment.Is64BitOperatingSystem ? "x64" : "i386";
                        if (Configuration["defaultArch"] != null)
                            arch = Program.Configuration["defaultArch"];
                        Installations.LinkInstall(args[2], arch);
                    }
                    else if (Environment.Is64BitOperatingSystem && args.Count() == 4)
                        Installations.LinkInstall(args[2], args[3]);
                    else if (!Environment.Is64BitOperatingSystem && args.Count() == 4)
                    {
                        Console.Error.WriteLine("INVALID ARGUMENT ERROR: You can only specify an architecture on 64-bit operating systems.");
                        Environment.Exit((int)Exit.FAILURE_INVALID_ARGS);
                    }
                    else
                    {
                        Console.Error.WriteLine("INVALID NUMBER OF ARGUMENTS ERROR: Valid this option takes 1 or 2 arguments.");
                        Environment.Exit((int)Exit.FAILURE_INVALID_NUMBER_ARGS);
                    }
                    break;
                case "version":
                case "v":
                    Console.WriteLine(Version);
                    break;
                default:
                    PrintHelp();
                    break;
            }

            // Since we made it out the switch, we'll assume that everything was successful
            Environment.Exit((int)Exit.SUCCESS);
        }

        private static void Setup()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\config.json", true)
                .Build();

            RvmHome = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Ruby Version Manager for Windows, Version " + Version);
            Console.WriteLine("Usage:");
            Console.WriteLine("Please note that anywhere arch is specified, it is optional. It is also unavailable on 32-bit systems.");
            Console.WriteLine("If used, arch should be set to x64 for 64-bit or i386 for 32-bit.");
            Console.WriteLine("");
            Console.WriteLine("rvm arch                           Show what architecture you are currently running");
            Console.WriteLine("rvm install <version> [arch]       Install Ruby version [and architecture] specified.");
            Console.WriteLine("rvm list                           List what Ruby versions are already installed.");
            Console.WriteLine("rvm uninstall <version> [arch]     Uninstall Ruby version [and architecture] specified.");
            Console.WriteLine("rvm use <version> [arch]           Use the Ruby version and architecture specified.");
            Console.WriteLine("rvm version | v                    Display what version of RVM you are using.");
        }
    }
}
