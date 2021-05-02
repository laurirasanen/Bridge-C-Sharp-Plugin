/*

>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>


██████╗ ██████╗ ██╗██████╗  ██████╗ ███████╗    ██╗███╗   ██╗████████╗███████╗ ██████╗ ██████╗  █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
██╔══██╗██╔══██╗██║██╔══██╗██╔════╝ ██╔════╝    ██║████╗  ██║╚══██╔══╝██╔════╝██╔════╝ ██╔══██╗██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
██████╔╝██████╔╝██║██║  ██║██║  ███╗█████╗      ██║██╔██╗ ██║   ██║   █████╗  ██║  ███╗██████╔╝███████║   ██║   ██║██║   ██║██╔██╗ ██║
██╔══██╗██╔══██╗██║██║  ██║██║   ██║██╔══╝      ██║██║╚██╗██║   ██║   ██╔══╝  ██║   ██║██╔══██╗██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
██████╔╝██║  ██║██║██████╔╝╚██████╔╝███████╗    ██║██║ ╚████║   ██║   ███████╗╚██████╔╝██║  ██║██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
╚═════╝ ╚═╝  ╚═╝╚═╝╚═════╝  ╚═════╝ ╚══════╝    ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝

>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

Quixel AB - Megascans Project

The Megascans Integration for Custom Exports was written in C# (.Net 4.0)

Megascans : https://megascans.se

This integration gives you a LiveLink between Megascans Bridge and Custom Exports. The source code is all exposed
and documented for you to use it as you wish (within the Megascans EULA limits, that is).
We provide a set of useful functions for importing json data from Bridge.

We've tried to document the code as much as we could, so if you're having any issues
please send me an email (ajwad@quixel.se) for support.

Main function is responsible for starting a thread that listens to the specified port (specified in BridgeServer.cs) for JSON data..

*/

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;

namespace BridgeSource2Plugin
{
	class BridgeImporter
	{
		public class Options
		{
			[Option( 'p', "project", Required = true, HelpText = "Project path to export assets to, e.g. \"C:/Program Files (x86)/Steam/steamapps/common/Half-Life Alyx/content/hlvr_addons/my_addon\"" )]
			public string ProjectPath { get; set; }

			[Option( 'd', "directory", Required = false, Default = "megascans", HelpText = "Directory to export assets to, relative to project root" )]
			public string ExportDirectory { get; set; }

			[Option( 'l', "listen", Required = false, Default = 24981, HelpText = "The port to listen on, this should be the same as in Bridge" )]
			public int ServerPort { get; set; }

			[Option( 'a', "auto-compile", Required = false, Default = true, HelpText = "Attempt to auto-compile exported assets with resourcecompiler.exe" )]
			public bool AutoCompile { get; set; }

			[Option( "no-clean", Required = false, Default = false, HelpText = "Keep unsupported textures after export" )]
			public bool NoClean { get; set; }

			[Option( "shader", Required = false, Default = "vr_complex.vfx", HelpText = "The shader used for 3D assets and surfaces" )]
			public string Shader { get; set; }

			[Option( "decal-shader", Required = false, Default = "vr_projected_decals.vfx", HelpText = "The shader used for decals and atlases" )]
			public string DecalShader { get; set; }

			[Option( "debug", Required = false, Default = false, HelpText = "Print asset info and don't export" )]
			public bool Debug { get; set; }

			[Option( "ignore-support", Required = false, Default = false, HelpText = "Ignore asset support, export anyway" )]
			public bool IgnoreSupport { get; set; }
		}

		static Options RunOptions;

		static void Main( string[] args )
		{
			Parser.Default.ParseArguments<Options>( args )
				.WithParsed( Run );
		}

