using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TraceSpy
{
    public abstract class Serializable<T> : ICloneable where T : new()
    {
        public static string ConfigurationFilePath
        {
            get
            {
                return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), typeof(T).Namespace), typeof(T).Name + ".config");
            }
        }

        public static T DeserializeFromConfiguration()
        {
            return Deserialize(ConfigurationFilePath);
        }

        public static T Deserialize(string filePath)
        {
            return Deserialize(filePath, new T());
        }

        public static T Deserialize(string filePath, T defaultValue)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
                return defaultValue;

            try
            {
                using (XmlTextReader reader = new XmlTextReader(filePath))
                {
                    return Deserialize(reader, defaultValue);
                }
            }
#if DEBUG
            catch(Exception e)
            {
                System.Diagnostics.Trace.WriteLine("!!!Exception trying to deserialize file '" + filePath + "': " + e);
                return defaultValue;
            }
#else
            catch
            {
                return defaultValue;
            }
#endif
        }

        public static T Deserialize(Stream stream)
        {
            return Deserialize(stream, new T());
        }

        public static T Deserialize(Stream stream, T defaultValue)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(T));
                return (T)deserializer.Deserialize(stream);
            }
#if DEBUG
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("!!!Exception trying to deserialize stream: " + e);
                return defaultValue;
            }
#else
            catch
            {
                return defaultValue;
            }
#endif
        }

        public static T Deserialize(TextReader reader)
        {
            return Deserialize(reader, new T());
        }

        public static T Deserialize(TextReader reader, T defaultValue)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(T));
                return (T)deserializer.Deserialize(reader);
            }
#if DEBUG
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("!!!Exception trying to deserialize textreader: " + e);
                return defaultValue;
            }
#else
            catch
            {
                return defaultValue;
            }
#endif
        }

        public static T Deserialize(XmlReader reader)
        {
            return Deserialize(reader, new T());
        }

        public static T Deserialize(XmlReader reader, T defaultValue)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(T));
                return (T)deserializer.Deserialize(reader);
            }
#if DEBUG
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("!!!Exception trying to deserialize xmlreader: " + e);
                return defaultValue;
            }
#else
            catch
            {
                return defaultValue;
            }
#endif
        }

        public void SerializeToConfiguration()
        {
            Serialize(ConfigurationFilePath);
        }

        public void Serialize(XmlWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            XmlSerializer serializer = new XmlSerializer(GetType());
            serializer.Serialize(writer, this);
        }

        public void Serialize(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            XmlSerializer serializer = new XmlSerializer(GetType());
            serializer.Serialize(writer, this);
        }

        public void Serialize(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            XmlSerializer serializer = new XmlSerializer(GetType());
            serializer.Serialize(stream, this);
        }

        public void Serialize(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");

            string dir = Path.GetDirectoryName(filePath);
            if ((dir !=null) && (!Directory.Exists(dir)))
            {
                Directory.CreateDirectory(dir);
            }
            using (XmlTextWriter writer = new XmlTextWriter(filePath, Encoding.UTF8))
            {
                Serialize(writer);
            }
        }

        public T Clone()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream);
                stream.Position = 0;
                return Deserialize(stream);
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
