using System;

namespace CSEBML.DocTypes.EBML {
	public class EBMLDocElement {
		public String Name { get; private set; }
		public EBMLElementType Type { get; private set; }
		public Int32 Id { get; private set; }

		public EBMLDocElement(Int32 id, EBMLElementType type, String name) {
			Name = name;
			Type = type;
			Id = id;
		}


	}

	[Flags]
	public enum EBMLElementType {
		Unknown = 0,
		Master = 1 << 0,
		Binary = 1 << 1,
		SInteger = 1 << 2,
		UInteger = 1 << 3,
		Float = 1 << 4,
		//Double = 1 << 5,
		UTF8 = 1 << 6,
		ASCII = 1 << 7,
		Date = 1 << 8,

		Custom = 1 << 30,
	}
}