		static void Run( Options options )
		{
			RunOptions = options;
			RunOptions.ProjectPath = RunOptions.ProjectPath.Replace( '\\', '/' );

			Console.WriteLine( $"Project: {RunOptions.ProjectPath}" );
			Console.WriteLine( $"Export directory: {RunOptions.ExportDirectory}" );
			Console.WriteLine( $"Port: {RunOptions.ServerPort}" );

			if ( RunOptions.Debug )
			{
				Console.WriteLine( "--debug passed, assets will not be exported" );
			}

			// Starts the server in background.
			BridgeServer listener = new BridgeServer(RunOptions.ServerPort);
			listener.StartServer();

			// New line will close the server and exit the console app.
			Console.ReadLine();
			listener.EndServer();
		}

		public static void AssetImporter( string jsonData )
		{
			List<Asset> assets = new List<Asset>();

			// Parsing JSON array for multiple assets.
			string jArray = jsonData;
			JArray assetsJsonArray = JArray.Parse(jArray);
			for ( int i = 0; i < assetsJsonArray.Count; ++i )
			{
				//Parsing JSON data.
				assets.Add( ImportMegascansAssets( assetsJsonArray[i].ToObject<JObject>() ) );
			}

			foreach ( Asset asset in assets )
			{
				if ( RunOptions.Debug )
				{
					Console.WriteLine( asset.ToString() );
				}

				var supported = true;
				var reason = "";
				asset.meta.ForEach( meta =>
				{
					if ( meta.key == "splitSubmeshes" && ( bool )meta.value == false )
					{
						supported = false;
						reason = "Asset has multiple meshes in a single file, this is not supported.";
						return;
					}
				} );

				if ( !supported && !RunOptions.IgnoreSupport )
				{
					Console.WriteLine( $"Skipping asset: {reason}" );
					Console.WriteLine( "Pass --ignore-support to export anyway" );
					continue;
				}
				else if ( RunOptions.IgnoreSupport )
				{
					Console.WriteLine( $"Warn: {reason}" );
				}

				if ( RunOptions.Debug )
				{
					continue;
				}

				if ( ExportAsset( asset, out string location ) )
				{
					Console.WriteLine( $"Exported to {location}\n" );
					Console.WriteLine( "- - - - - - - -" );
				}
				else
				{
					Console.WriteLine( "FAILED TO EXPORT\n" );
					Console.WriteLine( "- - - - - - - -" );
				}
			}
		}

