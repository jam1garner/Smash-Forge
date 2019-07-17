using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Net.Sockets;
using System.Threading;

namespace Nintaco
{

    public delegate int AccessPointListener(int accessPointType, int address,
        int value);
    public delegate void ActivateListener();
    public delegate void ControllersListener();
    public delegate void DeactivateListener();
    public delegate void FrameListener();
    public delegate void ScanlineListener(int scanline);
    public delegate void ScanlineCycleListener(int scanline, int scanlineCycle,
        int address, bool rendering);
    public delegate void SpriteZeroListener(int scanline, int scanlineCycle);
    public delegate void StatusListener(string message);
    public delegate void StopListener();

    static class Colors
    {
        public const int Gray = 0x00;
        public const int DarkBlue = 0x01;
        public const int DarkIndigo = 0x02;
        public const int DarkViolet = 0x03;
        public const int DarkMagenta = 0x04;
        public const int DarkRed = 0x05;
        public const int DarkOrange = 0x06;
        public const int DarkBrown = 0x07;
        public const int DarkOlive = 0x08;
        public const int DarkChartreuse = 0x09;
        public const int DarkGreen = 0x0A;
        public const int DarkMint = 0x0B;
        public const int DarkCyan = 0x0C;
        public const int BlackerThanBlack = 0x0D;
        public const int Black3 = 0x0E;
        public const int Black = 0x0F;

        public const int LightGray = 0x10;
        public const int Blue = 0x11;
        public const int Indigo = 0x12;
        public const int Violet = 0x13;
        public const int Magenta = 0x14;
        public const int Red = 0x15;
        public const int Orange = 0x16;
        public const int Brown = 0x17;
        public const int Olive = 0x18;
        public const int Chartreuse = 0x19;
        public const int Green = 0x1A;
        public const int Mint = 0x1B;
        public const int Cyan = 0x1C;
        public const int Black2 = 0x1D;
        public const int Black4 = 0x1E;
        public const int Black5 = 0x1F;

        public const int White = 0x20;
        public const int LightBlue = 0x21;
        public const int LightIndigo = 0x22;
        public const int LightViolet = 0x23;
        public const int LightMagenta = 0x24;
        public const int LightRed = 0x25;
        public const int LightOrange = 0x26;
        public const int LightBrown = 0x27;
        public const int LightOlive = 0x28;
        public const int LightChartreuse = 0x29;
        public const int LightGreen = 0x2A;
        public const int LightMint = 0x2B;
        public const int LightCyan = 0x2C;
        public const int DarkGray = 0x2D;
        public const int Black6 = 0x2E;
        public const int Black7 = 0x2F;

        public const int White2 = 0x30;
        public const int PaleBlue = 0x31;
        public const int PaleIndigo = 0x32;
        public const int PaleViolet = 0x33;
        public const int PaleMagenta = 0x34;
        public const int PaleRed = 0x35;
        public const int PaleOrange = 0x36;
        public const int Cream = 0x37;
        public const int Yellow = 0x38;
        public const int PaleChartreuse = 0x39;
        public const int PaleGreen = 0x3A;
        public const int PaleMint = 0x3B;
        public const int PaleCyan = 0x3C;
        public const int PaleGray = 0x3D;
        public const int Black8 = 0x3E;
        public const int Black9 = 0x3F;
    }

    static class EventTypes
    {
        public const int Activate = 1;
        public const int Deactivate = 3;
        public const int Stop = 5;
        public const int Access = 9;
        public const int Controllers = 11;
        public const int Frame = 13;
        public const int Scanline = 15;
        public const int ScanlineCycle = 17;
        public const int SpriteZero = 19;
        public const int Status = 21;
    }

    public static class GamepadButtons
    {
        public const int A = 0;
        public const int B = 1;
        public const int Select = 2;
        public const int Start = 3;
        public const int Up = 4;
        public const int Down = 5;
        public const int Left = 6;
        public const int Right = 7;
    }

    static class AccessPointType
    {
        public const int PreRead = 0;
        public const int PostRead = 1;
        public const int PreWrite = 2;
        public const int PostWrite = 3;
        public const int PreExecute = 4;
        public const int PostExecute = 5;
    }

    class AccessPoint
    {

        public readonly AccessPointListener listener;
        public readonly int type;
        public readonly int minAddress;
        public readonly int maxAddress;
        public readonly int bank;

        public AccessPoint(AccessPointListener listener, int type, int minAddress)
            : this(listener, type, minAddress, -1, -1)
        {
        }

        public AccessPoint(AccessPointListener listener, int type,
            int minAddress, int maxAddress) : this(listener, type, minAddress,
                maxAddress, -1)
        {
        }

        public AccessPoint(AccessPointListener listener, int type, int minAddress,
            int maxAddress, int bank)
        {

            this.listener = listener;
            this.type = type;
            this.bank = bank;

            if (maxAddress < 0)
            {
                this.minAddress = this.maxAddress = minAddress;
            }
            else if (minAddress <= maxAddress)
            {
                this.minAddress = minAddress;
                this.maxAddress = maxAddress;
            }
            else
            {
                this.minAddress = maxAddress;
                this.maxAddress = minAddress;
            }
        }
    }

