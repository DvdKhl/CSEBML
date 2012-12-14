//Mod. BSD License (See LICENSE file) DvdKhl (DvdKhl@web.de)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSEBML.DocTypes;

namespace CSEBML.DocTypes.MCChronicles {
	public class MCChroniclesDocType : EBMLDocType {

		protected override object RetrieveByExtension(EBMLDocElement docElem, byte[] data, long offset, long length) {
			if(docElem.Id == Position.Id || docElem.Id == Motion.Id) {
				return new double[] { 
					EBMLDocType.RetrieveDouble(data, offset),
					EBMLDocType.RetrieveDouble(data, offset + 8),
					EBMLDocType.RetrieveDouble(data, offset + 16)
				};
			} else if(docElem.Id == Rotation.Id) {
				return new Single[] { 
					EBMLDocType.RetrieveFloat(data, offset),
					EBMLDocType.RetrieveFloat(data, offset + 4),
				};
			} else if(docElem.Id == IntegralPosition.Id) {
				Byte[] b = new Byte[length];
				Buffer.BlockCopy(data, (Int32)offset, b, 0, (Int32)length);
				if(BitConverter.IsLittleEndian) {
					Array.Reverse(b, 0, 4);
					Array.Reverse(b, 4, 4);
					Array.Reverse(b, 8, 4);
				}

				return new int[] {
					BitConverter.ToInt32(b, 0),
					BitConverter.ToInt32(b, 4),
					BitConverter.ToInt32(b, 8),
				};

			} else if(docElem.Id == Coordinates.Id) {
				Byte[] b = new Byte[length];
				Buffer.BlockCopy(data, (Int32)offset, b, 0, (Int32)length);
				if(BitConverter.IsLittleEndian) {
					Array.Reverse(b, 0, 8);
					Array.Reverse(b, 8, 8);
				}

				return new long[] {
					BitConverter.ToInt64(b, 0),
					BitConverter.ToInt64(b, 8),
				};

			} else if(docElem.Id == CubesSym.Id || docElem.Id == ChunksSym.Id || docElem.Id == RegionsSym.Id || docElem.Id == WorldStatesSym.Id) {
				Byte[] b = new Byte[length];
				Buffer.BlockCopy(data, (Int32)offset, b, 0, (Int32)length);
				if(BitConverter.IsLittleEndian) {
					for(int i = 0;i < b.Length;i += 8) Array.Reverse(b, i, 8);
				}

				var values = new ulong[length / 8];

				for(int i = 0;i < values.Length;i++) values[i] = BitConverter.ToUInt64(b, i * 8);

				return values;
			}

			throw new Exception("Unknown Element");
		}


		protected override byte[] TransformElement(EBMLDocElement docElem, object value) {
			if(docElem.Id == Position.Id || docElem.Id == Motion.Id) {
				var v = (double[])value;
				var b = new Byte[24];
				Buffer.BlockCopy(EBMLDocType.TransformElement(v[0]), 0, b, 0, 8);
				Buffer.BlockCopy(EBMLDocType.TransformElement(v[1]), 0, b, 8, 8);
				Buffer.BlockCopy(EBMLDocType.TransformElement(v[2]), 0, b, 16, 8);
				return b;

			} else if(docElem.Id == Rotation.Id) {
				var v = (Single[])value;
				var b = new Byte[8];
				Buffer.BlockCopy(EBMLDocType.TransformElement(v[0]), 0, b, 0, 4);
				Buffer.BlockCopy(EBMLDocType.TransformElement(v[1]), 0, b, 4, 4);
				return b;

			} else if(docElem.Id == IntegralPosition.Id) {
				var v = (Int32[])value;
				var b = new Byte[12];
				Buffer.BlockCopy(EBMLDocType.TransformElement(v[0]), 0, b, 0, 8);
				Buffer.BlockCopy(EBMLDocType.TransformElement(v[1]), 0, b, 4, 8);
				Buffer.BlockCopy(EBMLDocType.TransformElement(v[2]), 0, b, 8, 8);
				return b;

			} else if(docElem.Id == Coordinates.Id) {
				var v = (Int64[])value;
				var b = new Byte[v.Length * 8];

				for(int i = 0;i < v.Length;i++) Buffer.BlockCopy(EBMLDocType.TransformElement(v[i]), 0, b, i * 8, 8);

				return b;

			} else if(docElem.Id == CubesSym.Id || docElem.Id == ChunksSym.Id || docElem.Id == RegionsSym.Id || docElem.Id == WorldStatesSym.Id) {
				var v = (UInt64[])value;
				var b = new Byte[v.Length * 8];

				for(int i = 0;i < v.Length;i++) Buffer.BlockCopy(EBMLDocType.TransformElement(v[i]), 0, b, i * 8, 8);

				return b;
			}

			throw new Exception("Unknown Element");
		}

