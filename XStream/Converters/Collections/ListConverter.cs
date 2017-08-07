using System;
using System.Collections;
using System.Collections.Generic;

namespace xstream.Converters.Collections {
    internal class ListConverter : Converter {
        private const string LIST_TYPE = "list-type";

        public bool CanConvert(Type type) {
            if(type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();

                Type[] interfaces = genericType.GetInterfaces();
                foreach(Type interfac in interfaces)
                {
                    if(interfac == typeof(IList))
                    {
                        return true;
                    }
                }
            }
            return typeof (ArrayList).Equals(type);
        }

        public void ToXml(object value, XStreamWriter writer, MarshallingContext context) {
            IList list = (IList) value;
            writer.WriteAttribute(LIST_TYPE, value.GetType().FullName);
            foreach (object o in list)
                context.ConvertOriginal(o);
        }

        public object FromXml(XStreamReader reader, UnmarshallingContext context) {
            IList result = (IList)DynamicInstanceBuilder.CreateInstance(context.currentTargetType);
            
            int count = reader.NoOfChildren();
            reader.MoveDown();
            for (int i = 0; i < count; i++) {
                result.Add(context.ConvertAnother());
                reader.MoveNext();
            }
            reader.MoveUp();
            return result;
        }
    }
}