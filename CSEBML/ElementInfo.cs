using CSEBML.DocTypes.EBML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSEBML {
	public class ElementInfo {
		public readonly EBMLDocElement DocElement;
		public readonly Int64 IdPos;
		public readonly Int64 VIntPos;
		public readonly Int64 DataPos;
		public readonly Int64? DataLength;

		public override string ToString() { return DocElement != null ? DocElement.Name.ToString() + "(" + Convert.ToString(DocElement.Id, 16) + ")" : ""; }
		public string ToDetailedString() { return (DocElement != null ? DocElement.Name.ToString() + "(" + Convert.ToString(DocElement.Id, 16) + ")" : "") + " IdPos:" + IdPos + " VIntPos:" + VIntPos + " DataPos:" + DataPos + " Datalength:" + DataLength; }

		public ElementInfo(EBMLDocElement docElement, Int64 idPos, Int64 vintPos, Int64 dataPos) {
			DocElement = docElement;
			IdPos = idPos;
			VIntPos = vintPos;
			DataPos = dataPos;
		}
		public ElementInfo(EBMLDocElement docElement, Int64 idPos, Int64 vintPos, Int64 dataPos, Int64 dataLength)
			: this(docElement, idPos, vintPos, dataPos) {
			DataLength = dataLength;
		}
	}

}