		static Asset ImportMegascansAssets( JObject objectList )
		{
			Asset asset = new Asset();

			// Parsing asset properties.
			asset.name = ( string )objectList["name"];
			asset.id = ( string )objectList["id"];
			asset.type = ( string )objectList["type"];
			asset.category = ( string )objectList["category"];
			asset.path = ( string )objectList["path"];
			asset.averageColor = ( string )objectList["averageColor"];
			asset.activeLOD = ( string )objectList["activeLOD"];
			asset.textureMimeType = ( string )objectList["textureFormat"];
			asset.meshVersion = ( int )objectList["meshVersion"];
			asset.resolution = ( string )objectList["resolution"];
			asset.resolutionValue = int.Parse( ( string )objectList["resolutionValue"] );
			asset.isCustom = ( bool )objectList["isCustom"];

			// Helpers
			string dirName = new DirectoryInfo(asset.path).Name;
			asset.directoryName = dirName;

			// Initializing asset component lists to avoid null reference error.
			asset.textures = new List<Texture>();
			asset.geometry = new List<Geometry>();
			asset.lodList = new List<GeometryLOD>();
			asset.packedTextures = new List<PackedTextures>();
			asset.meta = new List<MetaElement>();

			// Parse and store geometry list.
			JArray meshComps = (JArray)objectList["meshList"];
			foreach ( JObject obj in meshComps )
			{
				Geometry geo = new Geometry();
				geo.name = ( string )obj["name"];
				geo.path = ( string )obj["path"];
				geo.type = ( string )obj["type"];
				geo.format = ( string )obj["format"];

				asset.geometry.Add( geo );
			}

			// Parse and store LOD list.
			JArray lodComps = (JArray)objectList["lodList"];
			foreach ( JObject obj in lodComps )
			{
				GeometryLOD geo = new GeometryLOD();
				geo.name = ( string )obj["name"];
				geo.path = ( string )obj["path"];
				geo.type = ( string )obj["type"];
				geo.format = ( string )obj["format"];
				geo.lod = ( string )obj["lod"];

				asset.lodList.Add( geo );
			}

			// Parse and store meta data list.
			JArray metaData = (JArray)objectList["meta"];
			foreach ( JObject obj in metaData )
			{
				MetaElement mElement = new MetaElement();
				mElement.name = ( string )obj["name"];
				mElement.key = ( string )obj["key"];

				// Something weird is happening with booleans
				if ( obj["value"].Type == JTokenType.Boolean )
				{
					mElement.value = ( bool )obj["value"];
				}
				else
				{
					mElement.value = ( object )obj["value"];
				}

				asset.meta.Add( mElement );
			}

			// Parse and store textures list.
			JArray textureComps = (JArray)objectList["components"];
			foreach ( JObject obj in textureComps )
			{
				Texture tex = new Texture();
				tex.name = ( string )obj["name"];
				tex.path = ( string )obj["path"];
				tex.type = ( string )obj["type"];
				tex.format = ( string )obj["format"];
				tex.resolution = ( string )obj["resolution"];

				asset.textures.Add( tex );
			}

			// Parse and store channel packed textures list.
			JArray packedTextureComps = (JArray)objectList["packedTextures"];
			foreach ( JObject obj in packedTextureComps )
			{
				PackedTextures tex = new PackedTextures();
				tex.name = ( string )obj["name"];
				tex.path = ( string )obj["path"];
				tex.type = ( string )obj["type"];
				tex.format = ( string )obj["format"];
				tex.resolution = ( string )obj["resolution"];

				tex.channelsData.Red.type = ( string )obj["channelsData"]["Red"][0];
				tex.channelsData.Red.channel = ( string )obj["channelsData"]["Red"][1];
				tex.channelsData.Green.type = ( string )obj["channelsData"]["Green"][0];
				tex.channelsData.Green.channel = ( string )obj["channelsData"]["Green"][1];
				tex.channelsData.Blue.type = ( string )obj["channelsData"]["Blue"][0];
				tex.channelsData.Blue.channel = ( string )obj["channelsData"]["Blue"][1];
				tex.channelsData.Alpha.type = ( string )obj["channelsData"]["Alpha"][0];
				tex.channelsData.Alpha.channel = ( string )obj["channelsData"]["Alpha"][1];
				tex.channelsData.Grayscale.type = ( string )obj["channelsData"]["Grayscale"][0];
				tex.channelsData.Grayscale.channel = ( string )obj["channelsData"]["Grayscale"][1];

				asset.packedTextures.Add( tex );
			}

			// Parse and store categories list.
			JArray categories = (JArray)objectList["categories"];
			asset.categories = new string[categories.Count];
			for ( int i = 0; i < categories.Count; ++i )
			{
				asset.categories[i] = ( string )categories[i];
			}

			// Parse and store tags list.
			JArray tags = (JArray)objectList["tags"];
			asset.tags = new string[tags.Count];
			for ( int i = 0; i < tags.Count; ++i )
			{
				asset.tags[i] = ( string )tags[i];
			}

			return asset;
		}

