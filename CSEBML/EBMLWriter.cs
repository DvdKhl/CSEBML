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

		public EBMLWriter(IEBMLDataSource dataSrc, IEBMLDoc ebmlDoc) {
			this.dataSrc = dataSrc;
			this.ebmlDoc = ebmlDoc;
		}

		public void WriteEBMLHeader(string docType, ulong docTypeVersion, ulong docTypeReadVersion) {
			WriteMasterElement(EBMLDocType.EBMLHeader);
			WriteElement(EBMLDocType.EBMLVersion, 1UL);
			WriteElement(EBMLDocType.EBMLReadVersion, 1UL);
			WriteElement(EBMLDocType.EBMLMaxIDLength, 4UL);
			WriteElement(EBMLDocType.EBMLMaxSizeLength, 8UL);

			WriteElement(EBMLDocType.DocType, docType);
			WriteElement(EBMLDocType.DocTypeVersion, docTypeVersion);
			WriteElement(EBMLDocType.DocTypeReadVersion, docTypeReadVersion);
			WriteEndMasterElement();
		}

		public ElementInfo WriteElement(EBMLDocElement elem, Object value) {
			var binElem = ebmlDoc.TransformDocElement(elem, value);
			return WriteElement(elem, binElem, 0, binElem.Length);
		}

		public ElementInfo WriteElement(EBMLDocElement elem, byte[] b, int offset, int length) {
			Int64 idPos = dataSrc.Position;
			dataSrc.WriteIdentifier(elem.Id);

			Int64 vIntPos = dataSrc.Position;
			dataSrc.WriteVInt(length);

			Int64 dataPos = dataSrc.Position;
			dataSrc.Write(b, offset, length);

			return new ElementInfo(elem, idPos, vIntPos, dataPos, b.Length);
		}

		public MasterElementInfo WriteMasterElement(EBMLDocElement elem) {
			Int64 idPos = dataSrc.Position;
			dataSrc.WriteIdentifier(elem.Id);

			Int64 vIntPos = dataSrc.Position;
			dataSrc.WriteFakeVInt(8);

			var elemInfo = new MasterElementInfo(elem, idPos, vIntPos, dataSrc.Position);
			elemInfo.Disposed += (s, e) => UpdateElementLength(elemInfo);

			return elemInfo;
		}

		private void UpdateElementLength(ElementInfo elemInfo) {
			var currentPos = dataSrc.Position;

			var dataLength = dataSrc.Position - elemInfo.DataPos;
			elemInfo.DataLength = dataLength;

			dataSrc.WriteVInt(dataLength, 8);
			dataSrc.Position = elemInfo.VIntPos;

			dataSrc.Position = currentPos;
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
