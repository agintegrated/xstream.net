using System;

namespace xstream.Converters.Collections {
    internal class ArrayConverter : Converter {
        private const string ARRAY_TYPE = "array-type";

        public bool CanConvert(Type type) {
            return type.IsArray;
        }

        public void ToXml(object value, XStreamWriter writer, MarshallingContext context) {
            Array array = (Array) value;
            string typeName = value.GetType().AssemblyQualifiedName;
            int lastIndexOfBrackets = typeName.LastIndexOf("[]");

            //  classType is not valid for cross platform usage
            //string arrayType = string.Concat(typeName.Substring(0, lastIndexOfBrackets), typeName.Substring(lastIndexOfBrackets + 2));
            //  writer.WriteAttribute(ARRAY_TYPE, arrayType);

            foreach (object o in array)
            {
                context.ConvertOriginal(o);
            }
        }

        public object FromXml(XStreamReader reader, UnmarshallingContext context) {
            int count = reader.NoOfChildren();

            // Use the actual data type we are deserializing to rather than inferring from xml metadata
            Type arrayType = context.currentTargetType;

            string arrayTypeName = reader.GetAttribute(ARRAY_TYPE);
            if (arrayTypeName != "")
            {
                arrayType = context.GetTypeFromOtherAssemblies(arrayTypeName);
            }

            if (arrayType.IsArray)
            {
                // Due to the way that the currentTargetType and the constructor works, we need to get the element type instead of the current type.
                //  This probably won't work well for arrays of arrays.
                arrayType = arrayType.GetElementType();
            }

            Array result = Array.CreateInstance(arrayType, count);

            if (count != 0) {
                reader.MoveDown();
                for (int i = 0; i < count; i++) {
                    result.SetValue(context.ConvertOriginal(), i);
                    reader.MoveNext();
                }
                reader.MoveUp();
            }
            return result;
        }
    }
}