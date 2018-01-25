using System;

namespace xstream.Converters {
    internal class EnumConverter : Converter {
        public bool CanConvert(Type type) {
            return type.IsEnum;
        }

        public void ToXml(object value, XStreamWriter writer, MarshallingContext context) {
            //  classType is not valid for cross platform usage
            //  writer.WriteAttribute(Attributes.AttributeType, value.GetType().AssemblyQualifiedName);
            writer.SetValue(value.ToString());
        }

        public object FromXml(XStreamReader reader, UnmarshallingContext context) {
            return Enum.Parse(context.currentTargetType, reader.GetValue());
        }
    }
}