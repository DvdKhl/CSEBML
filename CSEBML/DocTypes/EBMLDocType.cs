//Mod. BSD License (See LICENSE file) DvdKhl (DvdKhl@web.de)
using CSEBML.DocTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSEBML.DocTypes {
	public class EBMLDocType {
		protected Dictionary<Int32, EBMLDocElement> docElementMap;

		public EBMLDocType() {
			docElementMap = GetDocElements(this.GetType(), null).ToDictionary(item => item.Id);

		}

		private static IEnumerable<EBMLDocElement> GetDocElements(Type type, Predicate<EBMLDocElement> filter) {
			var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
			foreach(var field in fields) {
				var obj = field.GetValue(null);
				if(obj is EBMLDocElement && (filter == null || filter((EBMLDocElement)obj))) yield return (EBMLDocElement)obj;
			}
		}


		public EBMLDocElement RetrieveDocElement(Int32 id) {
			EBMLDocElement elementType = (docElementMap.TryGetValue(id, out elementType) ? elementType : null);
			if(elementType == null) elementType = new EBMLDocElement(id, EBMLElementType.Unknown, "Unknown");

			return elementType;
		}

		public object RetrieveValue(EBMLDocElement docElem, Byte[] data, Int64 offset, Int64 length) {
			if(offset > Int32.MaxValue || length > Int32.MaxValue) throw new Exception();

			switch(docElem.Type) {
				case EBMLElementType.Master:
				case EBMLElementType.Unknown:
				case EBMLElementType.Binary: return RetrieveBinary(data, offset, length);
				case EBMLElementType.SInteger: return RetrieveSInteger(data, offset, length);
				case EBMLElementType.UInteger: return RetrieveInteger(data, offset, length);
				//case EBMLElementType.Double: return RetrieveDouble(data, offset);
				case EBMLElementType.Float: return length == 4 ? (double)RetrieveFloat(data, offset) : RetrieveDouble(data, offset);
				case EBMLElementType.UTF8: return RetrieveUTF8(data, offset, length);
				case EBMLElementType.ASCII: return RetrieveASCII(data, offset, length);
				case EBMLElementType.Date: return RetrieveDate(data, offset, length);
				case EBMLElementType.Custom: return RetrieveByExtension(docElem, data, offset, length);
				default: throw new Exception("Unhandled ElementType");
			}
		}


		public static Byte[] RetrieveBinary(Byte[] data, Int64 offset, Int64 length) {
			Byte[] value = new Byte[length];
			Buffer.BlockCopy(data, (Int32)offset, value, 0, (Int32)length);
			return value;
		}
		public static Int64 RetrieveSInteger(Byte[] data, Int64 offset, Int64 length) {
			Int64 sInteger = (data[offset] & 0x80) != 0 ? -1 : 0;
			for(Int32 i = 0;i < length;i++) { sInteger <<= 8; sInteger |= data[offset + i]; }
			return sInteger;
		}
		public static UInt64 RetrieveInteger(Byte[] data, Int64 offset, Int64 length) {
			UInt64 uInteger = 0;
			for(Int32 i = 0;i < length;i++) uInteger |= (UInt64)data[offset + i] << (((Int32)length - i - 1) << 3);
			return uInteger;
		}
		public static Single RetrieveFloat(Byte[] data, Int64 offset) {
			Byte[] bFloat = new Byte[4];
			Buffer.BlockCopy(data, (Int32)offset, bFloat, 0, 4);
			if(BitConverter.IsLittleEndian) Array.Reverse(bFloat); //TODO: Meh
			return BitConverter.ToSingle(bFloat, 0);
		}
		public static Double RetrieveDouble(Byte[] data, Int64 offset) {
			Byte[] bFloat = new Byte[8];
			Buffer.BlockCopy(data, (Int32)offset, bFloat, 0, 8);
			if(BitConverter.IsLittleEndian) Array.Reverse(bFloat); //TODO: Meh
			return BitConverter.ToDouble(bFloat, 0);
		}
		public static String RetrieveUTF8(Byte[] data, Int64 offset, Int64 length) { return System.Text.Encoding.UTF8.GetString(data, (Int32)offset, (Int32)length); }
		public static String RetrieveASCII(Byte[] data, Int64 offset, Int64 length) { return System.Text.Encoding.ASCII.GetString(data, (Int32)offset, (Int32)length); }
		public static DateTime RetrieveDate(Byte[] data, Int64 offset, Int64 length) {
			Int64 nanos = 0;
			for(Int32 i = 0;i < length;i++) nanos += (Int64)data[offset + i] << (((Int32)length - i - 1) << 3);
			return new DateTime(2001, 1, 1, 0, 0, 0).Add(TimeSpan.FromTicks(nanos / 100));
		}
		protected virtual object RetrieveByExtension(EBMLDocElement docElem, byte[] data, long offset, long length) { throw new NotSupportedException(); }

		public Int32 MaxEBMLReadVersion { get { return 1; } }


		#region DocTypes
		public static readonly EBMLDocElement EBMLHeader = new EBMLDocElement(0x1A45DFA3, EBMLElementType.Master, "EBMLHeader");
		public static readonly EBMLDocElement EBMLVersion = new EBMLDocElement(0x4286, EBMLElementType.UInteger, "EBMLVersion");
		public static readonly EBMLDocElement EBMLReadVersion = new EBMLDocElement(0x42F7, EBMLElementType.UInteger, "EBMLReadVersion");
		public static readonly EBMLDocElement EBMLMaxIDLength = new EBMLDocElement(0x42F2, EBMLElementType.UInteger, "EBMLMaxIDLength");
		public static readonly EBMLDocElement EBMLMaxSizeLength = new EBMLDocElement(0x42F3, EBMLElementType.UInteger, "EBMLMaxSizeLength");
		public static readonly EBMLDocElement DocType = new EBMLDocElement(0x4282, EBMLElementType.UTF8, "DocType");
		public static readonly EBMLDocElement DocTypeVersion = new EBMLDocElement(0x4287, EBMLElementType.UInteger, "DocTypeVersion");
		public static readonly EBMLDocElement DocTypeReadVersion = new EBMLDocElement(0x4285, EBMLElementType.UInteger, "DocTypeReadVersion");
		public static readonly EBMLDocElement CRC32 = new EBMLDocElement(0xBF, EBMLElementType.Binary, "CRC32");
		public static readonly EBMLDocElement Void = new EBMLDocElement(0xEC, EBMLElementType.Binary, "Void");
		#endregion


		public Byte[] TransformDocElement(EBMLDocElement elem, Object value) {
			switch(elem.Type) {
				case EBMLElementType.Binary: return (Byte[])value;
				case EBMLElementType.SInteger: return TransformElement((Int64)value);
				case EBMLElementType.UInteger: return TransformElement((UInt64)value);
				case EBMLElementType.Float: return value is Single ? TransformElement((Single)value) : TransformElement((Double)value);
				case EBMLElementType.UTF8: return TransformElement((String)value, false);
				case EBMLElementType.ASCII: return TransformElement((String)value, true);
				case EBMLElementType.Date: return TransformElement((DateTime)value);
				case EBMLElementType.Custom: return TransformDocElement(elem, value);
				case EBMLElementType.Unknown:
				case EBMLElementType.Master:
				default: throw new Exception();
			}
		}
		public static Byte[] TransformElement(Int64 value) {
			var bin = BitConverter.GetBytes(value);
			if(BitConverter.IsLittleEndian) Array.Reverse(bin);
			return bin;
			//return bin.SkipWhile(ldByte => ldByte == 0).ToArray();
		}
		public static Byte[] TransformElement(UInt64 value) {
			var bin = BitConverter.GetBytes(value);
			if(BitConverter.IsLittleEndian) Array.Reverse(bin);
			return bin;
			//return bin.SkipWhile(ldByte => ldByte == 0).ToArray();
		}
		public static Byte[] TransformElement(Single value) {
			var bin = BitConverter.GetBytes(value);
			if(BitConverter.IsLittleEndian) Array.Reverse(bin);
			return bin;
		}
		public static Byte[] TransformElement(Double value) {
			var bin = BitConverter.GetBytes(value);
			if(BitConverter.IsLittleEndian) Array.Reverse(bin);
			return bin;
		}
		public static Byte[] TransformElement(String value, Boolean asASCII = false) { return asASCII ? Encoding.ASCII.GetBytes(value) : Encoding.UTF8.GetBytes(value); }
		public static Byte[] TransformElement(DateTime value) {
			var nanos = (value - new DateTime(2001, 1, 1, 0, 0, 0)).Ticks / 100;
			return TransformElement(nanos);
		}
		protected virtual Byte[] TransformElement(EBMLDocElement elem, Object value) { throw new NotSupportedException(); }
	}
}
