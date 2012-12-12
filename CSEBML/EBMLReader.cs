using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CSEBML.DocTypes;
using CSEBML.DocTypes.EBML;
using CSEBML.DataSource;
using System.Diagnostics.CodeAnalysis;

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
			if(currentElement != null && currentElement.DataLength.HasValue && currentElement.DataPos == dataSrc.Position) {
				Int64 offset;
				Byte[] valueData = dataSrc.GetData(currentElement.DataLength.Value, out offset);
				return ebmlDoc.RetrieveValue(currentElement.DocElement, valueData, offset, currentElement.DataLength.Value);

			} else throw new InvalidOperationException("Cannot read value: Invalid State");
		}

		public void JumpTo(Int64 position) {
			while(true) {
				if(parentElements[parentElements.Count - 1].IdPos < position && GetEndOfElement(parentElements.Count) > position) break;
				LeaveMasterElement();
			}
			dataSrc.Position = position;
		}

		public ElementInfo NextElementInfo() {
			Int64 idPos = dataSrc.Position;

			if(currentElement != null) {
				idPos = GetEndOfElement(parentElements.Count); //dataSrc.Position;
				if(idPos != dataSrc.Position) dataSrc.Position = idPos;
			}

			if(GetEndOfElement(parentElements.Count - 1) == idPos || dataSrc.EOF) return null;


			Int32 docElementId = dataSrc.ReadIdentifier();
			Int64 vintPos = dataSrc.Position;
			Int64 dataLength = dataSrc.ReadVInt();

#pragma warning disable 675
			if((docElementId | dataLength | VIntConsts.ERROR) != 0) {
				throw new InvalidOperationException("File most likely Corrupted"); //TODO Magic code to resync it goes here
			}
#pragma warning restore 675

			return currentElement = new ElementInfo(ebmlDoc.RetrieveDocElement(docElementId), idPos, vintPos, dataSrc.Position, dataLength);
		}

		public void EnterMasterElement() {
			if(currentElement != null && currentElement.DocElement.Type == EBMLElementType.Master) {
				parentElements.Add(currentElement);
				currentElement = null;
			} else throw new InvalidOperationException("Cannot enter non Master Element");
		}

		public void LeaveMasterElement() { LeaveMasterElements(-1); }
		public void LeaveMasterElements(Int32 toLevel) {
			if(parentElements.Count != 0) {
				if(toLevel < 0 & -toLevel <= parentElements.Count) {
					toLevel = parentElements.Count + toLevel;
				} else throw new InvalidOperationException("Invalid Level");

				//dataSrc.Position = GetEndOfElement(toLevel);
				parentElements.RemoveRange(toLevel, parentElements.Count - toLevel);
				currentElement = null;

			} else throw new InvalidOperationException("No Master Elements to leave");
		}

		private Int64 GetEndOfElement(Int32 index) {
			if(index < 0) return lengthKnown ? dataSrc.Length : -1;

			ElementInfo elem = index == parentElements.Count ? currentElement : parentElements[index];
			return elem != null && elem.DataLength.HasValue ? elem.DataPos + elem.DataLength.Value : GetEndOfElement(index - 1);
		}
	}
}
