using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CSEBML.DocTypes;
using CSEBML.DocTypes.EBML;
using CSEBML.DataSource;

namespace CSEBML {
	public class EBMLReader {
		public const Int32 TargetEBMLVersion = 1;
		private readonly Boolean lengthKnown;

		private IEBMLDoc ebmlDoc;
		private IEBMLDataSource dataSrc;

		private List<ElementInfo> parentElements;
		private ElementInfo currentElement;


		public EBMLReader(IEBMLDataSource dataSrc, IEBMLDoc ebmlDoc) {
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

		public void JumpTo(Int64 position) {
			while(true) {
				if(parentElements[parentElements.Count - 1].ElementPosition < position && GetEndOfElement(parentElements.Count) > position) break;
				LeaveMasterElement();
			}
			dataSrc.Position = position;
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
				throw new InvalidOperationException("File most likely Corrupted");
				//Crap just happened
				//TODO Magic code to fix it goes here
			}

			return currentElement = new ElementInfo(ebmlDoc.RetrieveDocElement(docElementId), oldPosition, dataSrc.Position, length);
		}

		public void EnterMasterElement() {
			if(currentElement != null && currentElement.DocElement.Type == EBMLElementType.Master) {
				parentElements.Add(currentElement);
				currentElement = null;
			} else throw new Exception("Cannot enter non Master Element");
		}

		public void LeaveMasterElement() { LeaveMasterElements(-1); }
		public void LeaveMasterElements(Int32 toLevel) {
			if(parentElements.Count != 0) {
				if(toLevel < 0 & -toLevel <= parentElements.Count) {
					toLevel = parentElements.Count + toLevel;
				} else throw new Exception("Invalid Level");

				//dataSrc.Position = GetEndOfElement(toLevel);
				parentElements.RemoveRange(toLevel, parentElements.Count - toLevel);
				currentElement = null;

			} else throw new Exception("No Master Elements to leave");
		}

		private Int64 GetEndOfElement(Int32 index) {
			if(index < 0) return lengthKnown ? dataSrc.Length : -1;

			ElementInfo elem = index == parentElements.Count ? currentElement : parentElements[index];
			return elem != null && elem.Length.HasValue ? elem.ElementPosition + elem.Length.Value : GetEndOfElement(index - 1);
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
		public class Context {
			private EBMLReader reader;
			private List<ElementInfo> parentElements;

			public ElementInfo CurrentElement { get; private set; }
			public Int64 Position { get; private set; }
			public ReadOnlyCollection<ElementInfo> ParentElements { get { return parentElements.AsReadOnly(); } }


			public Context(EBMLReader reader, ElementInfo currentElement) {
				parentElements = new List<ElementInfo>(reader.ParentElements);
				Position = reader.BaseStream.Position;
				CurrentElement = currentElement;
				this.reader = reader;
			}

			public Boolean IsContextOf(EBMLReader reader) { return this.reader == reader; }
		}
	}
}
