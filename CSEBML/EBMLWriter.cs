using CSEBML.DataSource;
using CSEBML.DocTypes;
using CSEBML.DocTypes.EBML;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CSEBML {
	public class EBMLWriter {
		private IEBMLDoc ebmlDoc;
		private IEBMLDataSource dataSrc;

		private Stack<ElementInfo> parentElements = new Stack<ElementInfo>();

		public EBMLWriter(IEBMLDataSource dataSrc, IEBMLDoc ebmlDoc) {
			this.dataSrc = dataSrc;
			this.ebmlDoc = ebmlDoc;
		}

		public void WriteHeader(string docType, ulong docTypeVersion, ulong docTypeReadVersion) {
			WriteStartMasterElement(EBMLDocType.EBMLHeader);
			WriteElement(EBMLDocType.EBMLVersion, 1UL);
			WriteElement(EBMLDocType.EBMLReadVersion, 1UL);
			WriteElement(EBMLDocType.EBMLMaxIDLength, 4UL);
			WriteElement(EBMLDocType.EBMLMaxSizeLength, 8UL);

			WriteElement(EBMLDocType.DocType, docType);
			WriteElement(EBMLDocType.DocTypeVersion, docTypeVersion);
			WriteElement(EBMLDocType.DocTypeReadVersion, docTypeReadVersion);
			WriteEndMasterElement();

		}

		public ElementInfo WriteElement(EBMLDocElement elem, Object value, int vIntLength = 1) {
			var binElem = ebmlDoc.TransformDocElement(elem, value);

			Int64 idPos = dataSrc.Position;
			dataSrc.WriteIdentifier(elem.Id);

			Int64 vIntPos = dataSrc.Position;
			dataSrc.WriteVInt(binElem.Length, vIntLength);

			Int64 dataPos = dataSrc.Position;
			dataSrc.Write(binElem, 0, binElem.Length);

			var elemInfo = new ElementInfo(elem, idPos, vIntPos, dataPos);

			//Debug.WriteLine("WriteElement: " + elemInfo.ToDetailedString() + " DataSrcPos: " + dataSrc.Position);
			return elemInfo;
		}

		public ElementInfo WriteBinaryElement(EBMLDocElement elem, byte[] b, int offset, int length) {
			Int64 idPos = dataSrc.Position;
			dataSrc.WriteIdentifier(elem.Id);

			Int64 vIntPos = dataSrc.Position;
			dataSrc.WriteVInt(length);

			Int64 dataPos = dataSrc.Position;
			dataSrc.Write(b, offset, length);

			var elemInfo = new ElementInfo(elem, idPos, vIntPos, dataPos);
			//Debug.WriteLine("WriteBinaryElement: " + elemInfo.ToDetailedString() + " DataSrcPos: " + dataSrc.Position);

			return elemInfo;
		}

		public ElementInfo WriteStartMasterElement(EBMLDocElement elem) {
			Int64 idPos = dataSrc.Position;
			dataSrc.WriteIdentifier(elem.Id);

			Int64 vIntPos = dataSrc.Position;
			dataSrc.WriteVInt(0, 8);

			var elemInfo = new ElementInfo(elem, idPos, vIntPos, dataSrc.Position);
			parentElements.Push(elemInfo);

			//Debug.WriteLine("WriteStartMasterElement: " + elemInfo.ToDetailedString() + " DataSrcPos: " + dataSrc.Position);
			return elemInfo;
		}

		public long WriteEndMasterElement() {
			var elemInfo = parentElements.Pop();


			var srcPos = dataSrc.Position;

			dataSrc.Position = elemInfo.VIntPos;
			dataSrc.WriteVInt(srcPos - elemInfo.DataPos, (Int32)(elemInfo.DataPos - elemInfo.VIntPos));

			dataSrc.Position = srcPos;

			//Debug.WriteLine("WriteEndMasterElement: " + elemInfo.ToDetailedString() + " DataLength: " + (srcPos - elemInfo.DataPos) + " DataSrcPos: " + dataSrc.Position);
			return dataSrc.Position;
		}

		public void UpdateMasterElementLength(ElementInfo elemInfo) { UpdateMasterElementLength(elemInfo, dataSrc.Position - elemInfo.DataPos); }
		public void UpdateMasterElementLength(ElementInfo elemInfo, long length) {
			var currentPos = dataSrc.Position;

			//TODO: Check if the vint is big enough

			dataSrc.Position = elemInfo.VIntPos;
			dataSrc.WriteVInt(length, 8);

			dataSrc.Position = currentPos;
		}


		public ContextObj Context {
			get { return new ContextObj(this); }
			set {
				parentElements = new Stack<ElementInfo>(value.ParentElements);
				dataSrc.Position = value.Position;
			}
		}
		public void SetReaderContext(EBMLReader.Context context) { Context = new ContextObj(this, context); }

		//public class ElementInfo {
		//	public EBMLDocElement DocElement;
		//	public Int64 IdPos;
		//	public Int64 VIntPos;
		//	public Int64 DataPos;
		//
		//	public override string ToString() { return DocElement != null ? DocElement.Name.ToString() + "(" + Convert.ToString(DocElement.Id, 16) + ")" : ""; }
		//	public string ToDetailedString() { return (DocElement != null ? DocElement.Name.ToString() + "(" + Convert.ToString(DocElement.Id, 16) + ")" : "") + " IdPos:" + IdPos + " VIntPos:" + VIntPos + " DataPos:" + DataPos; }
		//}


		public class ContextObj {
			private EBMLWriter writer;
			private List<ElementInfo> parentElements;

			public Int64 Position { get; private set; }
			public ReadOnlyCollection<ElementInfo> ParentElements { get { return parentElements.AsReadOnly(); } }


			public ContextObj(EBMLWriter writer) {
				parentElements = new List<ElementInfo>(writer.parentElements);
				Position = writer.dataSrc.Position;
				this.writer = writer;
			}
			public ContextObj(EBMLWriter writer, EBMLReader.Context context) {

				Func<int, int> idLength = (id) => {
					int length = 0;
					while(id != 0) { id = id >> 8; length++; }
					return length;
				};

				parentElements = context.ParentElements.Select(ldElem => new ElementInfo(
					ldElem.DocElement,
					ldElem.IdPos,
					ldElem.IdPos + idLength(ldElem.DocElement.Id),
					ldElem.DataPos
				)).ToList();
				Position = context.Position;
				this.writer = writer;
			}


			public Boolean IsContextOf(EBMLWriter writer) { return this.writer == writer; }
		}

		public static void Optimize(IEBMLDoc ebmlDoc, Stream source, Stream target) {
			var dataSrc = new EBMLStreamDataSource(source);
			var dataTrg = new EBMLStreamDataSource(target);

			while(source.Position != source.Length) {
				var id = dataSrc.ReadIdentifier();
				var length = dataSrc.ReadVInt();
				var docElem = ebmlDoc.RetrieveDocElement(id);

				dataTrg.WriteIdentifier(id);
				dataTrg.WriteVInt(length);

				if(docElem.Type != EBMLElementType.Master) {
					var offset = 0L;
					var b = dataSrc.GetData(length, out offset);
					dataTrg.Write(b, (Int32)offset, b.Length);
				}
			}
		}
	}
}
