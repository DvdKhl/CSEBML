using System;
using System.Collections.Generic;
using System.Text;

namespace CSEBML.DocTypes.Matroska {
    public class MatroskaDocMetaElement {
		public readonly Boolean IsMandatory;
		public readonly Boolean Multiple;
		public readonly Object DefaultValue;
		public readonly Int32[] ParentIds;
		public readonly Predicate<Object> RangeCheck;

        public MatroskaDocMetaElement(Int32 id, string options, Object defaultValue, Predicate<Object> rangeCheck, Int32[] parentIds, string description) {
        }

        internal bool CompatibleTo(MatroskaVersion version) {
            throw new NotImplementedException();
        }
    }
}
