using System;
using CSEBML.DocTypes.EBML;

namespace CSEBML.DocTypes {
    public interface IDocType {
        Object RetrieveValue(EBMLDocElement docElem, Byte[] data, Int64 offset, Int64 length);

		Byte[] TransformDocElement(EBMLDocElement elem, Object value);

		int MaxDocTypeReadVersion { get; }
    }
}