		static bool ExportAsset( Asset asset, out string location )
		{
			location = $@"{RunOptions.ProjectPath}/{RunOptions.ExportDirectory}/{asset.type}/{asset.directoryName.ToLower()}";

			if ( CopyFiles( ref asset, location ) )
			{
				Console.WriteLine( $"Copied files to {location}" );
			}
			else
			{
				Console.WriteLine( $"Failed to copy files" );
				return false;
			}

			if ( CreateVmat( asset, out string vmatLocation ) )
			{
				Console.WriteLine( $"Created vmat {vmatLocation}" );
				if ( RunOptions.AutoCompile )
				{
					Util.CompileResource( vmatLocation, RunOptions );
				}
			}
			else
			{
				Console.WriteLine( $"Failed to create vmat" );
				return false;
			}

			if ( asset.geometry.Count > 0 || asset.lodList.Count > 0 )
			{
				if ( CreateVmdl( asset, out string vmdlLocation ) )
				{
					Console.WriteLine( $"Created vmdl {vmdlLocation}" );
					if ( RunOptions.AutoCompile )
					{
						Util.CompileResource( vmdlLocation, RunOptions );
					}
				}
				else
				{
					Console.WriteLine( $"Failed to create vmdl" );
					return false;
				}
			}

			return true;
		}

		static bool CopyFiles( ref Asset asset, string location )
		{
			// Sanity
			DirectoryInfo dir = new DirectoryInfo(asset.path);
			if ( !dir.Exists )
			{
				Console.WriteLine( $"Could not find source directory {asset.path}" );
				return false;
			}

			// Create destination directories
			Directory.CreateDirectory( location );
			if ( asset.geometry.Count > 0 )
			{
				Directory.CreateDirectory( $"{location}/geometry" );
			}
			if ( asset.textures.Count > 0 )
			{
				Directory.CreateDirectory( $"{location}/textures" );
			}

			// TODO: remove repetition
			for ( int i = 0; i < asset.geometry.Count; i++ )
			{
				Geometry geometry = asset.geometry[i];
				string destination = geometry.path.Replace( asset.path, $"{location}/geometry" );
				destination = Util.SanitizeProjectPath( destination, RunOptions.ProjectPath );
				Directory.CreateDirectory( Path.GetDirectoryName( destination ) );
				Console.WriteLine( $"Copying geometry {geometry.path} -> {destination}" );
				File.Copy( geometry.path, destination, true );
				geometry.path = destination;
				geometry.name = geometry.name.ToLower();
				asset.geometry[i] = geometry;
			}

			for ( int i = 0; i < asset.lodList.Count; i++ )
			{
				GeometryLOD lod = asset.lodList[i];
				string destination = lod.path.Replace( asset.path, $"{location}/geometry" );
				destination = Util.SanitizeProjectPath( destination, RunOptions.ProjectPath );
				Directory.CreateDirectory( Path.GetDirectoryName( destination ) );
				Console.WriteLine( $"Copying lod {lod.path} -> {destination}" );
				File.Copy( lod.path, destination, true );
				lod.path = destination;
				lod.name = lod.name.ToLower();
				asset.lodList[i] = lod;
			}

			for ( int i = 0; i < asset.textures.Count; i++ )
			{
				Texture texture = asset.textures[i];
				string destination = texture.path.Replace( asset.path, $"{location}/textures" );
				destination = Util.SanitizeProjectPath( destination, RunOptions.ProjectPath );
				Directory.CreateDirectory( Path.GetDirectoryName( destination ) );
				Console.WriteLine( $"Copying texture {texture.path} -> {destination}" );
				File.Copy( texture.path, destination, true );
				texture.path = destination;
				texture.name = texture.name.ToLower();
				asset.textures[i] = texture;
			}

			asset.path = location;
			asset.name = asset.name.ToLower();
			asset.directoryName = asset.directoryName.ToLower();
			asset.id = asset.id.ToLower();

			return true;
		}

