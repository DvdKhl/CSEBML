using CSEBML.DocTypes.EBML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSEBML.DocTypes {
    public abstract class BaseDocType {
        protected IDocType extension;
        protected Dictionary<Int32, EBMLDocElement> docElementMap;

        public BaseDocType(IDocType extension) {
            this.extension = extension;

			docElementMap = GetDocElements(this.GetType(), null).Concat(GetDocElements(extension.GetType(), null)).ToDictionary(item => item.Id);
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