    class ScanlineCyclePoint
    {

        public readonly ScanlineCycleListener listener;
        public readonly int scanline;
        public readonly int scanlineCycle;

        public ScanlineCyclePoint(ScanlineCycleListener listener, int scanline,
            int scanlineCycle)
        {
            this.listener = listener;
            this.scanline = scanline;
            this.scanlineCycle = scanlineCycle;
        }
    }

    class ScanlinePoint
    {

        public readonly ScanlineListener listener;
        public readonly int scanline;

        public ScanlinePoint(ScanlineListener listener, int scanline)
        {
            this.listener = listener;
            this.scanline = scanline;
        }
    }

    public class DataStream
    {

        public const int ArrayLength = 1024;

        private readonly BinaryWriter writer;
        private readonly BinaryReader reader;

        public DataStream(BinaryWriter writer, BinaryReader reader)
        {
            this.writer = writer;
            this.reader = reader;
        }

        public void WriteByte(int value)
        {
            writer.Write((byte)value);
        }

        public int ReadByte()
        {
            return reader.ReadByte();
        }

        public void WriteInt(int value)
        {
            WriteByte(value >> 24);
            WriteByte(value >> 16);
            WriteByte(value >> 8);
            WriteByte(value);
        }

        public int ReadInt()
        {
            return (ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8)
                | ReadByte();
        }

        public void WriteIntArray(int[] array)
        {
            WriteInt(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                WriteInt(array[i]);
            }
        }

        public int ReadIntArray(int[] array)
        {
            int length = ReadInt();
            if (length < 0 || length > array.Length)
            {
                writer.Close();
                reader.Close();
                throw new IOException("Invalid array length: " + length);
            }
            for (int i = 0; i < length; i++)
            {
                array[i] = ReadInt();
            }
            return length;
        }

        public void WriteBoolean(bool value)
        {
            WriteByte(value ? 1 : 0);
        }

        public bool ReadBoolean()
        {
            return ReadByte() != 0;
        }

        public void WriteChar(char value)
        {
            WriteByte(value);
        }

        public char ReadChar()
        {
            return (char)ReadByte();
        }

        public void WriteCharArray(char[] array)
        {
            WriteInt(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                WriteChar(array[i]);
            }
        }

        public int ReadCharArray(char[] array)
        {
            int length = ReadInt();
            if (length < 0 || length > array.Length)
            {
                writer.Close();
                reader.Close();
                throw new IOException("Invalid array length: " + length);
            }
            for (int i = 0; i < length; i++)
            {
                array[i] = ReadChar();
            }
            return length;
        }

        public void WriteString(string value)
        {
            int length = value.Length;
            WriteInt(length);
            for (int i = 0; i < length; i++)
            {
                WriteChar(value[i]);
            }
        }

        public string ReadString()
        {
            int length = ReadInt();
            if (length < 0 || length > ArrayLength)
            {
                writer.Close();
                reader.Close();
                throw new IOException("Invalid array length: " + length);
            }
            char[] cs = new char[length];
            for (int i = 0; i < length; i++)
            {
                cs[i] = ReadChar();
            }
            return new String(cs);
        }

        public void WriteStringArray(string[] array)
        {
            WriteInt(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                WriteString(array[i]);
            }
        }

        public int ReadStringArray(String[] array)
        {
            int length = ReadInt();
            if (length < 0 || length > array.Length)
            {
                writer.Close();
                reader.Close();
                throw new IOException("Invalid array length: " + length);
            }
            for (int i = 0; i < length; i++)
            {
                array[i] = ReadString();
            }
            return length;
        }

        public String[] ReadDynamicStringArray()
        {
            int length = ReadInt();
            if (length < 0 || length > ArrayLength)
            {
                writer.Close();
                reader.Close();
                throw new IOException("Invalid array length: " + length);
            }
            String[] array = new String[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = ReadString();
            }
            return array;
        }

        public void Flush()
        {
            writer.Flush();
        }
    }

    class IdentityComparator : IEqualityComparer<object>
    {

