using System;

namespace CSEBML.DocTypes.Matroska {
    public class MatroskaBlock {
        private byte[] data;
        private long offset;
        private int length;

        private static Int64 ReadVInt(byte[] block, ref Int64 offset, Int64 length) {
            Byte bytesToRead = 0;
            Byte mask = 1 << 7;
            Byte encodedSize = block[offset++];

            while((mask & encodedSize) == 0 && bytesToRead++ < 8) mask = (Byte)(mask >> 1);

            Int64 value = 0;
            for(int i = 0; i < bytesToRead; i++, offset++) {
                if(offset == block.Length) return 0;
                value += (Int64)block[offset] << ((bytesToRead - i - 1) << 3);
            }

            return value + ((encodedSize ^ mask) << (bytesToRead << 3));
        }

        private static byte GetVIntSize(byte encodedSize) {
            Byte mask = 1 << 7;
            Byte vIntLength = 0;
            while((mask & encodedSize) == 0 && vIntLength++ < 8) mask = (Byte)(mask >> 1);
            if(vIntLength == 9) return 0; //TODO Add Warning
            return ++vIntLength;
        }

        public MatroskaBlock(byte[] data, long offset, int length) {
            Int64 startPos = offset;

            TrackNumber = (int)ReadVInt(data, ref offset, length);
            TimeCode = (Int16)((data[offset] << 8) + data[offset + 1]); offset += 2;

            Flags = (BlockFlag)data[offset++];
            LaceType laceType = (LaceType)(Flags & BlockFlag.LaceMask);
            if(laceType != LaceType.None) {
                FrameCount = data[offset++];
                if(laceType == LaceType.Ebml) {
                    for(int i = 0; i < FrameCount; i++) offset += GetVIntSize(data[offset]);

                } else if(laceType == LaceType.Xiph) {
                    int i = 0;
                    while(i++ != FrameCount) if(data[offset++] != 0xFF) ;
                }
            } else FrameCount = 1;


            DataLength = (UInt32)(length - (offset - startPos));

            this.data = data;
            this.offset = offset;
            this.length = length;
        }

        public Int32 TrackNumber { get; private set; }
        public Int16 TimeCode { get; private set; }
        public BlockFlag Flags { get; private set; }
        public LaceType LacingType { get { return (LaceType)(Flags & BlockFlag.LaceMask); } }
        public Byte FrameCount { get; private set; }
        public UInt32 DataLength { get; private set; }
        public byte[] Data {
            get {
                byte[] data = new byte[DataLength];
                Buffer.BlockCopy(this.data, (int)offset, data, 0, (int)DataLength);
                return data;
            }
        }

        public enum BlockFlag : byte {
            Discardable = 0x01,
            LaceMask = 0x06,
            Invisible = 0x08,
            Keyframe = 0x80
        }
        public enum LaceType : byte {
            None = 0x00,
            Xiph = 0x02,
            Fixed = 0x04,
            Ebml = 0x06
        }
    }
}
