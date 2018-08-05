using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Syroot.BinaryData;
using Syroot.NintenTools.Byaml.IO;

namespace Syroot.NintenTools.Byaml.Serialization
{
    /// <summary>
    /// Represents a BYAML parser capable of loading and storing strongly typed data.
    /// </summary>
    public class ByamlSerializer
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const ushort _magicBytes = 0x4259; // "BY"

        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private Dictionary<Type, ByamlObjectInfo> _byamlObjectInfos;
        private Dictionary<object, Dictionary<string, object>> _customMembers;
        private object _instance;

        private List<string> _nameArray;
        private List<string> _stringArray;
        private List<List<ByamlPathPoint>> _pathArray;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ByamlSerializer"/> class with default serialization behavior.
        /// </summary>
        public ByamlSerializer() : this(new ByamlSerializerSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByamlSerializer"/> class with the given
        /// <paramref name="settings"/> controlling the serialization behavior.
        /// </summary>
        /// <param name="settings">The <see cref="ByamlSerializerSettings"/> instance controlling the serialization
        /// behavior.</param>
        public ByamlSerializer(ByamlSerializerSettings settings)
        {
            _byamlObjectInfos = new Dictionary<Type, ByamlObjectInfo>();

            Settings = settings;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="ByamlSerializerSettings"/> controlling the serialization behavior.
        /// </summary>
        public ByamlSerializerSettings Settings { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Deserializes the BYAML data from the file with the given <paramref name="fileName"/> into the given instance
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data to deserialize.</typeparam>
        /// <param name="fileName">The name of the file providing the data to deserialize.</param>
        /// <param name="instance">The instance of type <typeparamref name="T"/> to deserialize the data into.</param>
        public void Deserialize<T>(string fileName, T instance)
        {
            _instance = instance;
            Deserialize<T>(fileName);
        }

        /// <summary>
        /// Deserializes the BYAML data from the given <paramref name="stream"/> into the given instance of type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data to deserialize.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> providing the data to deserialize.</param>
        /// <param name="instance">The instance of type <typeparamref name="T"/> to deserialize the data into.</param>
        public void Deserialize<T>(Stream stream, T instance)
        {
            _instance = instance;
            Deserialize<T>(stream);
        }

        /// <summary>
        /// Deserializes the BYAML data from the file with the given <paramref name="fileName"/> and returns the new
        /// deserialized instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data to deserialize.</typeparam>
        /// <param name="fileName">The name of the file providing the data to deserialize.</param>
        /// <returns>The deserialized instance of type <typeparamref name="T"/>.</returns>
        public T Deserialize<T>(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Deserialize<T>(stream);
            }
        }

        /// <summary>
        /// Deserializes the BYAML data from the given <paramref name="stream"/> and returns thew new deserialized
        /// instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data to deserialize.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> providing the data to deserialize.</param>
        /// <returns>The deserialized instance of type <typeparamref name="T"/>.</returns>
        public T Deserialize<T>(Stream stream)
        {
            // Open a reader on the given stream.
            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                reader.ByteOrder = Settings.ByteOrder;
                return (T)Read(reader, typeof(T));
            }
        }

        /// <summary>
        /// Serializes the given <paramref name="obj"/> and stores it in the file with the specified
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file in which the data will be stored.</param>
        /// <param name="obj">The instance to store.</param>
        public void Serialize(string fileName, object obj)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Serialize(stream, obj);
            }
        }

        /// <summary>
        /// Serializes the given <paramref name="obj"/> and stores it in the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> in which the data will be stored.</param>
        /// <param name="obj">The instance to store.</param>
        public void Serialize(Stream stream, object obj)
        {
            // Generate the name, string and path arrays.
            _nameArray = new List<string>();
            _stringArray = new List<string>();
            if (Settings.SupportPaths)
            {
                _pathArray = new List<List<ByamlPathPoint>>();
            }

            _customMembers = new Dictionary<object, Dictionary<string, object>>();
            CollectArrayContents(obj);
            _nameArray = _nameArray.Distinct().ToList();
            _nameArray.Sort(StringComparer.Ordinal);
            _stringArray = _stringArray.Distinct().ToList();
            _stringArray.Sort(StringComparer.Ordinal);

            // Open a reader on the given stream.
            using (BinaryDataWriter writer = new BinaryDataWriter(stream, true))
            {
                writer.ByteOrder = Settings.ByteOrder;
                Write(writer, obj);
            }
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        // ---- Deserialization ----

        private object Read(BinaryDataReader reader, Type type)
        {
            // Load the header which specifies magic bytes and BYAML version.
            if (reader.ReadUInt16() != _magicBytes)
            {
                throw new ByamlException("Invalid BYAML header.");
            }
            ushort version = reader.ReadUInt16();
            if (version != (ushort)Settings.Version)
            {
                throw new ByamlException($"Unexpected BYAML version '{version}'.");
            }

            // Read the main node offsets.
            uint nameArrayOffset = reader.ReadUInt32();
            uint stringArrayOffset = reader.ReadUInt32();
            uint pathArrayOffset = 0;
            if (Settings.SupportPaths)
            {
                pathArrayOffset = reader.ReadUInt32();
            }
            uint rootNodeOffset = reader.ReadUInt32();

            // Read the name array, holding strings referenced by index for the names of other nodes.
            reader.Seek(nameArrayOffset, SeekOrigin.Begin);
            _nameArray = (List<string>)ReadValue(reader, typeof(List<string>));

            // Read the optional string array, holding strings referenced by index in string nodes.
            if (stringArrayOffset != 0)
            {
                reader.Seek(stringArrayOffset, SeekOrigin.Begin);
                _stringArray = (List<string>)ReadValue(reader, typeof(List<string>));
            }

            // Read the optional path array, holding paths referenced by index in path nodes.
            if (Settings.SupportPaths && pathArrayOffset != 0)
            {
                reader.Seek(pathArrayOffset, SeekOrigin.Begin);
                _pathArray = (List<List<ByamlPathPoint>>)ReadValue(reader, typeof(List<List<ByamlPathPoint>>));
            }

            // Read the root node.
            reader.Seek(rootNodeOffset, SeekOrigin.Begin);
            return ReadValue(reader, type);
        }

        private object ReadValue(BinaryDataReader reader, Type type, ByamlNodeType nodeType = 0)
        {
            // Read the node type if it has not been provided from an array or dictionary.
            bool nodeTypeGiven = nodeType != 0;
            if (!nodeTypeGiven) nodeType = (ByamlNodeType)reader.ReadByte();
            if (nodeType >= ByamlNodeType.Array && nodeType <= ByamlNodeType.PathArray)
            {
                // Get the length of arrays. If the node type was given, the array value is read from an offset.
                long? oldPos = null;
                if (nodeTypeGiven)
                {
                    uint offset = reader.ReadUInt32();
                    oldPos = reader.Position;
                    reader.Seek(offset, SeekOrigin.Begin);
                }
                else
                {
                    reader.Seek(-1);
                }
                int length = (int)Get3LsbBytes(reader.ReadUInt32());
                // Read the array data.
                object value = null;
                switch (nodeType)
                {
                    case ByamlNodeType.Array:
                        value = ReadArray(reader, type, length);
                        break;
                    case ByamlNodeType.Dictionary:
                        value = ReadDictionary(reader, type, length);
                        break;
                    case ByamlNodeType.StringArray:
                        value = ReadStringArray(reader, type, length);
                        break;
                    case ByamlNodeType.PathArray:
                        if (!Settings.SupportPaths) throw new ByamlException("Path found, but set to be unsupported.");
                        value = ReadPathArray(reader, type, length);
                        break;
                }
                // Seek back to the previous position if this was a value read from an offset.
                if (oldPos.HasValue) reader.Seek(oldPos.Value, SeekOrigin.Begin);
                return value;
            }
            else
            {
                // Read the following uint which is representing the value directly.
                switch (nodeType)
                {
                    case ByamlNodeType.StringIndex:
                        return _stringArray[reader.ReadInt32()];
                    case ByamlNodeType.PathIndex:
                        if (!Settings.SupportPaths) throw new ByamlException("Path found, but set to be unsupported.");
                        return _pathArray[reader.ReadInt32()];
                    case ByamlNodeType.Boolean:
                        return reader.ReadInt32() != 0;
                    case ByamlNodeType.Integer:
                        return reader.ReadInt32();
                    case ByamlNodeType.Float:
                        return reader.ReadSingle();
                    case ByamlNodeType.Null:
                        int value = reader.ReadInt32();
                        if (value != 0) throw new ByamlException($"Null node has unexpected value of {value}.");
                        return null;
                    default:
                        throw new ByamlException($"Invalid BYAML node type {nodeType}.");
                }
            }
        }
        
        private IList ReadArray(BinaryDataReader reader, Type type, int length)
        {
            // Use a given instance as the root array if available.
            IList array;
            if (_instance == null)
            {
                array = InstantiateType<IList>(type);
            }
            else
            {
                array = (IList)_instance;
                _instance = null;
            }

            // Find the generic element type by looking at the indexer (this allows IList-inheriting classes to work).
            Type elementType = type.GetTypeInfo().GetElementType();

            // Read the element types of the array. All elements must be of same type or serialization is impossible.
            byte[] nodeTypes = reader.ReadBytes(length);
            // Read the elements, which begin after a padding to the next 4 bytes.
            reader.Align(4);
            for (int i = 0; i < length; i++)
            {
                array.Add(ReadValue(reader, elementType, (ByamlNodeType)nodeTypes[i]));
            }

            return array;
        }

        private object ReadDictionary(BinaryDataReader reader, Type type, int length)
        {
            // Get the information required to serialize this type (for Nullables take the underlying type).
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) type = nullableType;
            ByamlObjectInfo objectInfo;
            if (!_byamlObjectInfos.TryGetValue(type, out objectInfo))
            {
                objectInfo = new ByamlObjectInfo(type);
                _byamlObjectInfos.Add(type, objectInfo);
            }

            // Instantiate the type and read in the elements. Use a given instance as the root if available.
            object instance;
            if (_instance == null)
            {
                instance = Activator.CreateInstance(type, true);
            }
            else
            {
                instance = _instance;
                _instance = null;
            }
            // Collect them in a dictionary for custom deserialization.
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            for (int i = 0; i < length; i++)
            {
                uint indexAndType = reader.ReadUInt32();
                int nodeNameIndex = (int)Get3MsbBytes(indexAndType);
                ByamlNodeType nodeType = (ByamlNodeType)Get1MsbByte(indexAndType);
                string key = _nameArray[nodeNameIndex];
                // Find a member for it to map the value to.
                object value;
                ByamlMemberInfo member;
                if (objectInfo.Members.TryGetValue(key, out member))
                {
                    // The key could be mapped to a member, read it as the member's type.
                    value = ReadValue(reader, member.Type, nodeType);
                    member.SetValue(instance, value);
                }
                else
                {
                    // If the key could not be mapped to a member, add it to the dictionary for custom deserialization.
                    //Debug.WriteLine($"No member in {type.Name} found to map key \"{key}\" to.");
                    value = ReadValue(reader, nodeType.GetInstanceType(), nodeType);
                }
                dictionary.Add(key, value);
            }

            // Call IByamlSerializable methods if the interface was implemented.
            if (objectInfo.ImplementsByamlSerializable)
            {
                IByamlSerializable byamlSerializable = (IByamlSerializable)instance;
                byamlSerializable.DeserializeByaml(dictionary);
            }

            // Check if any required fields were not filled.
            foreach (ByamlMemberInfo member in objectInfo.Members.Values)
            {
                if (!member.Optional && member.GetValue(instance) == null)
                {
                    throw new ByamlException(
                        $"Member {type.Name}.{member.MemberInfo.Name} is not optional, but has not been deserialized.");
                }
            }

            return instance;
        }

        private IList<string> ReadStringArray(BinaryDataReader reader, Type type, int length)
        {
            IList<string> stringArray = InstantiateType<IList<string>>(type);

            // Read the element offsets.
            long nodeOffset = reader.Position - 4; // String offsets are relative to the start of node.
            uint[] offsets = reader.ReadUInt32s(length);

            // Read the strings by seeking to their element offset and then back.
            long oldPosition = reader.Position;
            for (int i = 0; i < length; i++)
            {
                reader.Seek(nodeOffset + offsets[i], SeekOrigin.Begin);
                stringArray.Add(reader.ReadString(BinaryStringFormat.ZeroTerminated));
            }
            reader.Seek(oldPosition, SeekOrigin.Begin);

            return stringArray;
        }

        private IList<List<ByamlPathPoint>> ReadPathArray(BinaryDataReader reader, Type type, int length)
        {
            IList<List<ByamlPathPoint>> pathArray = InstantiateType<IList<List<ByamlPathPoint>>>(type);
            Type elementType = type.GenericTypeArguments[0];

            // Read the element offsets.
            long nodeOffset = reader.Position - 4; // Path offsets are relative to the start of node.
            uint[] offsets = reader.ReadUInt32s(length + 1);

            // Read the paths by seeking to their element offset and then back.
            long oldPosition = reader.Position;
            for (int i = 0; i < length; i++)
            {
                reader.Seek(nodeOffset + offsets[i], SeekOrigin.Begin);
                int pointCount = (int)((offsets[i + 1] - offsets[i]) / 0x1C);
                pathArray.Add(ReadPath(reader, elementType, pointCount));
            }
            reader.Seek(oldPosition, SeekOrigin.Begin);

            return pathArray;
        }

        private List<ByamlPathPoint> ReadPath(BinaryDataReader reader, Type type, int length)
        {
            List<ByamlPathPoint> path = InstantiateType<List<ByamlPathPoint>>(type);
            for (int i = 0; i < length; i++)
            {
                ByamlPathPoint point = new ByamlPathPoint();
                point.Position = reader.ReadVector3F();
                point.Normal = reader.ReadVector3F();
                point.Unknown = reader.ReadUInt32();
                path.Add(point);
            }
            return path;
        }
        
        private T InstantiateType<T>(Type type)
        {
            // Validate if the given type is compatible with the required one.
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(type))
            {
                throw new ByamlException($"Type {type.Name} cannot be used as BYAML object data.");
            }
            // Return a new instance.
            return (T)Activator.CreateInstance(type, true);
        }
        
        // ---- Serialization ----

        private void CollectArrayContents(object obj)
        {
            // Put strings into the string array.
            if (obj is string)
            {
                _stringArray.Add((string)obj);
                return;
            }

            // Put paths into the path array (if supported).
            if (Settings.SupportPaths)
            {
                if (obj is List<ByamlPathPoint>)
                {
                    _pathArray.Add((List <ByamlPathPoint>)obj);
                    return;
                }
            }

            // Traverse through arrays if the element type is of interest.
            if (obj is IList)
            {
                IList objArray = (IList)obj;
                Type elementType = objArray.GetType().GetTypeInfo().GetElementType();
                if (elementType == typeof(string) || elementType is IList || IsTypeByamlObject(elementType))
                {
                    foreach (object element in objArray)
                    {
                        CollectArrayContents(element);
                    }
                }
                return;
            }

            // Traverse through other types decorated with the ByamlObjectAttribute and collect their keys and values.
            Type type = obj.GetType();
            if (IsTypeByamlObject(type))
            {
                ByamlObjectInfo objectInfo;
                if (!_byamlObjectInfos.TryGetValue(type, out objectInfo))
                {
                    objectInfo = new ByamlObjectInfo(type);
                    _byamlObjectInfos.Add(type, objectInfo);
                }
                // Query the custom values and remember them for when to actually store them in the BYAML.
                if (objectInfo.ImplementsByamlSerializable)
                {
                    IByamlSerializable byamlSerializable = (IByamlSerializable)obj;
                    Dictionary<string, object> customMembers = new Dictionary<string, object>();
                    byamlSerializable.SerializeByaml(customMembers);
                    foreach (KeyValuePair<string, object> customMember in customMembers)
                    {
                        _nameArray.Add(customMember.Key);
                        CollectArrayContents(customMember.Value);
                    }
                    if (customMembers.Count > 0)
                    {
                        _customMembers.Add(obj, customMembers);
                    }
                }
                // Go through the default members.
                foreach (KeyValuePair<string, ByamlMemberInfo> member in objectInfo.Members)
                {
                    object value = member.Value.GetValue(obj);
                    if (value != null || !member.Value.Optional)
                    {
                        _nameArray.Add(member.Key);
                        if (value != null)
                        {
                            CollectArrayContents(value);
                        }
                    }
                }
                return;
            }
        }

        private void Write(BinaryDataWriter writer, object obj)
        {
            // Write the header, specifying magic bytes, version and main node offsets.
            writer.Write(_magicBytes);
            writer.Write((short)Settings.Version);
            Offset nameArrayOffset = writer.ReserveOffset();
            Offset stringArrayOffset = writer.ReserveOffset();
            Offset pathArrayOffset = Settings.SupportPaths ? writer.ReserveOffset() : null;
            Offset rootOffset = writer.ReserveOffset();

            // Write the name array.
            WriteStringArray(writer, nameArrayOffset, _nameArray);

            // Write the string array.
            if (_stringArray.Count == 0)
            {
                writer.Write(0);
            }
            else
            {
                WriteStringArray(writer, stringArrayOffset, _stringArray);
            }

            // Write the path array (if requested).
            if (Settings.SupportPaths)
            {
                if (_pathArray.Count == 0)
                {
                    writer.Write(0);
                }
                else
                {
                    WritePathArray(writer, pathArrayOffset, _pathArray);
                }
            }

            // Write the root node.
            WriteArrayOrDictionary(writer, rootOffset, obj);
        }

        private void WriteStringArray(BinaryDataWriter writer, Offset offset, IList<string> stringArray)
        {
            // Satisfy the offset to the value in the BYAML file which must be 4-byte aligned.
            writer.Align(4);
            offset.Satisfy();

            WriteTypeAndElementCount(writer, ByamlNodeType.StringArray, stringArray.Count);

            // Write the offsets to the strings.
            int stringOffset = 4 + 4 * (stringArray.Count + 1); // Relative to node start + all uint32 offsets.
            foreach (string str in stringArray)
            {
                writer.Write(stringOffset);
                stringOffset += str.Length + 1;
            }
            writer.Write(stringOffset); // The last one points to the end of the last string.

            // Write the 0-terminated strings.
            foreach (string str in stringArray)
            {
                writer.Write(str, BinaryStringFormat.ZeroTerminated);
            }
        }

        private void WritePathArray(BinaryDataWriter writer, Offset offset, IList<List<ByamlPathPoint>> pathArray)
        {
            // Satisfy the offset to the value in the BYAML file which must be 4-byte aligned.
            writer.Align(4);
            offset.Satisfy();

            WriteTypeAndElementCount(writer, ByamlNodeType.PathArray, pathArray.Count);

            // Write the offsets to the paths.
            int pathOffset = 4 + 4 * (pathArray.Count + 1); // Relative to node start + all uint32 offsets.
            foreach (List<ByamlPathPoint> path in pathArray)
            {
                writer.Write(pathOffset);
                pathOffset += path.Count * ByamlPathPoint.SizeInBytes;
            }
            writer.Write(pathOffset); // The last one points to the end of the last path.

            // Write the paths.
            foreach (List<ByamlPathPoint> path in pathArray)
            {
                foreach (ByamlPathPoint point in path)
                {
                    writer.Write(point.Position);
                    writer.Write(point.Normal);
                    writer.Write(point.Unknown);
                }
            }
        }

        private void WriteArrayOrDictionary(BinaryDataWriter writer, Offset offset, object obj)
        {
            // Satisfy the offset to the value in the BYAML file which must be 4-byte aligned.
            writer.Align(4);
            offset.Satisfy();

            // Serialize the value as an array.
            if (obj is IList)
            {
                WriteArray(writer, (IList)obj);
                return;
            }

            // Serialize the value as a custom type.
            Type objType = obj.GetType();
            if (IsTypeByamlObject(objType))
            {
                WriteDictionary(writer, obj);
                return;
            }

            throw new ByamlException($"Type {objType.Name} is not supported as BYAML array or dictionary data.");
        }

        private void WriteArray(BinaryDataWriter writer, IList array)
        {
            WriteTypeAndElementCount(writer, ByamlNodeType.Array, array.Count);

            // Write the element types (which must be the same for each to be supported for serialization).
            ByamlNodeType nodeType = GetNodeType(array.GetType().GetTypeInfo().GetElementType());
            for (int i = 0; i < array.Count; i++)
            {
                writer.Write((byte)nodeType);
            }

            // Write the elements, which begin after a padding to the next 4 bytes.
            writer.Align(4);
            if (nodeType == ByamlNodeType.Array || nodeType == ByamlNodeType.Dictionary)
            {
                // Arrays or dictionaries are referenced by offsets pointing behind the array.
                Offset[] offsets = new Offset[array.Count];
                for (int i = 0; i < array.Count; i++)
                {
                    offsets[i] = writer.ReserveOffset();
                }
                // Behind the offsets, write the array or dictionary contents and satisfy the 4-byte aligned offsets.
                for (int i = 0; i < array.Count; i++)
                {
                    WriteArrayOrDictionary(writer, offsets[i], array[i]);
                }
            }
            else
            {
                // Primitive values are stored directly rather than being referenced by offsets.
                foreach (object element in array)
                {
                    WritePrimitiveType(writer, nodeType, element);
                }
            }
        }

        private void WriteDictionary(BinaryDataWriter writer, object obj)
        {
            // Create a string-object dictionary out of the members.
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            // Add the custom members if any have been created when collecting node contents previously.
            Dictionary<string, object> customMembers;
            if (_customMembers.TryGetValue(obj, out customMembers))
            {
                foreach (KeyValuePair<string, object> customMember in customMembers)
                {
                    dictionary.Add(customMember.Key, customMember.Value);
                }
            }
            // Add the ByamlMemberAttribute decorated members.
            ByamlObjectInfo objectInfo = _byamlObjectInfos[obj.GetType()];
            foreach (KeyValuePair<string, ByamlMemberInfo> member in objectInfo.Members)
            {
                object value = member.Value.GetValue(obj);
                if (value != null || !member.Value.Optional)
                {
                    dictionary.Add(member.Key, value);
                }
            }
            // Dictionaries need to be sorted ordinally by key.
            var sortedDict = dictionary.Values.Zip(dictionary.Keys, (Value, Key) => new { Key, Value })
                .OrderBy(x => x.Key, StringComparer.Ordinal).ToList();

            WriteTypeAndElementCount(writer, ByamlNodeType.Dictionary, dictionary.Count);

            // Write the key-value pairs.
            Dictionary<Offset, object> offsetElements = new Dictionary<Offset, object>();
            foreach (var keyValuePair in sortedDict)
            {
                string key = keyValuePair.Key;
                object element = keyValuePair.Value;

                // Get the index of the key string in the file's name array and write it together with the type.
                uint keyIndex = (uint)_nameArray.IndexOf(key);
                ByamlNodeType nodeType = element == null ? ByamlNodeType.Null : GetNodeType(element.GetType());
                if (Settings.ByteOrder == ByteOrder.BigEndian)
                {
                    writer.Write(keyIndex << 8 | (uint)nodeType);
                }
                else
                {
                    writer.Write(keyIndex | (uint)nodeType << 24);
                }

                // Write the elements. Complex types are just offsets, primitive types are directly written as values.
                if (nodeType == ByamlNodeType.Array || nodeType == ByamlNodeType.Dictionary)
                {
                    offsetElements.Add(writer.ReserveOffset(), element);
                }
                else
                {
                    WritePrimitiveType(writer, nodeType, element);
                }
            }

            // Write the array or dictionary elements and satisfy their offsets.
            foreach (KeyValuePair<Offset, object> offsetElement in offsetElements)
            {
                WriteArrayOrDictionary(writer, offsetElement.Key, offsetElement.Value);
            }
        }

        private void WriteTypeAndElementCount(BinaryDataWriter writer, ByamlNodeType type, int count)
        {
            uint value;
            if (Settings.ByteOrder == ByteOrder.BigEndian)
            {
                value = (uint)type << 24 | (uint)count;
            }
            else
            {
                value = (uint)type | (uint)count << 8;
            }
            writer.Write(value);
        }

        private void WritePrimitiveType(BinaryDataWriter writer, ByamlNodeType nodeType, object obj)
        {
            switch (nodeType)
            {
                case ByamlNodeType.StringIndex:
                    writer.Write(_stringArray.IndexOf((string)obj));
                    break;
                case ByamlNodeType.PathIndex:
                    writer.Write(_pathArray.IndexOf((List<ByamlPathPoint>)obj));
                    break;
                case ByamlNodeType.Boolean:
                    writer.Write((bool)obj ? 1 : 0);
                    break;
                case ByamlNodeType.Integer:
                    writer.Write((int)obj);
                    break;
                case ByamlNodeType.Float:
                    writer.Write((float)obj);
                    break;
                case ByamlNodeType.Null:
                    writer.Write(0);
                    break;
            }
        }

        private ByamlNodeType GetNodeType(Type type)
        {
            if (type == typeof(bool))
            {
                return ByamlNodeType.Boolean;
            }
            else if (type == typeof(int) || (type.GetTypeInfo().IsEnum && Enum.GetUnderlyingType(type) == typeof(int)))
            {
                return ByamlNodeType.Integer;
            }
            else if (type == typeof(float))
            {
                return ByamlNodeType.Float;
            }
            else if (type == typeof(string))
            {
                return ByamlNodeType.StringIndex;
            }
            else if (type == typeof(List<ByamlPathPoint>))
            {
                return ByamlNodeType.PathIndex;
            }
            else if (typeof(IList).GetTypeInfo().IsAssignableFrom(type))
            {
                return ByamlNodeType.Array;
            }
            else if (IsTypeByamlObject(type))
            {
                return ByamlNodeType.Dictionary;
            }

            throw new ByamlException($"Type {type.Name} is not supported as BYAML data.");
        }
        
        private bool IsTypeByamlObject(Type type)
        {
            return type.GetTypeInfo().GetCustomAttribute<ByamlObjectAttribute>(true) != null;
        }

        // ---- Helper methods ----

        private uint Get1MsbByte(uint value)
        {
            if (Settings.ByteOrder == ByteOrder.BigEndian)
            {
                return value & 0x000000FF;
            }
            else
            {
                return value >> 24;
            }
        }

        private uint Get3LsbBytes(uint value)
        {
            if (Settings.ByteOrder == ByteOrder.BigEndian)
            {
                return value & 0x00FFFFFF;
            }
            else
            {
                return value >> 8;
            }
        }

        private uint Get3MsbBytes(uint value)
        {
            if (Settings.ByteOrder == ByteOrder.BigEndian)
            {
                return value >> 8;
            }
            else
            {
                return value & 0x00FFFFFF;
            }
        }
    }
}
