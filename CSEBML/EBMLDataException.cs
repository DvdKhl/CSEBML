//Mod. BSD License (See LICENSE file) DvdKhl (DvdKhl@web.de)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSEBML {
	[Serializable]
	public class EBMLDataException : Exception {
		public EBMLDataException() { }
		public EBMLDataException(string message) : base(message) { }
		public EBMLDataException(string message, Exception inner) : base(message, inner) { }
		protected EBMLDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
