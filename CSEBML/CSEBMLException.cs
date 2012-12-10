using System;

namespace CSEBML {
    [Serializable]
    public class CSEBMLException : Exception {
        public CSEBMLException() { }
        public CSEBMLException(string message) : base(message) { }
        public CSEBMLException(string message, Exception inner) : base(message, inner) { }

        protected CSEBMLException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
