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
		public string Description { get; private set; }
		public MatroskaVersion Versions { get; private set; }

		public MatroskaDocMetaElement(Int32 id, string options, Object defaultValue, Predicate<Object> rangeCheck, Int32[] parentIds, string description) {
			Id = id;
			IsMandatory = options[4] == 'M' && options[5] == 'a';
			Multiple = options[6] == 'M' && options[7] == 'u';
			DefaultValue = defaultValue;
			ParentIds = parentIds;
			RangeCheck = rangeCheck;
			Description = description;

			Versions =
				(options[0] == '1' ? MatroskaVersion.V1 : 0) |
				(options[1] == '2' ? MatroskaVersion.V2 : 0) |
				(options[2] == '3' ? MatroskaVersion.V3 : 0) |
				(options[3] == 'W' ? MatroskaVersion.WebM : 0);
		}

		internal bool CompatibleTo(MatroskaVersion version) {
			throw new NotImplementedException();
		}
	}
}
