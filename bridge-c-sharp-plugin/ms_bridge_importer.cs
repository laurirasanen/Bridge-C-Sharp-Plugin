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

Main function is responsible for starting a thread that listens to the specified port (specified in Bridge_server.cs) for JSON data..

*/





using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using CommandLine;

namespace bridge_c_sharp_plugin
{
	class ms_bridge_importer
	{
		public class Options
		{
			[Option( 'p', "project", Required = true, HelpText = "Project path to export assets to, e.g. \"C:/Program Files (x86)/Steam/steamapps/common/Half-Life Alyx/content/hlvr_addons/my_addon/\"" )]
			public string ProjectPath { get; set; }

			[Option( 'd', "directory", Required = false, Default = "/megascans", HelpText = "Directory to export assets to, relative to project root" )]
			public string ExportDirectory { get; set; }

			[Option( 'l', "listen", Required = false, Default = 24981, HelpText = "The port to listen on" )]
			public int ServerPort { get; set; }
		}

		static void Main( string[] args )
		{
			Parser.Default.ParseArguments<Options>( args )
				.WithParsed( Run );
		}

		static void Run( Options options )
		{
			//Starts the server in background.
			Bridge_Server listener = new Bridge_Server(options.ServerPort);
			listener.StartServer();

			//New line will close the server and exit the console app.
			Console.ReadLine();
			listener.EndServer();
		}

		public static void AssetImporter( string jsonData )
		{
			List<Asset> assets = new List<Asset>();

			//Parsing JSON array for multiple assets.
			string jArray = jsonData;
			JArray assetsJsonArray = JArray.Parse(jArray);
			for ( int i = 0; i < assetsJsonArray.Count; ++i )
			{
				//Parsing JSON data.
				assets.Add( ImportMegascansAssets( assetsJsonArray[i].ToObject<JObject>() ) );
			}

			foreach ( Asset asset in assets )
			{
				//Prints some values from the parsed json data.
				Console.WriteLine( "\nASSET" );
				Console.WriteLine( "- - - - - - - -\n" );
				Console.WriteLine( asset.ToString() );
				Console.WriteLine( "- - - - - - - -" );


			}
		}

		static Asset ImportMegascansAssets( JObject objectList )
		{
			Asset asset = new Asset();

			//Parsing asset properties.
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

			//Initializing asset component lists to avoid null reference error.
			asset.textures = new List<Texture>();
			asset.geometry = new List<Geometry>();
			asset.lodList = new List<GeometryLOD>();
			asset.packedTextures = new List<PackedTextures>();
			asset.meta = new List<MetaElement>();

			//Parse and store geometry list.
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

			//Parse and store LOD list.
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

			//Parse and store meta data list.
			JArray metaData = (JArray)objectList["meta"];
			foreach ( JObject obj in metaData )
			{
				MetaElement mElement = new MetaElement();
				mElement.name = ( string )obj["name"];
				mElement.key = ( string )obj["key"];
				mElement.value = obj["value"];

				asset.meta.Add( mElement );
			}

			//Parse and store textures list.
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

			//Parse and store channel packed textures list.
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

			//Parse and store categories list.
			JArray categories = (JArray)objectList["categories"];
			asset.categories = new string[categories.Count];
			for ( int i = 0; i < categories.Count; ++i )
			{
				asset.categories[i] = ( string )categories[i];
			}

			//Parse and store tags list.
			JArray tags = (JArray)objectList["tags"];
			asset.tags = new string[tags.Count];
			for ( int i = 0; i < tags.Count; ++i )
			{
				asset.tags[i] = ( string )tags[i];
			}

			return asset;
		}
	}
}
