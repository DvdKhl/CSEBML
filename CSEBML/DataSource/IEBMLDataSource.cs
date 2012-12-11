using System;

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

		void WriteIdentifier(Int32 id);
		void WriteVInt(Int64 value, Int32 vIntLength = 1);
		void WriteFakeVInt(Int32 vIntLength);

		void Write(Byte[] b, Int32 offset, Int32 length);
    }
}
