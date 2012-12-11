using CSEBML.DocTypes.EBML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSEBML {
	public class ElementInfo {
		public EBMLDocElement DocElement { get; private set; }
		public Int64 IdPos { get; private set; }
		public Int64 VIntPos { get; private set; }
		public Int64 DataPos { get; private set; }
		public Int64? DataLength { get; internal set; }

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
	public class MasterElementInfo : ElementInfo, IDisposable {
		public event EventHandler Disposed = delegate { };

		public void Dispose() { Disposed(this, EventArgs.Empty); }

		public MasterElementInfo(EBMLDocElement docElement, Int64 idPos, Int64 vintPos, Int64 dataPos, Int64 dataLength) : base(docElement, idPos, vintPos, dataPos, dataLength) { }
		public MasterElementInfo(EBMLDocElement docElement, Int64 idPos, Int64 vintPos, Int64 dataPos) : base(docElement, idPos, vintPos, dataPos) { }

	}

}
