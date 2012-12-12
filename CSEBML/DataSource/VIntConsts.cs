using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSEBML.DataSource {
	public static class VIntConsts {
		public readonly static Int64[] RESERVEDVINTS = { 0x7F, 0x3FFF, 0x1FFFFF, 0x0FFFFFFF, 0x07FFFFFFFF, 0x03FFFFFFFFFF, 0x01FFFFFFFFFFFF, 0x00FFFFFFFFFFFFFF };

		public const int UNKNOWN_LENGTH = 1;

		public const int RESERVED = 2;

		public const int ERROR = 1024;
		public const int INVALID_LENGTH_DESCRIPTOR_ERROR = ERROR | 1;
		public const int BASESOURCE_ERROR = ERROR | 2;
	}
}
