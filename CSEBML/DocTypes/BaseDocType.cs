using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using CSEBML.DocTypes.EBML;
using System.Collections;
using System.Collections.ObjectModel;

namespace CSEBML.DocTypes {
    public abstract class BaseDocType {
        protected IDocType extension;
        protected DynKeyedCollection<Int32, EBMLDocElement> docElementMap;

        public BaseDocType(IDocType extension) {
            this.extension = extension;

            docElementMap = new DynKeyedCollection<int, EBMLDocElement>(docElem => docElem.Id,
				GetDocElements(this.GetType(), null).Concat(GetDocElements(extension.GetType(), null))
			);
        }

        private static IEnumerable<EBMLDocElement> GetDocElements(Type type, Predicate<EBMLDocElement> filter) {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
            foreach(var field in fields) {
                var obj = field.GetValue(null);
                if(obj is EBMLDocElement && (filter == null || filter((EBMLDocElement)obj))) yield return (EBMLDocElement)obj;
            }
        }


        public abstract EBMLDocElement RetrieveDocElement(int id);
        public abstract object RetrieveValue(EBMLDocElement docElem, byte[] data, long offset, long length);
        public abstract int MaxDocTypeReadVersion { get; }
	}
}
