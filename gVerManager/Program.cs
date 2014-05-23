using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace gVerManager
{
	class Program
	{
		struct VersionProp
		{
			public int BuildMax;
			public int RevMax;
			public int BuildInc;
			public int RevInc;
			public string FilePath;
		}

		static void Main(string[] args)
		{
			//Major Version
			//Minor Version
			//Build Number
			//Revision

			VersionProp vp = parseCommandLine(args);

			string fileContent = getFileContent(vp.FilePath);
			string asmVer = getAsmVerString(fileContent);
			Version oldVer = parseVersion(asmVer);

			if (oldVer == null)
			{
				Console.WriteLine("Unable to parse AssemblyVersion");
				Environment.Exit(0);
			}
			Version newVer = processVersion(vp, oldVer);
			replaceVersion(vp, oldVer, newVer, ref fileContent);

			Console.WriteLine(oldVer.ToString() + " -> "  + newVer.ToString());

		}
		static VersionProp parseCommandLine(string[] args)
		{
			//-f filepath
			//-bm Build number max
			//-rm Revision number max
			//-ri Revision increment
			//-bi Build increment
			//-h help

			if (args.Count() == 0)
			{
				showHelp();
				Environment.Exit(0);
			}

			VersionProp vp = new VersionProp()
				{
					BuildInc = 0,
					BuildMax = 100,
					RevInc = 1,
					RevMax = 100
				};

			for (int i = 0; i < args.Count(); i++)
			{
				if (args[i] == "-f")
				{
					if (File.Exists(args[i + 1]))
					{
						vp.FilePath = args[i + 1];
						i++;
					}
					else
					{
						Console.WriteLine("File doesn't exist : " + args[i + 1]);
						Environment.Exit(0);
						break;
					}
				}
				else if (args[i] == "-bm" || args[i] == "-rm" || args[i] == "-ri" || args[i] == "-bi")
				{
					int m;
					if (int.TryParse(args[i + 1], out m))
					{
						switch (args[i])
						{
							case "-bm":
								vp.BuildMax = m;
								break;
							case "-rm":
								vp.RevMax = m;
								break;
							case "-ri":
								vp.RevInc = m;
								break;
							case "-bi":
								vp.BuildInc = m;
								break;
						}
						i++;
					}
					else
					{
						Console.WriteLine("Argument \"" + args[i] + " " + args[i + 1] + "\" is invalid.");
						Environment.Exit(0);
					}
				}
				else
				{
					Console.WriteLine("Invalid argument \"" + args[i] + "\"");
					Environment.Exit(0);
				}

			}
			return vp;
		}

		static string getFileContent(string filepath)
		{
			using (StreamReader sr = new StreamReader(filepath))
			{
				return sr.ReadToEnd();
			}
		}

		static string getAsmVerString(string str)
		{
			string pattern = ("\\[assembly:\\ AssemblyVersion\\(\"(\\d+\\.?){4}\"\\)\\]");
			Match match = Regex.Match(str, pattern);
			match = Regex.Match(match.ToString(), "(\\d+\\.?){4}");
			return match.ToString();
		}

		static Version parseVersion(string version)
		{
			Version v;
			if (Version.TryParse(version, out v))
			{
				return v;
			}
			else
			{
				return null;
			}
		}

		static Version processVersion(VersionProp vp, Version v)
		{
			int rev = v.Revision + vp.RevInc;
			int build = v.Build + vp.BuildInc;
			int minor = v.Minor;
			if (rev >= vp.RevMax)
			{
				rev = 0;
				build++;
			}
			if (build >= vp.BuildMax)
			{
				build = 0;
				minor++;
			}

			return new Version(v.Major, minor, build, rev);
		}

		static void replaceVersion(VersionProp vp, Version oldVer, Version newVer, ref string fileContent)
		{
			string strOldVer = verToAsmVersion(oldVer);
			string strNewVer = verToAsmVersion(newVer);

			fileContent = fileContent.Replace(strOldVer, strNewVer);

			using (StreamWriter sw = new StreamWriter(vp.FilePath) { AutoFlush = true })
			{
				sw.Write(fileContent);
				sw.Close();
			}
		}

		static string verToAsmVersion(Version v)
		{
			string ver = "[assembly: AssemblyVersion(\"" + v.ToString() + "\")]";
			return ver;
		}

		static void showHelp()
		{
			//-f filepath
			//-bm Build number max
			//-rm Revision number max
			//-ri Revision increment
			//-bi Build increment
			//-h help
			string help = @"
gVerManager - By GaryNg@http://github.com/garyng

A simple version manager for managing AssemblyInfo.cs's AssemblyVersion field that implemented in C#

Usage:
gVerManager.exe -f <filepath> [options]

Version Format:
Major.Minor.Build.Revision

Options:
	-rm
		Max revision build number (default : 100)
	-bm 
		Max build number (default : 100)
	-ri 
		Increment for revision (default : 1)
	-bi
		Increment for build (default : 0)

Examples:
gVerManager.exe -f ""AssemblyInfo.cs""
gVerManager.exe -f ""AssemblyInfo.cs"" -rm 200 -bm 2000 -ri 1 -bi 0";
			Console.WriteLine(help);
		}
	}
}
