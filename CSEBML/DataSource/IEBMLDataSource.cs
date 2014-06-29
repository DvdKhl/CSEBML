//Mod. BSD License (See LICENSE file) DvdKhl (DvdKhl@web.de)
using System;
using System.IO;

namespace CSEBML.DataSource {
    public interface IEBMLDataSource {
        Boolean CanSeek { get; }
        Boolean HasKnownLength { get; }
		Boolean EOF { get; }

        Int64 Length { get; }
        Int64 Position { get; set; }

        Byte[] GetData(Int64 neededBytes, out Int64 offset);

        Int32 ReadIdentifier();
        Int64 ReadVInt();

		void SyncTo(BytePatterns bytePatterns, long seekUntil);

		void WriteIdentifier(Int32 id);
		void WriteVInt(Int64 value, Int32 vIntLength = 1);
		void WriteFakeVInt(Int32 vIntLength);
		
		void Write(Byte[] b, Int32 offset, Int32 length);
		Int64 Write(Stream source);
	}
}
