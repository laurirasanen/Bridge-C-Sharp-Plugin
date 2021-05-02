using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System;

namespace BridgeSource2Plugin
{
	static class Util
	{
		public static string SanitizeProjectPath( string path, string projectPath )
		{
			var projectName = projectPath.Split('/').Last();
			var sanPath = path.Replace('\\', '/');
			var parts = new List<string>(sanPath.Split('/'));
			var pastProjectRoot = false;
			sanPath = "";

			parts.ForEach( part =>
			{
				if ( part.Length <= 0 || part == "/" )
					return;

				if ( pastProjectRoot )
				{
					// Lower case everything inside project
					sanPath += part.ToLower();
				}
				else
				{
					sanPath += part;
				}

				if ( part == projectName )
				{
					pastProjectRoot = true;
				}

				if ( parts.IndexOf( part ) < parts.Count - 1 )
				{
					sanPath += "/";
				}
			} );

			return sanPath;
		}

		public static bool GetGameRoot( string projectPath, out string root )
		{
			var parts = projectPath.Split('/');
			string gamePath = "";

			var pastCommon = false;
			var found = false;
			foreach ( var part in parts )
			{
				if ( pastCommon )
				{
					gamePath += part;
					found = true;
					break;
				}
				else
				{
					gamePath += $"{part}/";
					if ( part == "common" )
					{
						pastCommon = true;
					}
				}
			}

			root = gamePath;

			return found;
		}

		public static bool FindInGameDir( string searchPattern, string projectPath, out string foundPath )
		{
			foundPath = "";

			if ( !GetGameRoot( projectPath, out string gamePath ) )
			{
				return false;
			}

			var files = Directory.GetFiles( gamePath, searchPattern, SearchOption.AllDirectories );
			if ( files.Length > 0 )
			{
				foundPath = files[0];
				return true;
			}

			return false;
		}

		public static bool CompileResource( string path, BridgeImporter.Options options )
		{
			if ( FindInGameDir( "resourcecompiler.exe", options.ProjectPath, out string compilerPath ) )
			{
				Console.WriteLine( $"Compiling {path}\n" );
				var proc = new Process();
				proc.StartInfo.FileName = compilerPath;
				proc.StartInfo.Arguments = $"-i \"{path}\"";
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.RedirectStandardOutput = true;

				proc.Start();
				var output = proc.StandardOutput.ReadToEnd();
				proc.WaitForExit();

				Console.WriteLine( output );
				return proc.ExitCode == 0;
			}

			Console.WriteLine( "Failed to find resourcecompiler.exe" );
			return false;
		}
	}
}
