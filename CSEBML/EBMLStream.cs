using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSEBML.DataSource;
using CSEBML.DocTypes;
using System.Collections.ObjectModel;
using CSEBML.DocTypes.EBML;

namespace CSEBML {
	public class EBMLStream {
		public const Int32 TargetEBMLVersion = 1;
		private readonly Boolean lengthKnown;

		private IEBMLDoc ebmlDoc;
		private IEBMLDataSource dataSrc;

		private List<ElementInfo> parentElements;
		private ElementInfo currentElement;


		public EBMLStream(IEBMLDataSource dataSrc, IEBMLDoc ebmlDoc) {
			parentElements = new List<ElementInfo>();

			lengthKnown = dataSrc.HasKnownLength;

			this.dataSrc = dataSrc;
			this.ebmlDoc = ebmlDoc;
		}

		public IEBMLDataSource BaseStream { get { return dataSrc; } }
		public ReadOnlyCollection<ElementInfo> ParentElements { get { return parentElements.AsReadOnly(); } }

		public Object RetrieveValue() {
			if(currentElement != null && currentElement.Length.HasValue && currentElement.ElementPosition == dataSrc.Position) {
				Int64 offset;
				Byte[] valueData = dataSrc.GetData(currentElement.Length.Value, out offset);
				return ebmlDoc.RetrieveValue(currentElement.DocElement, valueData, offset, currentElement.Length.Value);

			} else throw new Exception("Cannot read value. Invalid State");
		}

		public ElementInfo NextElementInfo() {
			Int64 oldPosition = dataSrc.Position;

			if(currentElement != null) {
				oldPosition = GetEndOfElement(parentElements.Count); //dataSrc.Position;
				if(oldPosition != dataSrc.Position) dataSrc.Position = oldPosition;
			}

			if(GetEndOfElement(parentElements.Count - 1) == oldPosition || dataSrc.EOF) return null;


			Int32 docElementId = dataSrc.ReadIdentifier();
			Int64 length = dataSrc.ReadVInt();

			if(docElementId < 0 || length < 0) {
				//Crap just happened
				//TODO Magic code to fix it goes here
			}

			return currentElement = new ElementInfo(ebmlDoc.RetrieveDocElement(docElementId), oldPosition, dataSrc.Position, length);
		}

		public IDisposable EnterMasterElement() {
			if(currentElement != null && currentElement.DocElement.Type == EBMLElementType.Master) {
				parentElements.Add(currentElement);
				currentElement = null;
			} else throw new Exception("Cannot enter non Master Element");

			return new EBMLHandle(parentElements.Last(), (handle) => {
				if(!handle.Equals(parentElements.Last())) throw new Exception("Cannot enter Master Element (Mismatch)");
				LeaveMasterElement();
			});
		}

		private void LeaveMasterElement() { LeaveMasterElements(-1); }
		private void LeaveMasterElements(Int32 toLevel) {
			if(parentElements.Count != 0) {
				if(toLevel < 0 & -toLevel <= parentElements.Count) {
					toLevel = parentElements.Count + toLevel;
				} else if(toLevel > parentElements.Count) throw new Exception("Invalid Level");

				dataSrc.Position = GetEndOfElement(toLevel);
				parentElements.RemoveRange(toLevel, parentElements.Count - toLevel);
				currentElement = null;

			} else throw new Exception("No Master Elements to leave");
		}

		private Int64 GetEndOfElement(Int32 index) {
			if(index < 0) return lengthKnown ? dataSrc.Length : -1;

			ElementInfo elem = index == parentElements.Count ? currentElement : parentElements[index];
			return elem != null && elem.Length.HasValue ? elem.ElementPosition + elem.Length.Value : GetEndOfElement(index - 1);
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

			var elemInfo = new ElementInfo(elem, idPos, dataPos, binElem.Length);

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

			var elemInfo = new ElementInfo(elem, idPos, dataPos, length);
			//Debug.WriteLine("WriteBinaryElement: " + elemInfo.ToDetailedString() + " DataSrcPos: " + dataSrc.Position);

			return elemInfo;
		}

