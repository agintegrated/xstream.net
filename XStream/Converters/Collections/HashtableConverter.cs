using System.Collections;

namespace xstream.Converters.Collections {
    internal class HashtableConverter : BaseDictionaryConverter<Hashtable> {
        protected override IDictionary EmptyDictionary(XStreamReader reader, UnmarshallingContext context)
        {
            return new Hashtable();
        }
    }
}