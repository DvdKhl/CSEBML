using System;
using System.Collections.Generic;
using System.Text;

namespace CSEBML.DocTypes.Matroska {
    public class MatroskaDocMetaElement {
		public Boolean IsMandatory { get; private set; }
		public Boolean Multiple { get; private set; }
		public Object DefaultValue { get; private set; }
		public Int32[] ParentIds { get; private set; }
		public Predicate<Object> RangeCheck { get; private set; }
		public Int32 Id { get; private set; }

        public MatroskaDocMetaElement(Int32 id, string options, Object defaultValue, Predicate<Object> rangeCheck, Int32[] parentIds, string description) {
        }

        internal bool CompatibleTo(MatroskaVersion version) {
            throw new NotImplementedException();
        }
    }
}
