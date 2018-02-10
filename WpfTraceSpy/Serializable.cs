using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TraceSpy
{
    public abstract class Serializable<T> where T : new()
    {
        public static string ConfigurationFilePath => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), typeof(T).Namespace), typeof(T).Name + ".config");
        public static T DeserializeFromConfiguration() => Deserialize(ConfigurationFilePath);
        public static T Deserialize(string filePath) => Deserialize(filePath, new T());
        public static T Deserialize(string filePath, T defaultValue)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                return defaultValue;

            try
            {
                using (var reader = new XmlTextReader(filePath))
                {
                    return Deserialize(reader, defaultValue);
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T Deserialize(Stream stream) => Deserialize(stream, new T());
        public static T Deserialize(Stream stream, T defaultValue)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                return (T)deserializer.Deserialize(stream);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T Deserialize(TextReader reader) => Deserialize(reader, new T());
        public static T Deserialize(TextReader reader, T defaultValue)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                return (T)deserializer.Deserialize(reader);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T Deserialize(XmlReader reader) => Deserialize(reader, new T());
        public static T Deserialize(XmlReader reader, T defaultValue)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                return (T)deserializer.Deserialize(reader);
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SerializeToConfiguration() => Serialize(ConfigurationFilePath);
        public void Serialize(XmlWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            var serializer = new XmlSerializer(GetType());
            serializer.Serialize(writer, this);
        }

        public void Serialize(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            var serializer = new XmlSerializer(GetType());
            serializer.Serialize(writer, this);
        }

        public void Serialize(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var serializer = new XmlSerializer(GetType());
            serializer.Serialize(stream, this);
        }

        public void Serialize(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            string dir = Path.GetDirectoryName(filePath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var writer = new XmlTextWriter(filePath, Encoding.UTF8))
            {
                Serialize(writer);
            }
        }
    }
}
