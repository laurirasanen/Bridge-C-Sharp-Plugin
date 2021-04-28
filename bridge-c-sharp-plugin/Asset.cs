using System;
using System.Collections.Generic;

namespace bridge_c_sharp_plugin
{
	//This class represents the data structure of Bridge exported JSON.
	struct Asset
	{
		public string resolution;
		public int resolutionValue;
		public string category;
		public string type;
		public string id;
		public string name;
		public string directoryName;
		public string path;
		public string textureMimeType;
		public string averageColor;
		public string activeLOD;
		public string[] tags;
		public string[] categories;

		public bool isCustom;
		public int meshVersion;

		public List<Texture> textures;
		public List<Geometry> geometry;
		public List<GeometryLOD> lodList;
		public List<PackedTextures> packedTextures;
		public List<MetaElement> meta;

		public override string ToString()
		{
			var stringVal = "";

			foreach ( var field in GetType().GetFields() )
			{
				var value = field.GetValue(this);

				if ( value is System.Collections.IList list )
				{
					Console.WriteLine( field.Name + ": " );

					if ( value is string[] stringArr )
					{
						foreach ( var item in list )
						{
							Console.WriteLine( " " + item.ToString() );
						}
					}
					else
					{
						foreach ( var item in list )
						{
							Console.WriteLine( item.ToString() );
						}
					}
				}
				else
				{
					Console.WriteLine( field.Name + ": " + field.GetValue( this ).ToString() );
				}
			}

			return stringVal;
		}
	}

	struct Texture
	{
		public string name;
		public string path;
		public string resolution;
		public string format;
		public string type;

		public override string ToString()
		{
			return $" {name}:\n  path: {path}\n  resolution: {resolution}\n  format: {format}\n  type: {type}";
		}
	}

	struct Geometry
	{
		public string path;
		public string name;
		public string format;
		public string type;

		public override string ToString()
		{
			return $" {name}:\n  path: {path}\n  format: {format}\n  type: {type}";
		}
	}

	struct GeometryLOD
	{
		public string lod;
		public string path;
		public string name;
		public string format;
		public string type;

		public override string ToString()
		{
			return $" {name}:\n  path: {path}\n  lod: {lod}\n  format: {format}\n  type: {type}";
		}
	}

	struct PackedTextures
	{
		public string name;
		public string path;
		public string resolution;
		public string format;
		public string type;
		public ChannelsData channelsData;

		public override string ToString()
		{
			return $" {name}:\n  path: {path}\n  resolution: {resolution}\n  format: {format}\n  type: {type}";
		}
	}

	struct ChannelsData
	{
		public ChannelInfo Red;
		public ChannelInfo Green;
		public ChannelInfo Blue;
		public ChannelInfo Alpha;
		public ChannelInfo Grayscale;
	}

	struct ChannelInfo
	{
		public string type;
		public string channel;
	}

	struct MetaElement
	{
		public object value;
		public string name;
		public string key;

		public override string ToString()
		{
			return $" {name}:\n  key: {key}\n  value: {value.ToString()}";
		}
	}
}
