using System;

namespace xstream.Converters {
    internal class NullConverter : Converter {
        public bool CanConvert(Type type) {
            return false;
        }

        public void ToXml(object value, XStreamWriter writer, MarshallingContext context)
        {
            // Suppress null output
            //  writer.WriteAttribute(Attributes.Null, true.ToString());
        }

        public object FromXml(XStreamReader reader, UnmarshallingContext context) {
            return null;
        }
    }
}