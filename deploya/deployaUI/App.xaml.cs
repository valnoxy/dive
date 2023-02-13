﻿/* 
 * Dive (formally deploya) - Fast and Easy way to deploy Windows
 * Copyright (c) 2018 - 2022 Exploitox.
 * 
 * Dive is licensed under MIT License (https://github.com/valnoxy/dive/blob/main/LICENSE). 
 * So you are allowed to use freely and modify the application. 
 * I will not be responsible for any outcome. 
 * Proceed with any action at your own risk.
 * 
 * Source code: https://github.com/valnoxy/dive
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommandLine;
using CommandLine.Text;
using System.Reflection;
using System.IO;
using deployaCore;
using deployaCore.Common;

namespace deployaUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
        public static string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #region Parser options
        [Verb("Apply", HelpText = "Add file contents to the index.")]
        class ApplyOptions
        {
            [Option('w', "wim", Required = true, HelpText = "Input WIM-file to be processed.")]
            public string wimfile { get; set; }

            [Option('i', "index", Required = true, HelpText = "Index ID of the selected Windows-Installation.")]
            public int index { get; set; }

            [Option('d', "driveid", Required = true, HelpText = "Hard Drive ID of the destination hard drive.")]
            public int driveid { get; set; }

            [Option('e', "efi", Default = false, HelpText = "Use EFI for installation.")]
            public bool efi { get; set; }

            [Option('n', "ntldr", Default = false, HelpText = "Use NTLDR bootloader for XP and below.")]
            public bool ntldr { get; set; }


            [Usage(ApplicationAlias = "Dive")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>() {
                         new Example("\nApply WIM file on a EFI system", new ApplyOptions { driveid = 0, wimfile = "file.wim", index = 1, efi = true, ntldr = false }),
                         new Example("\nApply WIM file on a Legacy system", new ApplyOptions { driveid = 0, wimfile = "file.wim", index = 1, efi = false, ntldr = false }),
                         new Example("\nApply XP-based image on a Legacy system", new ApplyOptions { driveid = 0, wimfile = "file.wim", index = 1, efi = false, ntldr = true }),
                    };
                }
            }

        }

        [Verb("Capture", HelpText = "Captures a image")]
        class CaptureOptions
        {
            [Option('c', "cap", Required = true, HelpText = "Input WIM-file to be processed.")]
            public string capturedir { get; set; }

            [Option(Default = false, Hidden = true, HelpText = "Used for deploya GUI Installation")]
            public bool uimode { get; set; }

            [Option('i', "index", Required = true, HelpText = "Index ID of the selected Windows-Installation.")]
            public int index { get; set; }

        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false; // Remove the extra newline between options
                h.Heading = $"{VersionInfo.ProductName} [Version: {ver}]"; // Header
                h.Copyright = VersionInfo.LegalCopyright; // Copyright text
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }

        static void ShowGUI()
        {
            MainWindow wnd = new MainWindow();
            wnd.ShowDialog();
            Environment.Exit(0);
        }

        static void ShowAutoDive()
        {
            AutoDive wnd = new AutoDive();
            wnd.ShowDialog();
            Environment.Exit(0);
        }
        #endregion

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            var handle = GetConsoleWindow();

            if (args.Length == 1)
            {
#if DEBUG
                ShowWindow(handle, SW_SHOW);
#else
                ShowWindow(handle, SW_HIDE);
#endif

                Console.Title = $"{VersionInfo.ProductName} - Debug Console";
                Console.WriteLine($"{VersionInfo.ProductName} [Version: {VersionInfo.ProductVersion}]"); // Header
                Console.WriteLine(VersionInfo.LegalCopyright + "\n"); // Copyright text
                Common.Debug.WriteLine("Debug console initialized.", ConsoleColor.White);

                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in allDrives)
                {
                    if (File.Exists(Path.Combine(d.Name, ".diveconfig")))
                    {
#if RELEASE
                        if (!File.Exists("X:\\Windows\\System32\\wpeinit.exe")) ShowGUI();
#endif
                        string message = "Auto deployment config detected. Do you want to perform the deployment now?";
                        string title = "AutoDive";
                        string btn1 = "No";
                        string btn2 = "Yes";

                        var w = new MessageUI(title, message, btn1, btn2, true, 5);
                        if (w.ShowDialog() == false)
                        {
                            string? summary = w.Summary;
                            if (summary == "Btn2")
                            {
                                ShowAutoDive();
                            }
                        }
                    }
                }

                ShowGUI();
            }

            if (args.Contains("--unattend-test"))
            {
                Console.Title = $"{VersionInfo.ProductName} - Debug Console";
                Console.WriteLine($"{VersionInfo.ProductName} [Version: {VersionInfo.ProductVersion}]"); // Header
                Console.WriteLine(VersionInfo.LegalCopyright + "\n"); // Copyright text
                Common.Debug.WriteLine("Debug console initialized.", ConsoleColor.White);
                Common.Debug.WriteLine("Unit Test - Unattend Compiling\n", ConsoleColor.Magenta);

                var config = "";
                Common.UnattendMode? um = Common.UnattendMode.Admin;
                Common.DeploymentInfo.Username = "Administrator";
                Common.DeploymentInfo.Password = "Pa$$w0rd";
                Common.DeploymentOption.UseCopyProfile = true;
                Common.DeploymentOption.UseSMode = true;
                Common.OemInfo.UseOemInfo = true;
                Common.OemInfo.Model = "Toaster";
                Common.OemInfo.Manufacturer = "Fabrikam";
                Common.OemInfo.SupportHours = "24/7";
                Common.OemInfo.SupportURL = "https://fabrikam.com";
                Common.OemInfo.SupportPhone = "+1 111 11111111";

                Common.Debug.WriteLine("Unattend Mode: Admin");
                Common.Debug.WriteLine("Username: " + Common.DeploymentInfo.Username);
                Common.Debug.WriteLine("Password: " + Common.DeploymentInfo.Password);
                Common.Debug.WriteLine("Use S Mode: " + Common.DeploymentOption.UseSMode);
                Common.Debug.WriteLine("Use Copy Path: " + Common.DeploymentOption.UseCopyProfile);
                Common.Debug.WriteLine("Use OEM: " + Common.OemInfo.UseOemInfo);
                Common.Debug.WriteLine("Manufacturer: " + Common.OemInfo.Manufacturer);
                Common.Debug.WriteLine("Model: " + Common.OemInfo.Model);
                Common.Debug.WriteLine("Support Tel.: " + Common.OemInfo.SupportPhone);
                Common.Debug.WriteLine("Support Hours: " + Common.OemInfo.SupportHours);
                Common.Debug.WriteLine("Support URL: " + Common.OemInfo.SupportURL);

                Common.Debug.WriteLine("Building unattend configuration ...", ConsoleColor.DarkYellow);
                config = Common.UnattendBuilder.Build(um);
                Console.WriteLine(config);

                Environment.Exit(0);
            }

            var parser = new CommandLine.Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<ApplyOptions, CaptureOptions>(args);
            parserResult
                .WithParsed<ApplyOptions>(options => Run(options))
                .WithParsed<CaptureOptions>(options => RunA(options))
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            Environment.Exit(0);
        }

        private static void Run(ApplyOptions options)
        {
            throw new NotImplementedException("The CLI is not working in this version.");
#warning CLI version is not working.

            Entities.Firmware firmware = new Entities.Firmware();
            Entities.Bootloader bootloader = new Entities.Bootloader();
            Entities.UI ui = new Entities.UI();

            // Firmware definition
            if (options.efi) { firmware = Entities.Firmware.EFI; }
            if (!options.efi) { firmware = Entities.Firmware.BIOS; }

            // Bootloader definition
            if (options.ntldr) { bootloader = Entities.Bootloader.NTLDR; }
            if (!options.ntldr) { bootloader = Entities.Bootloader.BOOTMGR; }

            // UI definition
            ui = Entities.UI.Command;

            // CLI verify
            string image = options.wimfile.ToString();
            string Index = options.index.ToString();
            string diskId = options.driveid.ToString();

            if (diskId.Contains("\\\\.\\PHYSICALDRIVE"))
                diskId = new string(Enumerable.ToArray<char>(Enumerable.Where<char>((IEnumerable<char>)diskId, new Func<char, bool>(char.IsDigit))));

#region Check options

#region WIM-File
            Console.ForegroundColor = ConsoleColor.Magenta;
            if (!File.Exists(image))
            {
                Console.WriteLine("[i] Image not exist.");
                Console.ForegroundColor = (ConsoleColor)15;
                Environment.Exit(1);
            }
            Console.WriteLine("[i] Image     = " + image);
#endregion

#region Target
            if (App.GetDiskIndex(diskId) > 0U)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Target not exist. ID: " + App.GetDiskIndex(diskId).ToString());
                Console.ResetColor();
                Environment.Exit(1);
            }
            Console.WriteLine("[i] Target    = disk" + diskId);
#endregion

#region BIOS type & Bootloader

            if (options.efi && options.ntldr)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] You cannot use EFI with a legacy bootloader. Aborting ...");
                Console.ResetColor();
                Environment.Exit(1);
            }

            if (options.efi)
            {
                Console.WriteLine("[i] Firmware  = EFI");
                Console.WriteLine("[i] Legacy    = false");
            }
            else if (options.ntldr)
            {
                Console.WriteLine("[i] Firmware  = BIOS");
                Console.WriteLine("[i] Legacy    = true");
            }
            else
            {
                Console.WriteLine("[i] Firmware  = BIOS");
                Console.WriteLine("[i] Legacy    = false");
            }

#endregion

#endregion

            //Actions.PrepareDisk(firmware, bootloader, ui, options.driveid, true);
            Actions.ApplyWIM(ui, "W:\\", options.wimfile, options.index);

            if (bootloader == Entities.Bootloader.BOOTMGR)
                Actions.InstallBootloader(firmware, bootloader, ui, "W:\\", "S:\\");

            if (bootloader == Entities.Bootloader.NTLDR)
                Actions.InstallBootloader(firmware, bootloader, ui, "W:\\", "W:\\");
        }

        private static void RunA(CaptureOptions options)
        {
            throw new NotImplementedException("The CLI is not working in this version.");
#warning CLI version is not working.
        }

        #region Get Disk index
        public static int GetDiskIndex(string diskId)
        {
            // string tempPath = Path.GetTempPath();
            // File.WriteAllText(tempPath + "getdiskindex.cmd", "@wmic diskdrive get index | more +1");
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c \"@wmic diskdrive get index | more +1\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            string end = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // try { File.Delete(Path.Combine(tempPath, "getdiskindex.cmd")); } catch { }
            return end.Contains(diskId) ? 0 : -1;
        }
#endregion
    }
}
