using System;
using System.Collections;
using xstream.Converters;
using xstream.Utilities;

namespace xstream
{
    public class MarshallingContext
    {
        private readonly AlreadySerialisedDictionary alreadySerialised = new AlreadySerialisedDictionary();
        private readonly XStreamWriter writer;
        private readonly ConverterLookup converterLookup;
        private readonly Aliases aliases;

        internal MarshallingContext(XStreamWriter writer, ConverterLookup converterLookup, Aliases aliases)
        {
            this.writer = writer;
            this.converterLookup = converterLookup;
            this.aliases = aliases;
        }

        internal void ConvertAnother(object value)
        {
            if (value == null)
            {
                // TODO: Make this a configuration option
                // Don't export null values.
                return;
            }

            Converter converter = converterLookup.GetConverter(value);
            if (converter != null)
            {
                converter.ToXml(value, writer, this);
            }
            else
            {
                ConvertObject(value);
            }
        }

        private void ConvertObject(object value)
        {
            if (value == null)
            {
                // TODO: Make this a configuration option
                // Don't export null values.
                return;
            }

            if (alreadySerialised.ContainsKey(value))
            {
                // Reference by ID, not path
                writer.WriteAttribute(Attributes.reference, alreadySerialised[value]);
            }
            else
            {
                // Store the ID of the object
                int index = alreadySerialised.Count + 1;
                alreadySerialised.Add(value, index.ToString());

                // Write attribute for own id
                writer.WriteAttribute(Attributes.id, index.ToString());

                new Marshaller(writer, this).Marshal(value);
            }
        }

        public void ConvertOriginal(object value)
        {
            if (value == null)
            {
                // TODO: Make this a configuration option
                // Don't export null values.
                return;
            }

            StartNode(value);
            ConvertAnother(value);
            writer.EndNode();
        }

        private void StartNode(object value)
        {
            if (value == null)
            {
                Console.Write("NULL");
            }

            Type type = value != null ? value.GetType() : typeof(object);
            foreach (Alias alias in aliases)
            {
                string nodeAlias;
                if (alias.TryGetAlias(type, out nodeAlias))
                {
                    writer.StartNode(nodeAlias);
                    return;
                }
            }

            // TODO:  Look into refactoring the Xmlifier to be more cross-platform friendly
            if (value is IList)
            {
                writer.StartNode("list");
            }
            else if (value is String)
            {
                writer.StartNode("string");
            }
            else
            {
                writer.StartNode(Xmlifier.XmlifyNode(type));
            }
            //  classType is not valid for cross platform usage
            //writer.WriteAttribute(Attributes.classType, type.AssemblyQualifiedName);
        }
    }
}