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

		private IEBMLDoc ebmlDoc;
		private IEBMLDataSource dataSrc;
		private Int64 nextElementPos, lastElementPos;

		public EBMLReader(IEBMLDataSource dataSrc, IEBMLDoc ebmlDoc) {
			this.dataSrc = dataSrc;
			this.ebmlDoc = ebmlDoc;

			lastElementPos = dataSrc.HasKnownLength ? dataSrc.Length : ~VIntConsts.UNKNOWN_LENGTH;
		}

		public IEBMLDataSource BaseStream { get { return dataSrc; } }

		public Object RetrieveValue(ElementInfo elem) {
			if(elem != null && elem.DataLength.HasValue && elem.DataPos == dataSrc.Position) {
				Int64 offset;
				Byte[] valueData = dataSrc.GetData(elem.DataLength.Value, out offset);
				return ebmlDoc.RetrieveValue(elem.DocElement, valueData, offset, elem.DataLength.Value);
			} else throw new InvalidOperationException("Cannot read value: Invalid State");
		}

		//public void JumpTo(Int64 position) {
		//	while(true) {
		//		if(parentElements[parentElements.Count - 1].IdPos < position && GetEndOfElement(parentElements.Count) > position) break;
		//		LeaveMasterElement();
		//	}
		//	dataSrc.Position = position;
		//}

		public void Reset() { dataSrc.Position = nextElementPos = lastElementPos = 0; }
		public ElementInfo JumpToElementAt(Int64 elemPos) {
			dataSrc.Position = elemPos;
			nextElementPos = elemPos;
			lastElementPos = ~VIntConsts.UNKNOWN_LENGTH; //TODO: Check corner cases

			return Next();
		}


		public ElementInfo Next() {
			if(nextElementPos == lastElementPos || nextElementPos == ~VIntConsts.UNKNOWN_LENGTH || dataSrc.EOF) return null;
			if(dataSrc.Position != nextElementPos) dataSrc.Position = nextElementPos;

			Int32 docElementId = dataSrc.ReadIdentifier();
			Int64 vintPos = dataSrc.Position;
			Int64? dataLength = dataSrc.ReadVInt();
			Int64 dataPos = dataSrc.Position;

			if(docElementId < 0 && (~docElementId & VIntConsts.ERROR) != 0) {
				throw new InvalidOperationException("File most likely Corrupted"); //TODO Magic code to resync it goes here
			}

			if(dataLength.HasValue && dataLength < 0) {
				if((~dataLength & VIntConsts.ERROR) != 0) throw new InvalidOperationException("File most likely Corrupted"); //TODO Magic code to resync it goes here
				if(~dataLength == VIntConsts.UNKNOWN_LENGTH) dataLength = null;
			}

			var docElem = ebmlDoc.RetrieveDocElement(docElementId);
			var elemInfo = new ElementInfo(docElem, nextElementPos, vintPos, dataPos, dataLength);

			nextElementPos = dataLength.HasValue ? dataPos + dataLength.Value : ~VIntConsts.UNKNOWN_LENGTH;

			return elemInfo;
		}

		public IDisposable EnterElement(ElementInfo elemInfo) {
			var disposable = new PreviousState {
				NextElementPos = nextElementPos,
				LastElementPos = lastElementPos
			};

			disposable.Disposed += (s, e) => {
				var d = (PreviousState)s;
				nextElementPos = d.NextElementPos;
				lastElementPos = d.LastElementPos;
			};

			nextElementPos = elemInfo.DataPos;
			lastElementPos = elemInfo.DataLength.HasValue ? elemInfo.DataPos + elemInfo.DataLength.Value : ~VIntConsts.UNKNOWN_LENGTH;

			return disposable;
		}

		private sealed class PreviousState : IDisposable {
			public event EventHandler Disposed = delegate { };
			public void Dispose() { Disposed(this, EventArgs.Empty); Disposed = null; }

			public Int64 NextElementPos, LastElementPos;
		}
	}
}
