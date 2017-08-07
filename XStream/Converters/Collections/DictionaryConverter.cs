using System;
using System.Collections;
using System.Collections.Generic;

namespace xstream.Converters.Collections {
    internal class DictionaryConverter : BaseDictionaryConverter<Dictionary<object, object>> {
        protected override IDictionary EmptyDictionary(XStreamReader reader, UnmarshallingContext context)
        {
            return (IDictionary)DynamicInstanceBuilder.CreateInstance(context.currentTargetType);

            return (IDictionary) DynamicInstanceBuilder.CreateInstance(Type.GetType(reader.GetAttribute(Attributes.classType)));
        }
    }
}