		static bool CreateVmat( Asset asset, out string vmatLocation )
		{
			vmatLocation = $@"{asset.path}/materials/";
			Directory.CreateDirectory( vmatLocation );
			vmatLocation += $"/{asset.id}.vmat";

			var shader = RunOptions.Shader;
			if ( asset.type == "atlas" )
			{
				shader = RunOptions.DecalShader;
			}
			var vmatString = $"shader \"{shader}\"\n";
			bool enableOpacity = false;
			bool enableMetalness = false;
			bool enableTransmission = false;
			bool enableNormal = false;

			// Enable specular by default
			vmatString += "F_SPECULAR 1\n";

			// Get all used textures
			asset.textures.ForEach( texture =>
			{
				var textureType = "";
				switch ( texture.type )
				{
					case "albedo":
						textureType = "TextureColor";
						break;

					case "normal":
						textureType = "TextureNormal";
						enableNormal = true;
						break;

					case "opacity":
						textureType = "TextureTranslucency";
						enableOpacity = true;
						break;

					case "roughness":
						textureType = "TextureRoughness";
						break;

					case "ao":
						textureType = "TextureAmbientOcclusion";
						break;

					case "metalness":
						textureType = "TextureMetalness";
						enableMetalness = true;
						break;

					case "transmission":
						textureType = "TextureTranslucency";
						enableTransmission = true;
						break;

					default:
						if ( RunOptions.NoClean )
						{
							Console.WriteLine( $"Unsupported texture type '{texture.type}', skipping in vmat" );
						}
						else
						{
							Console.WriteLine( $"Unsupported texture type '{texture.type}', removing. Pass --no-clean to keep" );
							File.Delete( texture.path );
						}
						break;
				}

				if ( textureType.Length > 0 )
				{
					vmatString += $"{textureType} \"{texture.path.Replace( RunOptions.ProjectPath + "/", "" ).Replace( '\\', '/' )}\"\n";
				}
			} );

			if ( enableOpacity )
			{
				vmatString += "F_ALPHA_TEST 1\n";
				vmatString += "g_flAlphaTestReference \"0.500\"\n";
				vmatString += "g_flAntiAliasedEdgeStrength \"1.000\"\n";
				if ( asset.type != "atlas" )
				{
					vmatString += "F_RENDER_BACKFACES 1\n";
				}
			}

			if ( enableMetalness )
			{
				vmatString += "F_METALNESS_TEXTURE 1\n";
			}

			if ( enableTransmission )
			{
				vmatString += "F_TRANSLUCENT 1\n";
			}

			if ( enableNormal )
			{
				vmatString += "F_NORMAL_MAP 1\n";
			}

			// Indent
			var lines = new List<string>(vmatString.Split('\n'));
			vmatString = "Layer0\n{\n";
			lines.ForEach( line => vmatString += $"\t{line}\n" );
			vmatString += "}";

			// Write
			File.WriteAllText( vmatLocation, vmatString );

			return true;
		}

		static bool CreateVmdl( Asset asset, out string vmdlLocation )
		{
			var vmdlBase = @"templates/basemodel.vmdl";
			vmdlLocation = $@"{asset.path}/{asset.id}.vmdl";

			var vmatLocation = $"{asset.path.Replace( RunOptions.ProjectPath + "/", "" ).Replace( '\\', '/' )}/materials/{asset.id}.vmat";
			var baseLod = File.ReadAllText(@"templates/baselod.txt");
			var baseMesh = File.ReadAllText(@"templates/basemesh.txt");
			var lods = "";
			var meshes = "";

			for ( int i = 0; i < asset.lodList.Count; i++ )
			{
				lods += baseLod.Replace( "$THRESHOLD", ( i * 20 ).ToString() ).Replace( "$MESHNAME", $"unnamed_{i + 1}" ) + "\n\t\t\t\t\t";
				meshes += baseMesh.Replace( "$MESH", $"{asset.lodList[i].path.Replace( RunOptions.ProjectPath + "/", "" ).Replace( '\\', '/' )}" ) + "\n\t\t\t\t\t";
			}

			// Write vmdl
			string vmdl = File.ReadAllText(vmdlBase);
			vmdl = vmdl.Replace( "$VMAT", vmatLocation );
			vmdl = vmdl.Replace( "$LODS", lods );
			vmdl = vmdl.Replace( "$MESHES", meshes );
			File.WriteAllText( vmdlLocation, vmdl );

			return true;
		}
	}
}
