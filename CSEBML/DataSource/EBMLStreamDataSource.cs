using System.IO;
using System;
namespace CSEBML.DataSource {
	public class EBMLStreamDataSource : IEBMLDataSource {
		private static Int64[] UNKOWNSIZES = { 0x7F, 0x3FFF, 0x1FFFFF, 0x0FFFFFFF, 0x07FFFFFFFF, 0x03FFFFFFFFFF, 0x01FFFFFFFFFFFF, 0x00FFFFFFFFFFFFFF };

		private Stream source;

		public EBMLStreamDataSource(Stream source) { this.source = source; }

		public bool CanSeek { get { return source.CanSeek; } }

		public bool HasKnownLength { get { return false; } }

		public long Length { get { return source.Length; } }

		public long Position {
			get { return source.Position; }
			set { source.Position = value; }
		}

		public byte[] GetData(long neededBytes, out long offset) {
			byte[] block = new byte[neededBytes];
			var readBytes = source.Read(block, 0, (int)neededBytes);

			offset = 0;
			if(readBytes != neededBytes) throw new Exception();
			return block;
		}



		public Int32 ReadIdentifier() {
			int bytesToRead = 0;
			Byte mask = 1 << 7;
			Byte encodedSize = (byte)source.ReadByte();

			while((mask & encodedSize) == 0 && bytesToRead++ < 4) mask = (Byte)(mask >> 1);
			if(bytesToRead == 4) return -1; //Identifiers are Int32

			Byte[] block = new byte[bytesToRead];
			source.Read(block, 0, bytesToRead);

			Int32 value = 0;
			for(int i = 0;i < bytesToRead;i++) value += (Int32)block[i] << ((bytesToRead - i - 1) << 3);

			return value + (encodedSize << (bytesToRead << 3));
		}

		public Int64 ReadVInt() {
			int bytesToRead = 0;
			Byte mask = 1 << 7;
			Byte encodedSize = (byte)source.ReadByte();

			while((mask & encodedSize) == 0 && bytesToRead++ < 8) { mask = (Byte)(mask >> 1); }
			if(bytesToRead == 8) return -1; // //Identifiers are Int64

			Byte[] block = new byte[bytesToRead];
			source.Read(block, 0, bytesToRead);

			Int64 value = 0;
			for(int i = 0;i < bytesToRead;i++) value += (Int64)block[i] << ((bytesToRead - i - 1) << 3);
			value += (encodedSize ^ mask) << (bytesToRead << 3);

			return value == UNKOWNSIZES[bytesToRead] ? -3 : value;
		}


		public bool EOF { get { return Position == Length; } }


		public void WriteIdentifier(Int32 id) {
			var identifierLength = 0;
			while(id > UNKOWNSIZES[identifierLength]) identifierLength++;
			var idBin = new byte[identifierLength];

			for(int i = 0;i < identifierLength;i++) idBin[i] = (byte)(id >> ((identifierLength - i - 1) << 3));
			source.Write(idBin, 0, identifierLength);
		}



		public void WriteVInt(Int64 value, Int32 vIntLength = 1) {
			vIntLength--;
			while(value + 1 > UNKOWNSIZES[vIntLength++]) ;
			var valueBin = new byte[vIntLength];

			for(int i = vIntLength - 1;i >= 0;i--) valueBin[i] = (byte)(value >> ((vIntLength - i - 1) << 3));
			valueBin[0] |= (byte)(1 << (8 - vIntLength));

			source.Write(valueBin, 0, vIntLength);
		}

		private byte[] zeroArray = new byte[8];
		public void WriteFakeVInt(int vIntLength) { source.Write(zeroArray, 0, vIntLength); }

		public void Write(byte[] b, int offset, int length) { source.Write(b, offset, length); }


	}
}