		public ElementInfo WriteStartMasterElement(EBMLDocElement elem) {
			Int64 idPos = dataSrc.Position;
			dataSrc.WriteIdentifier(elem.Id);

			Int64 vIntPos = dataSrc.Position;
			dataSrc.WriteVInt(0, 8);

			var elemInfo = new ElementInfo(elem, idPos, dataSrc.Position, null); //TODO null
			parentElements.Add(elemInfo);
			currentElement = null;

			//Debug.WriteLine("WriteStartMasterElement: " + elemInfo.ToDetailedString() + " DataSrcPos: " + dataSrc.Position);
			return elemInfo;
		}

		public long WriteEndMasterElement() {
			var elemInfo = parentElements[parentElements.Count - 1];
			parentElements.RemoveAt(parentElements.Count - 1);


			var srcPos = dataSrc.Position;

			//dataSrc.Position = elemInfo.VIntPos;
			//dataSrc.WriteVInt(srcPos - elemInfo.DataPos, (Int32)(elemInfo.DataPos - elemInfo.VIntPos));

			dataSrc.Position = srcPos;

			//Debug.WriteLine("WriteEndMasterElement: " + elemInfo.ToDetailedString() + " DataLength: " + (srcPos - elemInfo.DataPos) + " DataSrcPos: " + dataSrc.Position);
			return dataSrc.Position;
		}

		//public void UpdateMasterElementLength(ElementInfo elemInfo) { UpdateMasterElementLength(elemInfo, dataSrc.Position - elemInfo.DataPos); }
		public void UpdateMasterElementLength(ElementInfo elemInfo, long length) {
			var currentPos = dataSrc.Position;

			//TODO: Check if the vint is big enough

			//dataSrc.Position = elemInfo.VIntPos;
			dataSrc.WriteVInt(length, 8);

			dataSrc.Position = currentPos;
		}













		public class ElementInfo {
			private EBMLDocElement docElement;
			private Int64 position;
			private Int64? length;

			public EBMLDocElement DocElement { get { return docElement; } }
			public Int64 ElementPosition { get { return position; } }
			public Int64 DataPosition { get { return position; } }
			public Int64? Length { get { return length; } }

			public override string ToString() { return docElement != null ? docElement.Name.ToString() + "(" + Convert.ToString(docElement.Id, 16) + ")" : ""; }

			internal ElementInfo(EBMLDocElement docElement, Int64 elementPosition, Int64 dataPosition, Int64? length) {
				this.docElement = docElement;
				this.position = dataPosition;
				this.length = length;
			}
		}

		//public class Context {
		//    private EBMLStream ebmlStream;
		//    private List<ElementInfo> parentElements;

		//    public ElementInfo CurrentElement { get; private set; }
		//    public Int64 Position { get; private set; }
		//    public ReadOnlyCollection<ElementInfo> ParentElements { get { return parentElements.AsReadOnly(); } }


		//    public Context(EBMLStream ebmlStream) {
		//        parentElements = new List<ElementInfo>(ebmlStream.ParentElements);
		//        Position = ebmlStream.BaseStream.Position;
		//        CurrentElement = ebmlStream.currentElement;
		//        this.ebmlStream = ebmlStream;
		//    }

		//    public Boolean IsContextOf(EBMLStream ebmlStream) { return this.ebmlStream == ebmlStream; }
		//}

		public class EBMLHandle : IDisposable {
			public Action<object> onDisposed;

			public object Handle { get; private set; }

			public EBMLHandle(object handle, Action<object> onDisposed) { Handle = handle; this.onDisposed = onDisposed; }

			public void Dispose() { if(onDisposed != null) onDisposed(Handle); }
		}
	}
}
