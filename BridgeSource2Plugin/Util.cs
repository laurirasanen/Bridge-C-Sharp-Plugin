using System.Collections.Generic;
using System.Linq;

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
	}
}
