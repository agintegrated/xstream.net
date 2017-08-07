using System;
using System.Collections.Generic;
using System.Reflection;
using xstream.Converters;

namespace xstream {
    public class UnmarshallingContext {
        private readonly Dictionary<string, object> alreadyDeserialised = new Dictionary<string, object>();
        private readonly XStreamReader reader;
        private readonly ConverterLookup converterLookup;
        private readonly Aliases aliases;
        private readonly List<Assembly> assemblies;

        public Type currentTargetType = null;

        internal UnmarshallingContext(XStreamReader reader, ConverterLookup converterLookup, Aliases aliases, List<Assembly> assemblies) {
            this.reader = reader;
            this.converterLookup = converterLookup;
            this.aliases = aliases;
            this.assemblies = assemblies;
        }

        public object ConvertAnother() {
            string nullAttribute = reader.GetAttribute(Attributes.Null);
            if (nullAttribute != null && nullAttribute == "true")
            {
                return null;
            }

            object result = Find();
            if (result != null)
            {
                return result;
            }

            Converter converter = converterLookup.GetConverter(reader.GetNodeName());
            if (converter == null) return ConvertOriginal();
            return converter.FromXml(reader, this);
        }

        internal object ConvertAnother(Type elementType)
        {
            string nullAttribute = reader.GetAttribute(Attributes.Null);
            if (nullAttribute != null && nullAttribute == "true")
            {
                return null;
            }

            object result = Find();
            if (result != null)
            {
                return result;
            }

            Converter converter = converterLookup.GetConverter(elementType);
            if (converter == null) return ConvertOriginal(elementType);
            return converter.FromXml(reader, this);
        }

        public object ConvertOriginal() {
            string nodeName = reader.GetNodeName();
            Type type = TypeToUse(nodeName);
            return ConvertOriginal(type);
        }

        public object ConvertOriginal(Type type)
        {
            Converter converter = converterLookup.GetConverter(type);
            if (converter != null) return converter.FromXml(reader, this);
            return new Unmarshaller(reader, this, converterLookup).Unmarshal(type);
        }

        private Type TypeToUse(string nodeName) {
            foreach (Alias alias in aliases) {
                Type type;
                if (alias.TryGetType(nodeName, out type))
                {
                    return type;
                }
            }
            string typeName = reader.GetAttribute(Attributes.classType);

            if (typeName == "")
            {
                typeName = nodeName;
            }

            Type returnType = GetTypeFromOtherAssemblies(typeName);

            if(returnType == null)
            {
                char[] a = typeName.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                typeName = new string(a);
                returnType = GetTypeFromOtherAssemblies(typeName);
            }

            return returnType;
        }

        internal Type GetTypeFromOtherAssemblies(string typeName) {

            if(typeName == "")
            {
                return null;
            }

            Type type = Type.GetType(typeName);
            int indexOfComma = typeName.IndexOf(',');
            if (type == null) {
                string assemblyName = String.Empty;
                string actualTypeName = typeName;
                if (indexOfComma > 0)
                {
                    assemblyName = typeName.Substring(indexOfComma + 2);
                    actualTypeName = typeName.Substring(0, indexOfComma);
                }
                
                foreach (Assembly assembly in assemblies) {
                    if (assemblyName.Equals(assembly.FullName) || assemblyName.Equals(string.Empty))
                    {
                        type = assembly.GetType(actualTypeName);
                    }

                    if (type != null)
                    {
                        break;
                    }
                }
                if (type == null) throw new ConversionException("Couldn't deserialise from " + typeName);
            }
            return type;
        }

        public void StackObject(object value) {
            // NOTE:  Because this reader is not streaming, we will somtimes hit a reference before the id is defined.
            //  Check for either 'id' or 'reference' when adding
            string idReferenceAttribute = reader.GetAttribute(Attributes.id);
            if (string.IsNullOrEmpty(idReferenceAttribute))
            {
                idReferenceAttribute = reader.GetAttribute(Attributes.reference);
            }

            try {
                if (!string.IsNullOrEmpty(idReferenceAttribute))
                {
                    alreadyDeserialised.Add(idReferenceAttribute, value);
                }
                else
                {
                    alreadyDeserialised[reader.CurrentPath] = value;
                }
            }
            catch (ArgumentException e) {
                throw new ConversionException(string.Format("Couldn't add path:{0}, value: {1}", reader.CurrentPath, value), e);
            }
        }

        public object Find() {
            // NOTE:  Because this reader is not streaming, we will somtimes hit a reference before the id is defined.
            //  Check for either 'id' or 'reference' when doing a lookup
            string idReferenceAttribute = reader.GetAttribute(Attributes.reference);
            if (string.IsNullOrEmpty(idReferenceAttribute))
            {
                idReferenceAttribute = reader.GetAttribute(Attributes.id);
            }

            if (!string.IsNullOrEmpty(idReferenceAttribute))
            {
                if (alreadyDeserialised.ContainsKey(idReferenceAttribute))
                {
                    return alreadyDeserialised[idReferenceAttribute];
                }
            }

            string referencesAttribute = reader.GetAttribute(Attributes.references);
            if (!string.IsNullOrEmpty(referencesAttribute)) 
            {
                return alreadyDeserialised[referencesAttribute];
            }
            return null;
        }
    }
}