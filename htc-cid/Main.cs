using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;


namespace htccid
{
	class MainClass
	{
		static Process fastboot;
		static string fastbootOS;
		static string deviceSerial;
		static string deviceCID;

		public static void Main (string[] args)
		{
			initFB();
			centerConsole("**STARTING HTC CID**");
			htccidMode(args);
			creditLine();
		}

		private static void htccidMode(string[] args)
		{
			string mainArg = null;
			try
			{
				 mainArg = args[0];
			}
			catch { }
			switch (mainArg)
			{
				case "unlock":
				{
					centerConsole("*UNLOCK MODE*\n");
					htcCID();
					break;
				}
				default:
				{
					helpOptions();
					break;
				}
			}
		}

		private static void helpOptions()
		{
			Console.WriteLine("Usage:\nhtc-cid.exe <option>\n\nOptions available:\nunlock -\t Begin CID Process" +
				"\nhelp -\t This...");

		}


		private static void countdownDevice()
		{
			for (int i = 10; i >= 0; --i)
		    {
		        int l = Console.CursorLeft;
		        int t = Console.CursorTop;
		        Console.Write("\nWaiting: {0}    ", i);
		        Console.CursorLeft = l;
		        Console.CursorTop = t;
		        Thread.Sleep(1000);
		    }
		}

		private static void centerConsole(string altText)
		{
			Console.WriteLine("{0," + ((Console.WindowWidth/2) + altText.Length/2) + "}", altText);
		}

		private static void htcCID()
		{
			Console.WriteLine("Using '{0}' as Fastboot for this OS!", fastbootOS);
			deviceWork();
			checkSOFF();

			Console.WriteLine("Getting current CID");
			deviceCID = getCID();
			Console.WriteLine("-CID: {0}\n", deviceCID);

			Console.WriteLine("Writing new CID...");
			runFBCommand("oem writecid 11111111");
			Console.WriteLine("Rebooting bootloader...\n");
			runFBCommand("reboot-bootloader");
			countdownDevice();
			Console.WriteLine("Detecting if device is still online...");
			deviceWork();
			Console.WriteLine("Checking CID...");

			if (!isNewCID(getCID()))
			{
				Console.WriteLine("-Unsuccessful CID change! :(");
			}
			else
			{
				Console.WriteLine("-Successful CID change! :D");
			}

		}

		private static bool isNewCID(string CID)
		{
			if (CID == deviceCID)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private static void checkSOFF()
		{
			Console.WriteLine("Checking if device is S-OFF");
			if (runFBCommand("getvar security").Contains("off") == true)
			{
				Console.WriteLine("-Device is S-OFF... good!");
			}
			else
			{
				Console.WriteLine("-Device is not S-OFF! TERMINATING!");
				Environment.Exit(0);
			}
		}

		private static string getCID()
		{
			return Regex.Match(runFBCommand("getvar cid"), "cid: (\\S+)").Groups[1].Value;
		}

		private static void creditLine()
		{
			Console.WriteLine("\n\nThanks for using 'htc-cid' by Simran!\nSpecial thanks to team nocturnal & XDA!");
		}

		private static void deviceWork()
		{
			deviceSerial = Regex.Match(runFBCommand("getvar serialno"), "serialno: (\\S+)").Groups[1].Value;
			if (string.IsNullOrEmpty(deviceSerial))
			{
				Console.WriteLine("NO DEVICE FOUND... TERMINATING!");
				Environment.Exit(0);
			}
			else
			{
				Console.WriteLine("FOUND DEVICE: {0}\n", deviceSerial);
			}
		}

		private static void initFB()
		{
			setFB();
			fastboot = new Process();
		}

		private static ProcessStartInfo newFBInfo()
		{
			return new ProcessStartInfo()
            {
                FileName = fastbootOS,
				RedirectStandardError = true,
				CreateNoWindow = true,
				UseShellExecute = false
            };
		}

		private static void setFB()
		{
			fastbootOS = "fastboot.exe";
			switch(Environment.OSVersion.Platform)
			{
				case PlatformID.Unix:
				{
					fastbootOS = "./fastboot";
					break;
				}
			}
		}

		private static string runFBCommand(string Command)
		{
			string response = null;
			fastboot.StartInfo = newFBInfo();
			fastboot.StartInfo.Arguments = Command;
			fastboot.Start();

			response = fastboot.StandardError.ReadToEnd();	

			fastboot.WaitForExit();

			return response;
		}
	}
}
