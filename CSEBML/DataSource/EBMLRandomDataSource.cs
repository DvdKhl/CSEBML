using CSEBML.DocTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSEBML.DataSource {
	public class EBMLRandomDataSource : IEBMLDataSource {
		public EBMLDocType DocType { get; private set; }

		private Random rnd;
		private long position;
		private long length;
		private int maxValueLength;

		public EBMLRandomDataSource(int seed, long length, int maxValueLength) {
			rnd = new Random(seed);
			DocType = new RndDocType();

			this.length = length;
			this.maxValueLength = maxValueLength;
		}

		public bool CanSeek { get { return true; } }

		public bool HasKnownLength { get { return length != ~VIntConsts.UNKNOWN_LENGTH; } }

		public bool EOF { get { return length == Position; } }

		public long Length { get { if(length != ~VIntConsts.UNKNOWN_LENGTH) return length; else throw new NotSupportedException(); } }

		public long Position { get { return position; } set { position = value; } }

		public byte[] GetData(long neededBytes, out long offset) {
			offset = 0;
			position += neededBytes;
			return new byte[neededBytes];
		}

		public int ReadIdentifier() {
			position += 4;

			var val = rnd.Next();

			if(val < int.MaxValue / 2) {
				if(position < length - 12 || length == ~VIntConsts.UNKNOWN_LENGTH) {
					val = 1;
				}
			}

			return val;
		}
		public long ReadVInt() {
			position += 8;
			return length != ~VIntConsts.UNKNOWN_LENGTH ? rnd.Next(0, Math.Min(maxValueLength, (int)(length - position))) : rnd.Next(0, maxValueLength);
		}

		public void SyncTo(BytePatterns bytePatterns, long seekUntil) { throw new NotSupportedException(); }


		public void WriteIdentifier(int id) { throw new NotSupportedException(); }
		public void WriteVInt(long value, int vIntLength = 1) { throw new NotSupportedException(); }
		public void WriteFakeVInt(int vIntLength) { throw new NotSupportedException(); }
		public void Write(byte[] b, int offset, int length) { throw new NotSupportedException(); }
		public long Write(System.IO.Stream source) { throw new NotSupportedException(); }

		private class RndDocType : EBMLDocType {
			public static readonly EBMLDocElement Master = new EBMLDocElement(1, EBMLElementType.Master, "Master");
		}
	}
}
