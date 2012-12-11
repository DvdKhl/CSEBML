using System;
using System.Collections;
using System.Collections.Generic;

namespace CSEBML.DataSource {
	public class EBMLBlockDataSource : IEBMLDataSource, IDisposable {
		private static Int64[] UNKOWNSIZES = { 0x7F, 0x3FFF, 0x1FFFFF, 0x0FFFFFFF, 0x07FFFFFFFF, 0x03FFFFFFFFFF, 0x01FFFFFFFFFFFF, 0x00FFFFFFFFFFFFFF };

		IEnumerator<byte[]> blocks;

		private Int64 absolutePosition;
		private Int64 relativePosition;
		private Int64 length;

		public EBMLBlockDataSource(IEnumerable<byte[]> blocks, long length) {
			this.blocks = blocks.GetEnumerator();
			this.blocks.MoveNext();

			this.length = length;
			BlockSize = this.blocks.Current.Length;

			advance = () => {
				absolutePosition += this.blocks.Current.Length;
				relativePosition = 0;
				this.blocks.MoveNext();
				ReadBlocks++;
			};
		}

		private Action advance;

		public int BlockSize { get; private set; }
		public int ReadBlocks { get; private set; }
		private int BlockLength() { return (int)Math.Min(blocks.Current.Length, length - ReadBlocks * BlockSize); }

		public bool CanSeek { get { return false; } }
		public long Length { get { return length; } }
		public long Position {
			get { return absolutePosition + relativePosition; }
			set {
				if(value > Length) throw new Exception("Cannot set position greater than the filelength");
				Int64 bytesToSkip = value - (absolutePosition + relativePosition);
				if(bytesToSkip < 0) throw new InvalidOperationException("Cannot seek backwards");

				Int64 bytesSkipped = Math.Min(BlockLength() - relativePosition, bytesToSkip);
				relativePosition += bytesSkipped;
				bytesToSkip -= bytesSkipped;

				while(bytesToSkip != 0) {
					bytesSkipped = BlockLength() - relativePosition;

					if(bytesToSkip >= bytesSkipped) {
						bytesToSkip -= bytesSkipped;
						advance();

					} else {
						relativePosition += bytesToSkip;
						bytesToSkip = 0;
					}
				}
			}
		}

		public Byte[] GetData(Int64 neededBytes, out Int64 offset) {
			Byte[] block = blocks.Current;

			if(BlockLength() - relativePosition > neededBytes) {
				offset = relativePosition;
				relativePosition += neededBytes;

			} else {
				if(absolutePosition + relativePosition + neededBytes > length) { //Requesting more than available
					Position = length;
					offset = 0;
					return null;
				}

				Int32 bytesCopied = 0;
				Byte[] b = new Byte[neededBytes];

				bytesCopied = BlockLength() - (Int32)relativePosition;
				Buffer.BlockCopy(block, (Int32)relativePosition, b, 0, bytesCopied);

				advance();
				block = blocks.Current;
				var blockLength = BlockLength();
				while(bytesCopied + blockLength <= neededBytes) {
					Buffer.BlockCopy(block, 0, b, bytesCopied, blockLength);
					bytesCopied += blockLength;

					advance();
					block = blocks.Current;
					blockLength = BlockLength();
				}


				Buffer.BlockCopy(block, 0, b, bytesCopied, (Int32)neededBytes - bytesCopied);
				relativePosition = neededBytes - bytesCopied;

				offset = 0;
				block = b;
			}

			return block;
		}


		public Int32 ReadIdentifier() {
			int bytesToRead = 0;
			Byte mask = 1 << 7;
			Byte[] block = blocks.Current;
			Byte encodedSize = block[relativePosition++];

			while((mask & encodedSize) == 0 && bytesToRead++ < 4) mask = (Byte)(mask >> 1);
			if(bytesToRead == 4) return -1; //Identifiers are Int32

			Int32 value = 0;
			for(int i = 0;i < bytesToRead;i++) {
				if(relativePosition == BlockLength()) {
					if(absolutePosition + relativePosition + bytesToRead > length) return -2; //Unexpected EOF

					advance();
					block = blocks.Current;
				}
				value += (Int32)block[relativePosition++] << ((bytesToRead - i - 1) << 3);
			}
			if(relativePosition == BlockLength()) advance();

			return value + (encodedSize << (bytesToRead << 3));
		}

		public Int64 ReadVInt() {
			int bytesToRead = 0;
			Byte mask = 1 << 7;
			Byte[] block = blocks.Current;
			Byte encodedSize = block[relativePosition++];

			while((mask & encodedSize) == 0 && bytesToRead++ < 8) { mask = (Byte)(mask >> 1); }
			if(bytesToRead == 8) return -1; // //Identifiers are Int64

			Int64 value = 0;
			for(int i = 0;i < bytesToRead;i++) {
				if(relativePosition == BlockLength()) {
					if(absolutePosition + relativePosition + bytesToRead > length) return -2; //Unexpected EOF
					advance();
					block = blocks.Current;
				}
				value += (Int64)block[relativePosition++] << ((bytesToRead - i - 1) << 3);
			}
			value += (encodedSize ^ mask) << (bytesToRead << 3);

			if(relativePosition == BlockLength()) advance();
			return value == UNKOWNSIZES[bytesToRead] ? -3 : value;
		}


		public bool HasKnownLength { get { return true; } }


		public bool EOF { get { return Length == Position; } }

		public void WriteIdentifier(int id) { throw new NotSupportedException(); }
		public void WriteVInt(long value, int vIntLength) { throw new NotSupportedException(); }
		public void WriteFakeVInt(int vIntLength) { throw new NotSupportedException(); }
		public void Write(byte[] b, int offset, int length) { throw new NotSupportedException(); }
		public long Write(System.IO.Stream source) {			throw new NotSupportedException();		}



		public void Dispose() { blocks.Dispose(); }


	}

}