        public new bool Equals(object x, object y)
        {
            return Object.ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    public static class ApiSource
    {

        public static RemoteApi api;

        public static void InitRemoteApi(String host, int port)
        {
            api = new RemoteApi(host, port);
        }
    }

    public abstract class RemoteBase
    {

        private readonly int[] eventTypes = {
      EventTypes.Activate,
      EventTypes.Deactivate,
      EventTypes.Stop,
      EventTypes.Access,
      EventTypes.Controllers,
      EventTypes.Frame,
      EventTypes.Scanline,
      EventTypes.ScanlineCycle,
      EventTypes.SpriteZero,
      EventTypes.Status,
    };

        public const int EventRequest = 0xFF;
        public const int EventResponse = 0xFE;
        public const int Heartbeat = 0xFD;
        public const int Ready = 0xFC;
        public const int RetryMillis = 1000;

        // listener -> listenerID
        protected readonly Dictionary<object, int> listenerIDs
            = new Dictionary<object, int>(new IdentityComparator());

        // eventType -> listenerID -> listenerObject(listener)
        protected readonly Dictionary<int, Dictionary<int, object>> listenerObjects
            = new Dictionary<int, Dictionary<int, object>>();

        protected string host;
        protected int port;
        protected DataStream stream;
        protected int nextId;
        protected bool running;

        public RemoteBase(string host, int port)
        {
            this.host = host;
            this.port = port;
            foreach (int eventType in eventTypes)
            {
                listenerObjects[eventType] = new Dictionary<int, object>();
            }
        }

        public void Run()
        {
            if (running)
            {
                return;
            }
            else
            {
                running = true;
            }
            while (true)
            {
                FireStatusChanged("Connecting to {0}:{1}...", host, port);
                TcpClient socket;
                try
                {
                    socket = new TcpClient(host, port);
                    BufferedStream bs = new BufferedStream(socket.GetStream());
                    stream = new DataStream(new BinaryWriter(bs), new BinaryReader(bs));
                }
                catch
                {
                    FireStatusChanged("Failed to establish connection.");
                }
                if (stream != null)
                {
                    try
                    {
                        FireStatusChanged("Connection established.");
                        SendListeners();
                        SendReady();
                        while (true)
                        {
                            ProbeEvents();
                        }
                    }
                    catch
                    {
                        FireDeactivated();
                        FireStatusChanged("Disconnected.");
                    }
                    finally
                    {
                        stream = null;
                    }
                }
                Thread.Sleep(RetryMillis);
            }
        }

        protected void FireDeactivated()
        {
            foreach (object obj in new List<object>(listenerObjects[EventTypes
                .Deactivate].Values))
            {
                ((DeactivateListener)obj)();
            }
        }

        protected void FireStatusChanged(string message, params object[] ps)
        {
            string msg = String.Format(message, ps);
            foreach (object obj in new List<object>(listenerObjects[EventTypes.Status]
                .Values))
            {
                ((StatusListener)obj)(msg);
            }
        }

        protected void SendReady()
        {
            if (stream != null)
            {
                try
                {
                    stream.WriteByte(Ready);
                    stream.Flush();
                }
                catch
                {
                }
            }
        }

        protected void SendListeners()
        {
            foreach (KeyValuePair<int, Dictionary<int, object>> e1 in listenerObjects)
            {
                foreach (KeyValuePair<int, object> e2 in e1.Value)
                {
                    SendListener(e2.Key, e1.Key, e2.Value);
                }
            }
        }

        protected void ProbeEvents()
        {

            stream.WriteByte(EventRequest);
            stream.Flush();

            int eventType = stream.ReadByte();

            if (eventType == Heartbeat)
            {
                stream.WriteByte(EventResponse);
                stream.Flush();
                return;
            }

            int listenerId = stream.ReadInt();
            object obj = listenerObjects[eventType][listenerId];

            if (obj != null)
            {
                if (eventType == EventTypes.Access)
                {
                    int type = stream.ReadInt();
                    int address = stream.ReadInt();
                    int value = stream.ReadInt();
                    int result = ((AccessPoint)obj).listener(type, address, value);
                    stream.WriteByte(EventResponse);
                    stream.WriteInt(result);
                }
                else
                {
                    switch (eventType)
                    {
                        case EventTypes.Activate:
                            ((ActivateListener)obj)();
                            break;
                        case EventTypes.Deactivate:
                            ((DeactivateListener)obj)();
                            break;
                        case EventTypes.Stop:
                            ((StopListener)obj)();
                            break;
                        case EventTypes.Controllers:
                            ((ControllersListener)obj)();
                            break;
                        case EventTypes.Frame:
                            ((FrameListener)obj)();
                            break;
                        case EventTypes.Scanline:
                            ((ScanlinePoint)obj).listener(stream.ReadInt());
                            break;
                        case EventTypes.ScanlineCycle:
                            ((ScanlineCyclePoint)obj).listener(stream.ReadInt(),
                                stream.ReadInt(), stream.ReadInt(), stream.ReadBoolean());
                            break;
                        case EventTypes.SpriteZero:
                            ((SpriteZeroListener)obj)(stream.ReadInt(),
                                stream.ReadInt());
                            break;
                        case EventTypes.Status:
                            ((StatusListener)obj)(stream.ReadString());
                            break;
                        default:
                            throw new IOException("Unknown listener type: " + eventType);
                    }
                    stream.WriteByte(EventResponse);
                }
            }

            stream.Flush();
        }


        protected void SendListener(int listenerId, int eventType,
            object listenerObject)
        {
            if (stream != null)
            {
                try
                {
                    stream.WriteByte(eventType);
                    stream.WriteInt(listenerId);
                    switch (eventType)
                    {
                        case EventTypes.Access:
                            {
                                AccessPoint point = (AccessPoint)listenerObject;
                                stream.WriteInt(point.type);
                                stream.WriteInt(point.minAddress);
                                stream.WriteInt(point.maxAddress);
                                stream.WriteInt(point.bank);
                                break;
                            }
                        case EventTypes.Scanline:
                            {
                                ScanlinePoint point = (ScanlinePoint)listenerObject;
                                stream.WriteInt(point.scanline);
                                break;
                            }
                        case EventTypes.ScanlineCycle:
                            {
                                ScanlineCyclePoint point = (ScanlineCyclePoint)listenerObject;
                                stream.WriteInt(point.scanline);
                                stream.WriteInt(point.scanlineCycle);
                                break;
                            }
                    }
                    stream.Flush();
                }
                catch
                {
                }
            }
        }


        protected void AddListener(object listener, int eventType)
        {
            if (listener != null)
            {
                SendListener(AddListenerObject(listener, eventType), eventType,
                    listener);
            }
        }

        public void RemoveListener(object listener, int eventType,
            int methodValue)
        {
            if (listener != null)
            {
                int listenerId = RemoveListenerObject(listener, eventType);
                if (listenerId >= 0 && stream != null)
                {
                    try
                    {
                        stream.WriteByte(methodValue);
                        stream.WriteInt(listenerId);
                        stream.Flush();
                    }
                    catch
                    {
                    }
                }
            }
        }


        protected int AddListenerObject(object listener, int eventType)
        {
            return AddListenerObject(listener, eventType, listener);
        }

        protected int AddListenerObject(object listener, int eventType,
            object listenerObject)
        {
            int listenerId = nextId++;
            listenerIDs[listener] = listenerId;
            listenerObjects[eventType][listenerId] = listenerObject;
            return listenerId;
        }

        protected int RemoveListenerObject(object listener, int eventType)
        {

            Int32 listenerId;
            if (listenerIDs.TryGetValue(listener, out listenerId))
            {
                listenerIDs.Remove(listener);
                listenerObjects[eventType].Remove(listenerId);
                return listenerId;
            }
            else
            {
                return -1;
            }
        }

        public void AddActivateListener(ActivateListener listener)
        {
            AddListener(listener, EventTypes.Activate);
        }

        public void RemoveActivateListener(ActivateListener listener)
        {
            RemoveListener(listener, EventTypes.Activate, 2);
        }

        public void AddDeactivateListener(DeactivateListener listener)
        {
            AddListener(listener, EventTypes.Deactivate);
        }

        public void RemoveDeactivateListener(DeactivateListener listener)
        {
            RemoveListener(listener, EventTypes.Deactivate, 4);
        }

        public void AddStopListener(StopListener listener)
        {
            AddListener(listener, EventTypes.Stop);
        }

        public void RemoveStopListener(StopListener listener)
        {
            RemoveListener(listener, EventTypes.Stop, 6);
        }

        public void AddAccessPointListener(AccessPointListener listener,
            int accessPointType, int address)
        {
            AddAccessPointListener(listener, accessPointType, address, -1, -1);
        }

        public void AddAccessPointListener(AccessPointListener listener,
            int accessPointType, int minAddress, int maxAddress)
        {
            AddAccessPointListener(listener, accessPointType, minAddress, maxAddress,
                -1);
        }

        public void AddAccessPointListener(AccessPointListener listener,
            int accessPointType, int minAddress, int maxAddress,
                int bank)
        {

            if (listener != null)
            {
                AccessPoint point = new AccessPoint(listener, accessPointType,
                    minAddress, maxAddress, bank);
                SendListener(AddListenerObject(listener, EventTypes.Access, point),
                    EventTypes.Access, point);
            }
        }

        public void RemoveAccessPointListener(AccessPointListener listener)
        {
            RemoveListener(listener, EventTypes.Access, 10);
        }

        public void AddControllersListener(ControllersListener listener)
        {
            AddListener(listener, EventTypes.Controllers);
        }

        public void RemoveControllersListener(ControllersListener listener)
        {
            RemoveListener(listener, EventTypes.Controllers, 12);
        }

        public void AddFrameListener(FrameListener listener)
        {
            AddListener(listener, EventTypes.Frame);
        }

        public void RemoveFrameListener(FrameListener listener)
        {
            RemoveListener(listener, EventTypes.Frame, 14);
        }

        public void AddScanlineListener(ScanlineListener listener, int scanline)
        {

            if (listener != null)
            {
                ScanlinePoint point = new ScanlinePoint(listener, scanline);
                SendListener(AddListenerObject(listener, EventTypes.Scanline, point),
                    EventTypes.Scanline, point);
            }
        }

        public void RemoveScanlineListener(ScanlineListener listener)
        {
            RemoveListener(listener, EventTypes.Scanline, 16);
        }

        public void AddScanlineCycleListener(ScanlineCycleListener listener,
            int scanline, int scanlineCycle)
        {

            if (listener != null)
            {
                ScanlineCyclePoint point = new ScanlineCyclePoint(listener,
                    scanline, scanlineCycle);
                SendListener(AddListenerObject(listener, EventTypes.ScanlineCycle,
                    point), EventTypes.ScanlineCycle, point);
            }
        }

        public void RemoveScanlineCycleListener(
            ScanlineCycleListener listener)
        {
            RemoveListener(listener, EventTypes.ScanlineCycle, 18);
        }

        public void AddSpriteZeroListener(SpriteZeroListener listener)
        {
            AddListener(listener, EventTypes.SpriteZero);
        }

        public void RemoveSpriteZeroListener(SpriteZeroListener listener)
        {
            RemoveListener(listener, EventTypes.SpriteZero, 20);
        }

        public void AddStatusListener(StatusListener listener)
        {
            AddListener(listener, EventTypes.Status);
        }

        public void RemoveStatusListener(StatusListener listener)
        {
            RemoveListener(listener, EventTypes.Status, 22);
        }

        public void GetPixels(int[] pixels)
        {
            try
            {
                stream.WriteByte(119);
                stream.Flush();
                stream.ReadIntArray(pixels);
            }
            catch
            {
            }
        }
    }

    // THIS IS AN AUTOGENERATED CLASS. DO NOT MODIFY.
    public class RemoteApi : RemoteBase
    {

        public RemoteApi(string host, int port) : base(host, port)
        {
        }

        public void SetPaused(bool paused)
        {
            try
            {
                stream.WriteByte(23);
                stream.WriteBoolean(paused);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsPaused()
        {
            try
            {
                stream.WriteByte(24);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public int GetFrameCount()
        {
            try
            {
                stream.WriteByte(25);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public int GetA()
        {
            try
            {
                stream.WriteByte(26);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetA(int a)
        {
            try
            {
                stream.WriteByte(27);
                stream.WriteInt(a);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetS()
        {
            try
            {
                stream.WriteByte(28);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetS(int s)
        {
            try
            {
                stream.WriteByte(29);
                stream.WriteInt(s);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetPc()
        {
            try
            {
                stream.WriteByte(30);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetPc(int pc)
        {
            try
            {
                stream.WriteByte(31);
                stream.WriteInt(pc);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetX()
        {
            try
            {
                stream.WriteByte(32);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetX(int x)
        {
            try
            {
                stream.WriteByte(33);
                stream.WriteInt(x);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetY()
        {
            try
            {
                stream.WriteByte(34);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetY(int y)
        {
            try
            {
                stream.WriteByte(35);
                stream.WriteInt(y);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetP()
        {
            try
            {
                stream.WriteByte(36);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetP(int p)
        {
            try
            {
                stream.WriteByte(37);
                stream.WriteInt(p);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsN()
        {
            try
            {
                stream.WriteByte(38);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetN(bool n)
        {
            try
            {
                stream.WriteByte(39);
                stream.WriteBoolean(n);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsV()
        {
            try
            {
                stream.WriteByte(40);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetV(bool v)
        {
            try
            {
                stream.WriteByte(41);
                stream.WriteBoolean(v);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsD()
        {
            try
            {
                stream.WriteByte(42);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetD(bool d)
        {
            try
            {
                stream.WriteByte(43);
                stream.WriteBoolean(d);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsI()
        {
            try
            {
                stream.WriteByte(44);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetI(bool I)
        {
            try
            {
                stream.WriteByte(45);
                stream.WriteBoolean(I);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsZ()
        {
            try
            {
                stream.WriteByte(46);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetZ(bool z)
        {
            try
            {
                stream.WriteByte(47);
                stream.WriteBoolean(z);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsC()
        {
            try
            {
                stream.WriteByte(48);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetC(bool c)
        {
            try
            {
                stream.WriteByte(49);
                stream.WriteBoolean(c);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetPpUv()
        {
            try
            {
                stream.WriteByte(50);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetPpUv(int v)
        {
            try
            {
                stream.WriteByte(51);
                stream.WriteInt(v);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetPpUt()
        {
            try
            {
                stream.WriteByte(52);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetPpUt(int t)
        {
            try
            {
                stream.WriteByte(53);
                stream.WriteInt(t);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetPpUx()
        {
            try
            {
                stream.WriteByte(54);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetPpUx(int x)
        {
            try
            {
                stream.WriteByte(55);
                stream.WriteInt(x);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsPpUw()
        {
            try
            {
                stream.WriteByte(56);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetPpUw(bool w)
        {
            try
            {
                stream.WriteByte(57);
                stream.WriteBoolean(w);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetCameraX()
        {
            try
            {
                stream.WriteByte(58);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetCameraX(int scrollX)
        {
            try
            {
                stream.WriteByte(59);
                stream.WriteInt(scrollX);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetCameraY()
        {
            try
            {
                stream.WriteByte(60);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetCameraY(int scrollY)
        {
            try
            {
                stream.WriteByte(61);
                stream.WriteInt(scrollY);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetScanline()
        {
            try
            {
                stream.WriteByte(62);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public int GetDot()
        {
            try
            {
                stream.WriteByte(63);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public bool IsSpriteZeroHit()
        {
            try
            {
                stream.WriteByte(64);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetSpriteZeroHit(bool sprite0Hit)
        {
            try
            {
                stream.WriteByte(65);
                stream.WriteBoolean(sprite0Hit);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetScanlineCount()
        {
            try
            {
                stream.WriteByte(66);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void RequestInterrupt()
        {
            try
            {
                stream.WriteByte(67);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void AcknowledgeInterrupt()
        {
            try
            {
                stream.WriteByte(68);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int PeekCpu(int address)
        {
            try
            {
                stream.WriteByte(69);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public int ReadCpu(int address)
        {
            try
            {
                stream.WriteByte(70);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void WriteCpu(int address, int value)
        {
            try
            {
                stream.WriteByte(71);
                stream.WriteInt(address);
                stream.WriteInt(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int PeekCpu16(int address)
        {
            try
            {
                stream.WriteByte(72);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public int ReadCpu16(int address)
        {
            try
            {
                stream.WriteByte(73);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void WriteCpu16(int address, int value)
        {
            try
            {
                stream.WriteByte(74);
                stream.WriteInt(address);
                stream.WriteInt(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int PeekCpu32(int address)
        {
            try
            {
                stream.WriteByte(75);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public int ReadCpu32(int address)
        {
            try
            {
                stream.WriteByte(76);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void WriteCpu32(int address, int value)
        {
            try
            {
                stream.WriteByte(77);
                stream.WriteInt(address);
                stream.WriteInt(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int ReadPpu(int address)
        {
            try
            {
                stream.WriteByte(78);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void WritePpu(int address, int value)
        {
            try
            {
                stream.WriteByte(79);
                stream.WriteInt(address);
                stream.WriteInt(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int ReadPaletteRam(int address)
        {
            try
            {
                stream.WriteByte(80);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void WritePaletteRam(int address, int value)
        {
            try
            {
                stream.WriteByte(81);
                stream.WriteInt(address);
                stream.WriteInt(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int ReadOam(int address)
        {
            try
            {
                stream.WriteByte(82);
                stream.WriteInt(address);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void WriteOam(int address, int value)
        {
            try
            {
                stream.WriteByte(83);
                stream.WriteInt(address);
                stream.WriteInt(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool ReadGamepad(int gamepad, int button)
        {
            try
            {
                stream.WriteByte(84);
                stream.WriteInt(gamepad);
                stream.WriteInt(button);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void WriteGamepad(int gamepad, int button, bool value)
        {
            try
            {
                stream.WriteByte(85);
                stream.WriteInt(gamepad);
                stream.WriteInt(button);
                stream.WriteBoolean(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public bool IsZapperTrigger()
        {
            try
            {
                stream.WriteByte(86);
                stream.Flush();
                return stream.ReadBoolean();
            }
            catch
            {
            }
            return false;
        }

        public void SetZapperTrigger(bool zapperTrigger)
        {
            try
            {
                stream.WriteByte(87);
                stream.WriteBoolean(zapperTrigger);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetZapperX()
        {
            try
            {
                stream.WriteByte(88);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetZapperX(int x)
        {
            try
            {
                stream.WriteByte(89);
                stream.WriteInt(x);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetZapperY()
        {
            try
            {
                stream.WriteByte(90);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetZapperY(int y)
        {
            try
            {
                stream.WriteByte(91);
                stream.WriteInt(y);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void SetColor(int color)
        {
            try
            {
                stream.WriteByte(92);
                stream.WriteInt(color);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetColor()
        {
            try
            {
                stream.WriteByte(93);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void SetClip(int x, int y, int width, int height)
        {
            try
            {
                stream.WriteByte(94);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void ClipRect(int x, int y, int width, int height)
        {
            try
            {
                stream.WriteByte(95);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void ResetClip()
        {
            try
            {
                stream.WriteByte(96);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void CopyArea(int x, int y, int width, int height, int dx, int dy)
        {
            try
            {
                stream.WriteByte(97);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.WriteInt(dx);
                stream.WriteInt(dy);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            try
            {
                stream.WriteByte(98);
                stream.WriteInt(x1);
                stream.WriteInt(y1);
                stream.WriteInt(x2);
                stream.WriteInt(y2);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawOval(int x, int y, int width, int height)
        {
            try
            {
                stream.WriteByte(99);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawPolygon(int[] xPoints, int[] yPoints, int nPoints)
        {
            try
            {
                stream.WriteByte(100);
                stream.WriteIntArray(xPoints);
                stream.WriteIntArray(yPoints);
                stream.WriteInt(nPoints);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawPolyline(int[] xPoints, int[] yPoints, int nPoints)
        {
            try
            {
                stream.WriteByte(101);
                stream.WriteIntArray(xPoints);
                stream.WriteIntArray(yPoints);
                stream.WriteInt(nPoints);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawRect(int x, int y, int width, int height)
        {
            try
            {
                stream.WriteByte(102);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawRoundRect(int x, int y, int width, int height, int arcWidth,
            int arcHeight)
        {
            try
            {
                stream.WriteByte(103);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.WriteInt(arcWidth);
                stream.WriteInt(arcHeight);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void Draw3DRect(int x, int y, int width, int height, bool raised)
        {
            try
            {
                stream.WriteByte(104);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.WriteBoolean(raised);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawArc(int x, int y, int width, int height, int startAngle,
            int arcAngle)
        {
            try
            {
                stream.WriteByte(105);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.WriteInt(startAngle);
                stream.WriteInt(arcAngle);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void Fill3DRect(int x, int y, int width, int height, bool raised)
        {
            try
            {
                stream.WriteByte(106);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.WriteBoolean(raised);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void FillArc(int x, int y, int width, int height, int startAngle,
            int arcAngle)
        {
            try
            {
                stream.WriteByte(107);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.WriteInt(startAngle);
                stream.WriteInt(arcAngle);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void FillOval(int x, int y, int width, int height)
        {
            try
            {
                stream.WriteByte(108);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void FillPolygon(int[] xPoints, int[] yPoints, int nPoints)
        {
            try
            {
                stream.WriteByte(109);
                stream.WriteIntArray(xPoints);
                stream.WriteIntArray(yPoints);
                stream.WriteInt(nPoints);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void FillRect(int x, int y, int width, int height)
        {
            try
            {
                stream.WriteByte(110);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void FillRoundRect(int x, int y, int width, int height, int arcWidth,
            int arcHeight)
        {
            try
            {
                stream.WriteByte(111);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.WriteInt(arcWidth);
                stream.WriteInt(arcHeight);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawChar(char c, int x, int y)
        {
            try
            {
                stream.WriteByte(112);
                stream.WriteChar(c);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawChars(char[] data, int offset, int length, int x, int y,
            bool monospaced)
        {
            try
            {
                stream.WriteByte(113);
                stream.WriteCharArray(data);
                stream.WriteInt(offset);
                stream.WriteInt(length);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteBoolean(monospaced);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawString(string str, int x, int y, bool monospaced)
        {
            try
            {
                stream.WriteByte(114);
                stream.WriteString(str);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteBoolean(monospaced);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void CreateSprite(int id, int width, int height, int[] pixels)
        {
            try
            {
                stream.WriteByte(115);
                stream.WriteInt(id);
                stream.WriteInt(width);
                stream.WriteInt(height);
                stream.WriteIntArray(pixels);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DrawSprite(int id, int x, int y)
        {
            try
            {
                stream.WriteByte(116);
                stream.WriteInt(id);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void SetPixel(int x, int y, int color)
        {
            try
            {
                stream.WriteByte(117);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(color);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetPixel(int x, int y)
        {
            try
            {
                stream.WriteByte(118);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void PowerCycle()
        {
            try
            {
                stream.WriteByte(120);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void Reset()
        {
            try
            {
                stream.WriteByte(121);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void DeleteSprite(int id)
        {
            try
            {
                stream.WriteByte(122);
                stream.WriteInt(id);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void SetSpeed(int percent)
        {
            try
            {
                stream.WriteByte(123);
                stream.WriteInt(percent);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void StepToNextFrame()
        {
            try
            {
                stream.WriteByte(124);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void ShowMessage(string message)
        {
            try
            {
                stream.WriteByte(125);
                stream.WriteString(message);
                stream.Flush();
            }
            catch
            {
            }
        }

        public string GetWorkingDirectory()
        {
            try
            {
                stream.WriteByte(126);
                stream.Flush();
                return stream.ReadString();
            }
            catch
            {
            }
            return null;
        }

        public string GetContentDirectory()
        {
            try
            {
                stream.WriteByte(127);
                stream.Flush();
                return stream.ReadString();
            }
            catch
            {
            }
            return null;
        }

        public void Open(string fileName)
        {
            try
            {
                stream.WriteByte(128);
                stream.WriteString(fileName);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void OpenArchiveEntry(string archiveFileName, string entryFileName)
        {
            try
            {
                stream.WriteByte(129);
                stream.WriteString(archiveFileName);
                stream.WriteString(entryFileName);
                stream.Flush();
            }
            catch
            {
            }
        }

        public string[] GetArchiveEntries(string archiveFileName)
        {
            try
            {
                stream.WriteByte(130);
                stream.WriteString(archiveFileName);
                stream.Flush();
                return stream.ReadDynamicStringArray();
            }
            catch
            {
            }
            return null;
        }

        public string GetDefaultArchiveEntry(string archiveFileName)
        {
            try
            {
                stream.WriteByte(131);
                stream.WriteString(archiveFileName);
                stream.Flush();
                return stream.ReadString();
            }
            catch
            {
            }
            return null;
        }

        public void OpenDefaultArchiveEntry(string archiveFileName)
        {
            try
            {
                stream.WriteByte(132);
                stream.WriteString(archiveFileName);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void Close()
        {
            try
            {
                stream.WriteByte(133);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void SaveState(string stateFileName)
        {
            try
            {
                stream.WriteByte(134);
                stream.WriteString(stateFileName);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void LoadState(string stateFileName)
        {
            try
            {
                stream.WriteByte(135);
                stream.WriteString(stateFileName);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void QuickSaveState(int slot)
        {
            try
            {
                stream.WriteByte(136);
                stream.WriteInt(slot);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void QuickLoadState(int slot)
        {
            try
            {
                stream.WriteByte(137);
                stream.WriteInt(slot);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void SetTvSystem(string tvSystem)
        {
            try
            {
                stream.WriteByte(138);
                stream.WriteString(tvSystem);
                stream.Flush();
            }
            catch
            {
            }
        }

        public string GetTvSystem()
        {
            try
            {
                stream.WriteByte(139);
                stream.Flush();
                return stream.ReadString();
            }
            catch
            {
            }
            return null;
        }

        public int GetDiskSides()
        {
            try
            {
                stream.WriteByte(140);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void InsertDisk(int disk, int side)
        {
            try
            {
                stream.WriteByte(141);
                stream.WriteInt(disk);
                stream.WriteInt(side);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void FlipDiskSide()
        {
            try
            {
                stream.WriteByte(142);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void EjectDisk()
        {
            try
            {
                stream.WriteByte(143);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void InsertCoin()
        {
            try
            {
                stream.WriteByte(144);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void PressServiceButton()
        {
            try
            {
                stream.WriteByte(145);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void ScreamIntoMicrophone()
        {
            try
            {
                stream.WriteByte(146);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void Glitch()
        {
            try
            {
                stream.WriteByte(147);
                stream.Flush();
            }
            catch
            {
            }
        }

        public string GetFileInfo()
        {
            try
            {
                stream.WriteByte(148);
                stream.Flush();
                return stream.ReadString();
            }
            catch
            {
            }
            return null;
        }

        public void SetFullscreenMode(bool fullscreenMode)
        {
            try
            {
                stream.WriteByte(149);
                stream.WriteBoolean(fullscreenMode);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void SaveScreenshot()
        {
            try
            {
                stream.WriteByte(150);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void AddCheat(int address, int value, int compare,
            string description, bool enabled)
        {
            try
            {
                stream.WriteByte(151);
                stream.WriteInt(address);
                stream.WriteInt(value);
                stream.WriteInt(compare);
                stream.WriteString(description);
                stream.WriteBoolean(enabled);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void RemoveCheat(int address, int value, int compare)
        {
            try
            {
                stream.WriteByte(152);
                stream.WriteInt(address);
                stream.WriteInt(value);
                stream.WriteInt(compare);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void AddGameGenie(string gameGenieCode, string description,
            bool enabled)
        {
            try
            {
                stream.WriteByte(153);
                stream.WriteString(gameGenieCode);
                stream.WriteString(description);
                stream.WriteBoolean(enabled);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void RemoveGameGenie(string gameGenieCode)
        {
            try
            {
                stream.WriteByte(154);
                stream.WriteString(gameGenieCode);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void AddProActionRocky(string proActionRockyCode, string description,
            bool enabled)
        {
            try
            {
                stream.WriteByte(155);
                stream.WriteString(proActionRockyCode);
                stream.WriteString(description);
                stream.WriteBoolean(enabled);
                stream.Flush();
            }
            catch
            {
            }
        }

        public void RemoveProActionRocky(string proActionRockyCode)
        {
            try
            {
                stream.WriteByte(156);
                stream.WriteString(proActionRockyCode);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetPrgRomSize()
        {
            try
            {
                stream.WriteByte(157);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public int ReadPrgRom(int index)
        {
            try
            {
                stream.WriteByte(158);
                stream.WriteInt(index);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void WritePrgRom(int index, int value)
        {
            try
            {
                stream.WriteByte(159);
                stream.WriteInt(index);
                stream.WriteInt(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetChrRomSize()
        {
            try
            {
                stream.WriteByte(160);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public int ReadChrRom(int index)
        {
            try
            {
                stream.WriteByte(161);
                stream.WriteInt(index);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public void WriteChrRom(int index, int value)
        {
            try
            {
                stream.WriteByte(162);
                stream.WriteInt(index);
                stream.WriteInt(value);
                stream.Flush();
            }
            catch
            {
            }
        }

        public int GetStringWidth(string str, bool monospaced)
        {
            try
            {
                stream.WriteByte(163);
                stream.WriteString(str);
                stream.WriteBoolean(monospaced);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }

        public int GetCharsWidth(char[] chars, bool monospaced)
        {
            try
            {
                stream.WriteByte(164);
                stream.WriteCharArray(chars);
                stream.WriteBoolean(monospaced);
                stream.Flush();
                return stream.ReadInt();
            }
            catch
            {
            }
            return -1;
        }
    }
}