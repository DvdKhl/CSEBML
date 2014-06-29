
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSEBML.DataSource {
	public class EBMLFixedByteArrayDataSource : IEBMLDataSource {
		private byte[] data;

		public EBMLFixedByteArrayDataSource(byte[] data) { this.data = data; }


		public bool CanSeek { get { return false; } }
		public long Length { get { return data.Length; } }
		public long Position { get; set; }

		public Byte[] GetData(Int64 neededBytes, out Int64 offset) {
			offset = Position;
			return data;
		}

		public void SyncTo(BytePatterns bytePatterns, long seekUntil) {
			long foundPosition = -1;
			bytePatterns.Match(data, (int)Position, (pattern, i) => { foundPosition = i; return false; });

			if(foundPosition > seekUntil) foundPosition = seekUntil;
			if(foundPosition != -1) Position = foundPosition;
		}

		public Int32 ReadIdentifier() {
			int bytesToRead = 0;
			Byte mask = 1 << 7;
			Byte[] block = data;
			Byte encodedSize = block[Position++];

			while((mask & encodedSize) == 0 && bytesToRead++ < 4) mask = (Byte)(mask >> 1);
			if(bytesToRead == 4) return ~VIntConsts.INVALID_LENGTH_DESCRIPTOR_ERROR; //Identifiers are Int32

			Int32 value = 0;
			for(int i = 0;i < bytesToRead;i++) {
				if(Position == data.Length) return ~VIntConsts.BASESOURCE_ERROR; //Unexpected EOF
				value += (Int32)block[Position++] << ((bytesToRead - i - 1) << 3);
			}

			value += (encodedSize << (bytesToRead << 3));

			return value == VIntConsts.RESERVEDVINTS[bytesToRead] ? ~VIntConsts.RESERVED : value;
		}

		public Int64 ReadVInt() {
			int bytesToRead = 0;
			Byte mask = 1 << 7;
			Byte[] block = data;
			Byte encodedSize = block[Position++];

			while((mask & encodedSize) == 0 && bytesToRead++ < 8) { mask = (Byte)(mask >> 1); }
			if(bytesToRead == 8) return ~VIntConsts.INVALID_LENGTH_DESCRIPTOR_ERROR; //Identifiers are Int64

			Int64 value = 0;
			for(int i = 0;i < bytesToRead;i++) {
				if(Position == data.Length) return ~VIntConsts.BASESOURCE_ERROR; //Unexpected EOF
				value += (Int64)block[Position++] << ((bytesToRead - i - 1) << 3);
			}
			value += (encodedSize ^ mask) << (bytesToRead << 3);


			return value == VIntConsts.RESERVEDVINTS[bytesToRead] ? ~VIntConsts.UNKNOWN_LENGTH : value;
		}

		public bool HasKnownLength { get { return true; } }

		public bool EOF { get { return Length == Position; } }

		public void WriteIdentifier(int id) { throw new NotSupportedException(); }
		public void WriteVInt(long value, int vIntLength) { throw new NotSupportedException(); }
		public void WriteFakeVInt(int vIntLength) { throw new NotSupportedException(); }
		public void Write(byte[] b, int offset, int length) { throw new NotSupportedException(); }
		public long Write(System.IO.Stream source) { throw new NotSupportedException(); }

		public void Dispose() { }
	}
}
