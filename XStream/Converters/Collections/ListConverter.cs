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

                if (type.GenericTypeArguments.Length == 1)
                {
                    Type[] interfaces = genericType.GetInterfaces();
                    foreach (Type interfac in interfaces)
                    {
                        if (interfac == typeof(IList))
                        {
                            return true;
                        }
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
            
            Type elementType = null;

            if (context.currentTargetType.HasElementType)
            {
                elementType = context.currentTargetType.GetElementType();
            }
            else if (context.currentTargetType.IsGenericType)
            {
                elementType = context.currentTargetType.GenericTypeArguments[0];
            }
            else
            {
                throw new Exception("Unable to get element type for: " + context.currentTargetType.ToString());
            }

            Type previousType = context.currentTargetType;
            context.currentTargetType = elementType;

            int count = reader.NoOfChildren();
            for (int i = 0; i < count; i++)
            {
                if (reader.MoveDown())
                {
                    result.Add(context.ConvertAnother(elementType));
                    reader.MoveUp();
                }
                reader.MoveNext();
            }

            context.currentTargetType = previousType;

            return result;
        }
    }
}