using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSEBML.DocTypes.EBML;

namespace CSEBML.DocTypes {
	public interface IEBMLDoc : IDocType {
		EBMLDocElement RetrieveDocElement(Int32 id);

		Byte[] TransformDocElement(EBMLDocElement elem, Object value);
	}
}
