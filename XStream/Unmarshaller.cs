using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using xstream.Converters;
using xstream.Utilities;

namespace xstream
{
    internal class Unmarshaller
    {
        private readonly XStreamReader reader;
        private readonly UnmarshallingContext context;
        private readonly ConverterLookup converterLookup;

        public Unmarshaller(XStreamReader reader, UnmarshallingContext context, ConverterLookup converterLookup)
        {
            this.reader = reader;
            this.context = context;
            this.converterLookup = converterLookup;
        }

        internal object Unmarshal(Type type)
        {
            if (reader.GetAttribute(Attributes.Null) == true.ToString())
            {
                return null;
            }

            object result = context.Find();
            if (result == null)
            {
                result = DynamicInstanceBuilder.CreateInstance(type);
                context.StackObject(result);
            }

            UnmarshalAs(result, type);
            return result;
        }

        private void UnmarshalAs(object result, Type type)
        {
            if (type.Equals(typeof(object)))
            {
                return;
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttributes(typeof(DontSerialiseAttribute), true).Length != 0) continue;
                if (field.GetCustomAttributes(typeof(XmlIgnoreAttribute), true).Length != 0) continue;
                if (typeof(MulticastDelegate).IsAssignableFrom(field.FieldType)) continue;
                Match autoPropertyMatch = Constants.AutoPropertyNamePattern.Match(field.Name);
                Match javaPropertyMatch = Constants.JavaInternalPropertyNamePattern.Match(field.Name);

                string fieldName = field.Name;
                if (autoPropertyMatch.Success)
                {
                    fieldName = autoPropertyMatch.Result("$1");

                }
                else if (javaPropertyMatch.Success)
                {
                    fieldName = javaPropertyMatch.Result("$1");
                }

                string reader_CurrentPath = reader.CurrentPath;
                if (reader.MoveDown(fieldName))
                {
                    field.SetValue(result, ConvertField(field.FieldType));
                    reader.MoveUp();

                    if (reader.CurrentPath != reader_CurrentPath)
                    {
                        Console.Error.WriteLine("Path exception " + field.Name);
                    }
                }
                else
                {
                    // Kept in place for use while debugging issues with missing fields
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        Console.Error.WriteLine("Skipping " + field.Name);
                    }
                }
            }

            PropertyInfo[] properties = type.GetProperties(Constants.BINDINGFlags);
            foreach (PropertyInfo property in properties)
            {
                // Only serialize propertied that are explicitly set
                if (property.GetCustomAttributes(typeof(SerialisedProperty), true).Length != 1) continue;

                string fieldName = property.Name;
                string reader_CurrentPath = reader.CurrentPath;
                if (reader.MoveDown(fieldName))
                {
                    property.SetValue(result, ConvertField(property.PropertyType));
                    reader.MoveUp();

                    if (reader.CurrentPath != reader_CurrentPath)
                    {
                        Console.Error.WriteLine("Path exception " + property.Name);
                    }
                }
                else
                {
                    // Kept in place for use while debugging issues with missing fields
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        Console.Error.WriteLine("Skipping " + property.Name);
                    }
                }
            }

            UnmarshalAs(result, type.BaseType);
        }

        private object ConvertField(Type fieldType)
        {
            string classAttribute = reader.GetAttribute(Attributes.classType);
            if (!string.IsNullOrEmpty(classAttribute)) fieldType = Type.GetType(Xmlifier.UnXmlify(classAttribute));
            Converter converter = converterLookup.GetConverter(fieldType);
            if (converter != null)
            {
                context.currentTargetType = fieldType;
                return converter.FromXml(reader, context);
            }
            else
            {
                return Unmarshal(fieldType);
            }
        }
    }
}