		public int MaxDocTypeReadVersion { get { return 1; } }

		public static readonly EBMLDocElement Id = new EBMLDocElement(0x8F, EBMLElementType.UInteger, "Id");
		public static readonly EBMLDocElement UniqueName = new EBMLDocElement(0xE9, EBMLElementType.UTF8, "Id");
		public static readonly EBMLDocElement IntegralPosition = new EBMLDocElement(0xFD, EBMLElementType.Custom, "IntegralPosition");
		public static readonly EBMLDocElement Position = new EBMLDocElement(0xC2, EBMLElementType.Custom, "Position");
		public static readonly EBMLDocElement Permissions = new EBMLDocElement(0x1C00F958, EBMLElementType.Master, "Permissions");
		public static readonly EBMLDocElement AddPermission = new EBMLDocElement(0xF8, EBMLElementType.UTF8, "AddPermission");
		public static readonly EBMLDocElement AddRole = new EBMLDocElement(0x91, EBMLElementType.UTF8, "AddRole");
		public static readonly EBMLDocElement RemovePermission = new EBMLDocElement(0xCE, EBMLElementType.UTF8, "RemovePermission");
		public static readonly EBMLDocElement RemoveRole = new EBMLDocElement(0x88, EBMLElementType.UTF8, "RemoveRole");
		public static readonly EBMLDocElement Nodes = new EBMLDocElement(0x16EC0FFB, EBMLElementType.Master, "Nodes");
		public static readonly EBMLDocElement Node = new EBMLDocElement(0x16D9D047, EBMLElementType.Master, "Node");
		public static readonly EBMLDocElement Key = new EBMLDocElement(0x94, EBMLElementType.UTF8, "Key");
		public static readonly EBMLDocElement Type = new EBMLDocElement(0x83, EBMLElementType.UInteger, "Type");
		public static readonly EBMLDocElement Value = new EBMLDocElement(0xF7, EBMLElementType.Binary, "Value");
		public static readonly EBMLDocElement Hashable = new EBMLDocElement(0x1C9F892D, EBMLElementType.Master, "Hashable");
		public static readonly EBMLDocElement HashNode = new EBMLDocElement(0x130E50E3, EBMLElementType.Master, "HashNode");
		public static readonly EBMLDocElement Algorithm = new EBMLDocElement(0xA2, EBMLElementType.UInteger, "Algorithm");
		public static readonly EBMLDocElement Hash = new EBMLDocElement(0xB6, EBMLElementType.Binary, "Hash");
		public static readonly EBMLDocElement ChronicleEntry = new EBMLDocElement(0x1D4C90A1, EBMLElementType.Master, "ChronicleEntry");
		public static readonly EBMLDocElement Message = new EBMLDocElement(0x97, EBMLElementType.UTF8, "Message");
		public static readonly EBMLDocElement ChronicleSym = new EBMLDocElement(0xC1, EBMLElementType.UInteger, "ChronicleSym");
		public static readonly EBMLDocElement Segment = new EBMLDocElement(0x1EB32211, EBMLElementType.Master, "Segment");
		public static readonly EBMLDocElement Graph = new EBMLDocElement(0x1C65AE36, EBMLElementType.Master, "Graph");
		public static readonly EBMLDocElement Transition = new EBMLDocElement(0x1208DBE8, EBMLElementType.Master, "Transition");
		public static readonly EBMLDocElement Sources = new EBMLDocElement(0x265896, EBMLElementType.Master, "Sources");
		public static readonly EBMLDocElement GraphContinue = new EBMLDocElement(0x15D95E73, EBMLElementType.Master, "GraphContinue");
		public static readonly EBMLDocElement Users = new EBMLDocElement(0x1174F1F7, EBMLElementType.Master, "Users");
		public static readonly EBMLDocElement User = new EBMLDocElement(0x1DFE190C, EBMLElementType.Master, "User");
		public static readonly EBMLDocElement UserName = new EBMLDocElement(0x9A, EBMLElementType.UTF8, "UserName");
		public static readonly EBMLDocElement UserPassword = new EBMLDocElement(0xE6, EBMLElementType.UTF8, "UserPassword");
		public static readonly EBMLDocElement Chronicle = new EBMLDocElement(0x1D2AB18D, EBMLElementType.Master, "Chronicle");
		public static readonly EBMLDocElement MCVersion = new EBMLDocElement(0xAB, EBMLElementType.UTF8, "MCVersion");
		public static readonly EBMLDocElement CommitMessage = new EBMLDocElement(0x86, EBMLElementType.UTF8, "CommitMessage");
		public static readonly EBMLDocElement Date = new EBMLDocElement(0xA7, EBMLElementType.Date, "Date");
		public static readonly EBMLDocElement UserSettings = new EBMLDocElement(0x13497184, EBMLElementType.Master, "UserSettings");
		public static readonly EBMLDocElement UserSetting = new EBMLDocElement(0x278ABE, EBMLElementType.Master, "UserSetting");
		public static readonly EBMLDocElement Universe = new EBMLDocElement(0x1A75B455, EBMLElementType.Master, "Universe");
		public static readonly EBMLDocElement WorldStatesSym = new EBMLDocElement(0xE0, EBMLElementType.Custom, "WorldStatesSym");
		public static readonly EBMLDocElement UserStates = new EBMLDocElement(0x1432E8CC, EBMLElementType.Master, "UserStates");
		public static readonly EBMLDocElement UserState = new EBMLDocElement(0x3051F4, EBMLElementType.Master, "UserState");
		public static readonly EBMLDocElement WorldState = new EBMLDocElement(0x2C26B4, EBMLElementType.Master, "WorldState");
		public static readonly EBMLDocElement RegionsSym = new EBMLDocElement(0xFB, EBMLElementType.Custom, "RegionsSym");
		public static readonly EBMLDocElement Region = new EBMLDocElement(0x342B21, EBMLElementType.Master, "Region");
		public static readonly EBMLDocElement Coordinates = new EBMLDocElement(0xD6, EBMLElementType.Custom, "Coordinates");
		public static readonly EBMLDocElement ChunksSym = new EBMLDocElement(0xB3, EBMLElementType.Custom, "ChunksSym");
		public static readonly EBMLDocElement Chunk = new EBMLDocElement(0x3F7EF3, EBMLElementType.Master, "Chunk");
		public static readonly EBMLDocElement CubesSym = new EBMLDocElement(0xD3, EBMLElementType.Custom, "CubesSym");
		public static readonly EBMLDocElement CubeData = new EBMLDocElement(0x3E1D19, EBMLElementType.Master, "CubeData");
		public static readonly EBMLDocElement BlockData = new EBMLDocElement(0xB9, EBMLElementType.Binary, "BlockData");
		public static readonly EBMLDocElement AdditionalBlockData = new EBMLDocElement(0x85, EBMLElementType.Binary, "AdditionalBlockData");
		public static readonly EBMLDocElement CubeEntities = new EBMLDocElement(0x2C2A3F, EBMLElementType.Master, "CubeEntities");
		public static readonly EBMLDocElement Entities = new EBMLDocElement(0x1001ED00, EBMLElementType.Master, "Entities");
		public static readonly EBMLDocElement Entity = new EBMLDocElement(0x345FF2, EBMLElementType.Master, "Entity");
		public static readonly EBMLDocElement Motion = new EBMLDocElement(0xE5, EBMLElementType.Custom, "Motion");
		public static readonly EBMLDocElement Rotation = new EBMLDocElement(0xD0, EBMLElementType.Custom, "Rotation");
		public static readonly EBMLDocElement FallDistance = new EBMLDocElement(0xAD, EBMLElementType.Float, "FallDistance");
		public static readonly EBMLDocElement Fire = new EBMLDocElement(0xF2, EBMLElementType.UInteger, "Fire");
		public static readonly EBMLDocElement Air = new EBMLDocElement(0xDC, EBMLElementType.UInteger, "Air");
		public static readonly EBMLDocElement OnGround = new EBMLDocElement(0xA1, EBMLElementType.UInteger, "OnGround");
		public static readonly EBMLDocElement TileEntities = new EBMLDocElement(0x12812432, EBMLElementType.Master, "TileEntities");
		public static readonly EBMLDocElement TileEntity = new EBMLDocElement(0x2B3925, EBMLElementType.Master, "TileEntity");
		public static readonly EBMLDocElement TileTicks = new EBMLDocElement(0x16B5F49D, EBMLElementType.Master, "TileTicks");
		public static readonly EBMLDocElement TileTick = new EBMLDocElement(0x354042, EBMLElementType.Master, "TileTick");
	}
}
