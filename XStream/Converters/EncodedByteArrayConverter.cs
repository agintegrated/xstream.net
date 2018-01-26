using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xstream.Converters
{
    class EncodedByteArrayConverter : Converter
    {
        public bool CanConvert(Type type)
        {
            return type.Equals(typeof(byte[]));
        }

        public object FromXml(XStreamReader reader, UnmarshallingContext context)
        {
            byte[] bytes = null;

            int count = reader.NoOfChildren();
            if (reader.MoveDown("byte-array"))
            {
                string base64 = reader.GetValue();
                bytes = Convert.FromBase64String(base64);
                reader.MoveUp();
            }

            return bytes;
        }

        public void ToXml(object value, XStreamWriter writer, MarshallingContext context)
        {
            byte[] bytes = (byte[])value;
            string base64 = Convert.ToBase64String(bytes);

            writer.StartNode("byte-array");

            writer.SetValue(base64);

            writer.EndNode();
        }

    }
}
