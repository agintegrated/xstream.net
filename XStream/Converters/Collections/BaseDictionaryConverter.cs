using System;
using System.Collections;

namespace xstream.Converters.Collections {
    internal abstract class BaseDictionaryConverter<T> : Converter where T : IDictionary {
        protected const string KEY = "key";
        private const string VALUE = "value";

        public bool CanConvert(Type type) {
            if (typeof (T).IsGenericType && type.IsGenericType)
                return type.GetGenericTypeDefinition().Equals(typeof (T).GetGenericTypeDefinition());
            return type.Equals(typeof (T));
        }

        public void ToXml(object value, XStreamWriter writer, MarshallingContext context) {
            IDictionary dictionary = (IDictionary) value;
            DoSpecificStuff(dictionary, writer);
            foreach (DictionaryEntry entry in dictionary) {
                writer.StartNode("entry");
                WriteNode(writer, context, BaseDictionaryConverter<Hashtable>.KEY, entry.Key);
                WriteNode(writer, context, BaseDictionaryConverter<Hashtable>.VALUE, entry.Value);
                writer.EndNode();
            }
        }

        protected virtual void DoSpecificStuff(IDictionary dictionary, XStreamWriter writer) {}

        public object FromXml(XStreamReader reader, UnmarshallingContext context) {
            IDictionary result = EmptyDictionary(reader, context);
            int count = reader.NoOfChildren();
            if (reader.MoveDown())
            {
                for (int i = 0; i < count; i++)
                {
                    if (reader.MoveDown())
                    {
                        object key = null, value = null;
                        GetKeyObject(context, ref key, reader);
                        reader.MoveNext();
                        GetValueObject(context, ref value, reader);
                        result.Add(key, value);
                        reader.MoveUp();
                    }
                    reader.MoveNext();
                }
                reader.MoveUp();
            }
            return result;
        }

        protected abstract IDictionary EmptyDictionary(XStreamReader reader, UnmarshallingContext context);

        private static void GetObject(UnmarshallingContext context, ref object key, ref object value, XStreamReader reader) {
            string nodeName = reader.GetNodeName();
            object o = context.ConvertOriginal();
            if (BaseDictionaryConverter<Hashtable>.KEY.Equals(nodeName)) key = o;
            else value = o;
        }

        private static void GetKeyObject(UnmarshallingContext context, ref object key, XStreamReader reader)
        {
            Type previousType = context.currentTargetType;
            Type keyType = context.currentTargetType.GenericTypeArguments[0];

            string nodeName = reader.GetNodeName();

            context.currentTargetType = keyType;
            key = context.ConvertOriginal(keyType);

            context.currentTargetType = previousType;
        }

        private static void GetValueObject(UnmarshallingContext context, ref object value, XStreamReader reader)
        {
            Type previousType = context.currentTargetType;
            Type valueType = context.currentTargetType.GenericTypeArguments[1];

            string nodeName = reader.GetNodeName();

            context.currentTargetType = valueType;
            value = context.ConvertOriginal(valueType);

            context.currentTargetType = previousType;
        }

        private static void WriteNode(XStreamWriter writer, MarshallingContext context, string node, object value) {
            writer.StartNode(node);
            Type type = value != null ? value.GetType() : typeof (object);
            //  classType is not valid for cross platform usage
            //  writer.WriteAttribute(Attributes.classType, type.AssemblyQualifiedName);
            context.ConvertAnother(value);
            writer.EndNode();
        }
    }
}