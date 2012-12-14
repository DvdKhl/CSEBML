//Mod. BSD License (See LICENSE file) DvdKhl (DvdKhl@web.de)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CSEBML.DocTypes;
using CSEBML.DataSource;
using System.Diagnostics.CodeAnalysis;

namespace CSEBML {
	public class EBMLReader {
		private EBMLDocType docType;
		private IEBMLDataSource dataSrc;
		private Int64 nextElementPos, lastElementPos;

		public event EventHandler DataError;

		public EBMLReader(IEBMLDataSource dataSrc, EBMLDocType ebmlDoc) {
			this.dataSrc = dataSrc;
			this.docType = ebmlDoc;

			lastElementPos = dataSrc.HasKnownLength ? dataSrc.Length : ~VIntConsts.UNKNOWN_LENGTH;
		}

		public IEBMLDataSource BaseStream { get { return dataSrc; } }
		public EBMLDocType DocType { get { return docType; } }

		public Object RetrieveValue(ElementInfo elem) {
			if(elem != null && elem.DataLength.HasValue && elem.DataPos == dataSrc.Position) {
				Int64 offset;
				Byte[] valueData = dataSrc.GetData(elem.DataLength.Value, out offset);
				return docType.RetrieveValue(elem.DocElement, valueData, offset, elem.DataLength.Value);
			} else throw new InvalidOperationException("Cannot read value: Invalid State");
		}

		public void Reset() { dataSrc.Position = nextElementPos = lastElementPos = 0; }
		public ElementInfo JumpToElementAt(Int64 elemPos) {
			dataSrc.Position = elemPos;
			nextElementPos = elemPos;
			lastElementPos = dataSrc.HasKnownLength ? dataSrc.Length : ~VIntConsts.UNKNOWN_LENGTH;

			return Next();
		}


		public ElementInfo Next() {
			if(nextElementPos == lastElementPos || nextElementPos == ~VIntConsts.UNKNOWN_LENGTH || dataSrc.EOF) return null;
			if(dataSrc.Position != nextElementPos) dataSrc.Position = nextElementPos;

			Int32 docElementId = dataSrc.ReadIdentifier();
			Int64 vintPos = dataSrc.Position;
			Int64 dataLength = dataSrc.ReadVInt();
			Int64 dataPos = dataSrc.Position;

			if((docElementId < 0 && (~docElementId & VIntConsts.ERROR) != 0) || (dataLength < 0 && (~dataLength & VIntConsts.ERROR) != 0)) {
				var dataError = DataError;
				if(dataError != null) {
					dataError(this, EventArgs.Empty);
					return new ElementInfo(EBMLDocElement.Unknown, nextElementPos, vintPos, dataPos, 0);
				} else throw new EBMLDataException("File most likely Corrupted");
			}

			var docElem = docType.RetrieveDocElement(docElementId);
			var elemInfo = new ElementInfo(docElem, nextElementPos, vintPos, dataPos, dataLength < 0 ? (Int64?)null : dataLength);

			nextElementPos = dataLength < 0 ? ~VIntConsts.UNKNOWN_LENGTH : dataPos + dataLength;

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

		protected sealed class PreviousState : IDisposable {
			public event EventHandler Disposed = delegate { };
			public void Dispose() { Disposed(this, EventArgs.Empty); Disposed = null; }

			public Int64 NextElementPos, LastElementPos;
		}
	}
}
