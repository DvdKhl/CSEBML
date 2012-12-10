using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Collections.ObjectModel {
    public class DynKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem> {


        private Converter<TItem, TKey> keyRetriever;

        public DynKeyedCollection(Converter<TItem, TKey> keyRetriever, IEnumerable<TItem> items = null) {
            this.keyRetriever = keyRetriever;

			if(items == null) return;
            foreach(var item in items) Add(item);
        }

        protected override TKey GetKeyForItem(TItem item) { return keyRetriever(item); }

        public Boolean TryGetValue(TKey key, out TItem item) {
            if(Contains(key)) item = base[key]; else item = default(TItem);
            return item != null;
        }
    }
}
