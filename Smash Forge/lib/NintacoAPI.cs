using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Net.Sockets;
using System.Threading;

namespace Nintaco {

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

  static class Colors {
    public const int GRAY = 0x00;
    public const int DARK_BLUE = 0x01;
    public const int DARK_INDIGO = 0x02;
    public const int DARK_VIOLET = 0x03;
    public const int DARK_MAGENTA = 0x04;
    public const int DARK_RED = 0x05;
    public const int DARK_ORANGE = 0x06;
    public const int DARK_BROWN = 0x07;
    public const int DARK_OLIVE = 0x08;
    public const int DARK_CHARTREUSE = 0x09;
    public const int DARK_GREEN = 0x0A;
    public const int DARK_MINT = 0x0B;
    public const int DARK_CYAN = 0x0C;
    public const int BLACKER_THAN_BLACK = 0x0D;
    public const int BLACK3 = 0x0E;
    public const int BLACK = 0x0F;

    public const int LIGHT_GRAY = 0x10;
    public const int BLUE = 0x11;
    public const int INDIGO = 0x12;
    public const int VIOLET = 0x13;
    public const int MAGENTA = 0x14;
    public const int RED = 0x15;
    public const int ORANGE = 0x16;
    public const int BROWN = 0x17;
    public const int OLIVE = 0x18;
    public const int CHARTREUSE = 0x19;
    public const int GREEN = 0x1A;
    public const int MINT = 0x1B;
    public const int CYAN = 0x1C;
    public const int BLACK2 = 0x1D;
    public const int BLACK4 = 0x1E;
    public const int BLACK5 = 0x1F;

    public const int WHITE = 0x20;
    public const int LIGHT_BLUE = 0x21;
    public const int LIGHT_INDIGO = 0x22;
    public const int LIGHT_VIOLET = 0x23;
    public const int LIGHT_MAGENTA = 0x24;
    public const int LIGHT_RED = 0x25;
    public const int LIGHT_ORANGE = 0x26;
    public const int LIGHT_BROWN = 0x27;
    public const int LIGHT_OLIVE = 0x28;
    public const int LIGHT_CHARTREUSE = 0x29;
    public const int LIGHT_GREEN = 0x2A;
    public const int LIGHT_MINT = 0x2B;
    public const int LIGHT_CYAN = 0x2C;
    public const int DARK_GRAY = 0x2D;
    public const int BLACK6 = 0x2E;
    public const int BLACK7 = 0x2F;

    public const int WHITE2 = 0x30;
    public const int PALE_BLUE = 0x31;
    public const int PALE_INDIGO = 0x32;
    public const int PALE_VIOLET = 0x33;
    public const int PALE_MAGENTA = 0x34;
    public const int PALE_RED = 0x35;
    public const int PALE_ORANGE = 0x36;
    public const int CREAM = 0x37;
    public const int YELLOW = 0x38;
    public const int PALE_CHARTREUSE = 0x39;
    public const int PALE_GREEN = 0x3A;
    public const int PALE_MINT = 0x3B;
    public const int PALE_CYAN = 0x3C;
    public const int PALE_GRAY = 0x3D;
    public const int BLACK8 = 0x3E;
    public const int BLACK9 = 0x3F;
  }

  static class EventTypes {
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

  public static class GamepadButtons {
    public const int A = 0;
    public const int B = 1;
    public const int Select = 2;
    public const int Start = 3;
    public const int Up = 4;
    public const int Down = 5;
    public const int Left = 6;
    public const int Right = 7;
  }

  static class AccessPointType {
    public const int PreRead = 0;
    public const int PostRead = 1;
    public const int PreWrite = 2;
    public const int PostWrite = 3;
    public const int PreExecute = 4;
    public const int PostExecute = 5;
  }

  class AccessPoint {

    public readonly AccessPointListener listener;
    public readonly int type;
    public readonly int minAddress;
    public readonly int maxAddress;
    public readonly int bank;

    public AccessPoint(AccessPointListener listener, int type, int minAddress) 
        : this(listener, type, minAddress, -1, -1) {
    }

    public AccessPoint(AccessPointListener listener, int type,
        int minAddress, int maxAddress) : this(listener, type, minAddress, 
            maxAddress, -1) {
    }

    public AccessPoint(AccessPointListener listener, int type, int minAddress, 
        int maxAddress, int bank) {

      this.listener = listener;
      this.type = type;
      this.bank = bank;

      if (maxAddress < 0) {
        this.minAddress = this.maxAddress = minAddress;
      } else if (minAddress <= maxAddress) {
        this.minAddress = minAddress;
        this.maxAddress = maxAddress;
      } else {
        this.minAddress = maxAddress;
        this.maxAddress = minAddress;
      }
    }
  }

  class ScanlineCyclePoint {

    public readonly ScanlineCycleListener listener;
    public readonly int scanline;
    public readonly int scanlineCycle;

    public ScanlineCyclePoint(ScanlineCycleListener listener, int scanline,
        int scanlineCycle) {
      this.listener = listener;
      this.scanline = scanline;
      this.scanlineCycle = scanlineCycle;
    }
  }

  class ScanlinePoint {

    public readonly ScanlineListener listener;
    public readonly int scanline;

    public ScanlinePoint(ScanlineListener listener, int scanline) {
      this.listener = listener;
      this.scanline = scanline;
    }
  }

  public class DataStream {

    public const int ARRAY_LENGTH = 1024;

    private readonly BinaryWriter writer;
    private readonly BinaryReader reader;

    public DataStream(BinaryWriter writer, BinaryReader reader) {
      this.writer = writer;
      this.reader = reader;
    }

    public void writeByte(int value) {
      writer.Write((byte)value);      
    }

    public int readByte() {
      return reader.ReadByte();    
    }

    public void writeInt(int value) {
      writeByte(value >> 24);
      writeByte(value >> 16);
      writeByte(value >> 8);
      writeByte(value);
    }

    public int readInt() {
      return (readByte() << 24) | (readByte() << 16) | (readByte() << 8) 
          | readByte();
    }

    public void writeIntArray(int[] array) {
      writeInt(array.Length);
      for(int i = 0; i < array.Length; i++) {
        writeInt(array[i]);
      }
    }

    public int readIntArray(int[] array) {
      int length = readInt();
      if (length < 0 || length > array.Length) {
        writer.Close();
        reader.Close();
        throw new IOException("Invalid array length: " + length);
      }
      for(int i = 0; i < length; i++) {
        array[i] = readInt();
      }
      return length;
    }

    public void writeBoolean(bool value) {
      writeByte(value ? 1 : 0);        
    }

    public bool readBoolean() {
      return readByte() != 0;
    }

    public void writeChar(char value) {
      writeByte(value);
    }

    public char readChar() {
      return (char)readByte();  
    }

    public void writeCharArray(char[] array) {
      writeInt(array.Length);
      for(int i = 0; i < array.Length; i++) {
        writeChar(array[i]);
      }
    }

    public int readCharArray(char[] array) {
      int length = readInt();
      if (length < 0 || length > array.Length) {
        writer.Close();
        reader.Close();
        throw new IOException("Invalid array length: " + length);
      }
      for(int i = 0; i < length; i++) {
        array[i] = readChar();
      }
      return length;
    }

    public void writeString(string value) {
      int length = value.Length;
      writeInt(length);
      for(int i = 0; i < length; i++) {
        writeChar(value[i]);
      }
    }

    public string readString() {
      int length = readInt();
        if (length < 0 || length > ARRAY_LENGTH) {
        writer.Close();
        reader.Close();
        throw new IOException("Invalid array length: " + length);
      }
      char[] cs = new char[length];
      for(int i = 0; i<length; i++) {
        cs[i] = readChar();
      }
      return new String(cs);
    }
  
    public void writeStringArray(string[] array) {
      writeInt(array.Length);
      for(int i = 0; i < array.Length; i++) {
        writeString(array[i]);
      }
    }

    public int readStringArray(String[] array) {
      int length = readInt();
      if (length < 0 || length > array.Length) {
        writer.Close();
        reader.Close();
        throw new IOException("Invalid array length: " + length);
      }
      for(int i = 0; i < length; i++) {
        array[i] = readString();
      }
      return length;
    }

    public String[] readDynamicStringArray() {
      int length = readInt();
      if (length < 0 || length > ARRAY_LENGTH) {
        writer.Close();
        reader.Close();
        throw new IOException("Invalid array length: " + length);
      }
      String[] array = new String[length];
      for(int i = 0; i<length; i++) {
        array[i] = readString();
      }
      return array;
    }  
  
    public void flush() {
      writer.Flush();
    }
  }

  class IdentityComparator : IEqualityComparer<object> {

    public new bool Equals(object x, object y) {
      return Object.ReferenceEquals(x, y);
    }

    public int GetHashCode(object obj) {
      return RuntimeHelpers.GetHashCode(obj);
    }
  }

  public static class ApiSource {

    public static RemoteAPI API;

    public static void initRemoteAPI(String host, int port) {
      API = new RemoteAPI(host, port);
    }
  }

  public abstract class RemoteBase {

    private readonly int[] EVENT_TYPES = {
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

    public const int EVENT_REQUEST = 0xFF;
    public const int EVENT_RESPONSE = 0xFE;
    public const int HEARTBEAT = 0xFD;
    public const int READY = 0xFC;
    public const int RETRY_MILLIS = 1000;

    // listener -> listenerID
    protected readonly Dictionary<object, int> listenerIDs 
        = new Dictionary<object, int>(new IdentityComparator());

    // eventType -> listenerID -> listenerObject(listener)
    protected readonly Dictionary<int, Dictionary<int, object>> listenerObjects
        = new Dictionary<int, Dictionary<int, object>>();

    protected string host;
    protected int port;
    protected DataStream stream;
    protected int nextID;
    protected bool running;

    public RemoteBase(string host, int port) {
      this.host = host;
      this.port = port;
      foreach(int eventType in EVENT_TYPES) {
        listenerObjects[eventType] = new Dictionary<int, object>();
      }
    }

    public void run() {
      if (running) {
        return;
      } else {
        running = true;
      }
      while(true) {
        fireStatusChanged("Connecting to {0}:{1}...", host, port);
        TcpClient socket;
        try {
          socket = new TcpClient(host, port);
          BufferedStream bs = new BufferedStream(socket.GetStream());
          stream = new DataStream(new BinaryWriter(bs), new BinaryReader(bs));
        } catch {
          fireStatusChanged("Failed to establish connection.");
        }
        if (stream != null) {
          try {
            fireStatusChanged("Connection established.");
            sendListeners();
            sendReady();
            while (true) {
              probeEvents();
            }
          } catch {
            fireDeactivated();
            fireStatusChanged("Disconnected.");
          } finally {
            stream = null;
          }
        }
        Thread.Sleep(RETRY_MILLIS);
      }
    }

    protected void fireDeactivated() {
      foreach(object obj in new List<object>(listenerObjects[EventTypes
          .Deactivate].Values)) {
        ((DeactivateListener)obj)();
      }
    }

  protected void fireStatusChanged(string message, params object[] ps) {
    string msg = String.Format(message, ps);
    foreach(object obj in new List<object>(listenerObjects[EventTypes.Status]
        .Values)) {
      ((StatusListener)obj)(msg);
    }
  }

  protected void sendReady() {
    if (stream != null) {
      try {
        stream.writeByte(READY);
        stream.flush();
      } catch {
      }
    }
  }

  protected void sendListeners() {
    foreach(KeyValuePair<int, Dictionary<int, object>> e1 in listenerObjects) {
      foreach(KeyValuePair<int, object> e2 in e1.Value) {
        sendListener(e2.Key, e1.Key, e2.Value);
      }
    }
  }

  protected void probeEvents() {

    stream.writeByte(EVENT_REQUEST);
    stream.flush();

    int eventType = stream.readByte();
    
    if (eventType == HEARTBEAT) {
      stream.writeByte(EVENT_RESPONSE);
      stream.flush();
      return;
    }

    int listenerID = stream.readInt();
    object obj = listenerObjects[eventType][listenerID];

    if (obj != null) {
      if (eventType == EventTypes.Access) {
        int type = stream.readInt();
        int address = stream.readInt();
        int value = stream.readInt();
        int result = ((AccessPoint)obj).listener(type, address, value);
        stream.writeByte(EVENT_RESPONSE);
        stream.writeInt(result);
      } else {
        switch (eventType) {
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
            ((ScanlinePoint)obj).listener(stream.readInt());
            break;
          case EventTypes.ScanlineCycle:
            ((ScanlineCyclePoint)obj).listener(stream.readInt(), 
                stream.readInt(), stream.readInt(), stream.readBoolean());
            break;
          case EventTypes.SpriteZero:
            ((SpriteZeroListener)obj)(stream.readInt(),
                stream.readInt());
            break;
          case EventTypes.Status:
            ((StatusListener)obj)(stream.readString());
            break;
          default:
            throw new IOException("Unknown listener type: " + eventType);
        }
        stream.writeByte(EVENT_RESPONSE);
      }
    }

    stream.flush();
    }


    protected void sendListener(int listenerID, int eventType,
        object listenerObject) {
      if (stream != null) {
        try {
          stream.writeByte(eventType);
          stream.writeInt(listenerID);
          switch (eventType) {
            case EventTypes.Access: {
              AccessPoint point = (AccessPoint)listenerObject;
              stream.writeInt(point.type);
              stream.writeInt(point.minAddress);
              stream.writeInt(point.maxAddress);
              stream.writeInt(point.bank);
              break;
            }
            case EventTypes.Scanline: {
              ScanlinePoint point = (ScanlinePoint)listenerObject;
              stream.writeInt(point.scanline);
              break;
            }
            case EventTypes.ScanlineCycle: {
              ScanlineCyclePoint point = (ScanlineCyclePoint)listenerObject;
              stream.writeInt(point.scanline);
              stream.writeInt(point.scanlineCycle);
              break;
            }
          }
          stream.flush();
        } catch {
        }
      }
    }


    protected void addListener(object listener, int eventType) {
      if (listener != null) {
        sendListener(addListenerObject(listener, eventType), eventType,
            listener);
      }
    }

    public void removeListener(object listener, int eventType, 
        int methodValue) {
      if (listener != null) {
        int listenerID = removeListenerObject(listener, eventType);
        if (listenerID >= 0 && stream != null) {
          try {
            stream.writeByte(methodValue);
            stream.writeInt(listenerID);
            stream.flush();
          } catch {
          }
          }
        }
      }


    protected int addListenerObject(object listener, int eventType) {
      return addListenerObject(listener, eventType, listener);
    }

    protected int addListenerObject(object listener, int eventType, 
        object listenerObject) {
      int listenerID = nextID++;
      listenerIDs[listener] = listenerID;
      listenerObjects[eventType][listenerID] = listenerObject;
      return listenerID;
    }

    protected int removeListenerObject(object listener, int eventType) {

      Int32 listenerID;
      if (listenerIDs.TryGetValue(listener, out listenerID)) { 
        listenerIDs.Remove(listener);
        listenerObjects[eventType].Remove(listenerID);
        return listenerID;
      } else {
        return -1;
      }
    }

    public void addActivateListener(ActivateListener listener) {
      addListener(listener, EventTypes.Activate);
    }

    public void removeActivateListener(ActivateListener listener) {
      removeListener(listener, EventTypes.Activate, 2);
    }

    public void addDeactivateListener(DeactivateListener listener) {
      addListener(listener, EventTypes.Deactivate);
    }

    public void removeDeactivateListener(DeactivateListener listener) {
      removeListener(listener, EventTypes.Deactivate, 4);
    }

    public void addStopListener(StopListener listener) {
      addListener(listener, EventTypes.Stop);
    }

    public void removeStopListener(StopListener listener) {
      removeListener(listener, EventTypes.Stop, 6);
    }

    public void addAccessPointListener(AccessPointListener listener,
        int accessPointType, int address) {
      addAccessPointListener(listener, accessPointType, address, -1, -1);
    }

    public void addAccessPointListener(AccessPointListener listener,
        int accessPointType, int minAddress, int maxAddress) {
      addAccessPointListener(listener, accessPointType, minAddress, maxAddress,
          -1);
    }

    public void addAccessPointListener(AccessPointListener listener,
        int accessPointType, int minAddress, int maxAddress,
            int bank) {

      if (listener != null) {
        AccessPoint point = new AccessPoint(listener, accessPointType,
            minAddress, maxAddress, bank);
        sendListener(addListenerObject(listener, EventTypes.Access, point), 
            EventTypes.Access, point);
      }
    }

    public void removeAccessPointListener(AccessPointListener listener) {
      removeListener(listener, EventTypes.Access, 10);
    }

    public void addControllersListener(ControllersListener listener) {
      addListener(listener, EventTypes.Controllers);
    }

    public void removeControllersListener(ControllersListener listener) {
      removeListener(listener, EventTypes.Controllers, 12);
    }

    public void addFrameListener(FrameListener listener) {
      addListener(listener, EventTypes.Frame);
    }

    public void removeFrameListener(FrameListener listener) {
      removeListener(listener, EventTypes.Frame, 14);
    }

    public void addScanlineListener(ScanlineListener listener, int scanline) {

      if (listener != null) {
        ScanlinePoint point = new ScanlinePoint(listener, scanline);
        sendListener(addListenerObject(listener, EventTypes.Scanline, point), 
            EventTypes.Scanline, point);
      }
    }

    public void removeScanlineListener(ScanlineListener listener) {
      removeListener(listener, EventTypes.Scanline, 16);
    }

    public void addScanlineCycleListener(ScanlineCycleListener listener,
        int scanline, int scanlineCycle) {

      if (listener != null) {
        ScanlineCyclePoint point = new ScanlineCyclePoint(listener,
            scanline, scanlineCycle);
        sendListener(addListenerObject(listener, EventTypes.ScanlineCycle, 
            point), EventTypes.ScanlineCycle, point);
      }
    }

    public void removeScanlineCycleListener(
        ScanlineCycleListener listener) {
      removeListener(listener, EventTypes.ScanlineCycle, 18);
    }

    public void addSpriteZeroListener(SpriteZeroListener listener) {
      addListener(listener, EventTypes.SpriteZero);
    }

    public void removeSpriteZeroListener(SpriteZeroListener listener) {
      removeListener(listener, EventTypes.SpriteZero, 20);
    }

    public void addStatusListener(StatusListener listener) {
      addListener(listener, EventTypes.Status);
    }

    public void removeStatusListener(StatusListener listener) {
      removeListener(listener, EventTypes.Status, 22);
    }

    public void getPixels(int[] pixels) {
      try {
        stream.writeByte(119);
        stream.flush();
        stream.readIntArray(pixels);
      } catch {
      }
    }
  }

  // THIS IS AN AUTOGENERATED CLASS. DO NOT MODIFY.
  public class RemoteAPI : RemoteBase {

    public RemoteAPI(string host, int port) : base(host, port) {
    }

    public void setPaused(bool paused) {
      try {
        stream.writeByte(23);
        stream.writeBoolean(paused);
        stream.flush();
      } catch {
      }
    }

    public bool isPaused() {
      try {
        stream.writeByte(24);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public int getFrameCount() {
      try {
        stream.writeByte(25);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public int getA() {
      try {
        stream.writeByte(26);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setA(int A) {
      try {
        stream.writeByte(27);
        stream.writeInt(A);
        stream.flush();
      } catch {
      }
    }

    public int getS() {
      try {
        stream.writeByte(28);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setS(int S) {
      try {
        stream.writeByte(29);
        stream.writeInt(S);
        stream.flush();
      } catch {
      }
    }

    public int getPC() {
      try {
        stream.writeByte(30);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setPC(int PC) {
      try {
        stream.writeByte(31);
        stream.writeInt(PC);
        stream.flush();
      } catch {
      }
    }

    public int getX() {
      try {
        stream.writeByte(32);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setX(int X) {
      try {
        stream.writeByte(33);
        stream.writeInt(X);
        stream.flush();
      } catch {
      }
    }

    public int getY() {
      try {
        stream.writeByte(34);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setY(int Y) {
      try {
        stream.writeByte(35);
        stream.writeInt(Y);
        stream.flush();
      } catch {
      }
    }

    public int getP() {
      try {
        stream.writeByte(36);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setP(int P) {
      try {
        stream.writeByte(37);
        stream.writeInt(P);
        stream.flush();
      } catch {
      }
    }

    public bool isN() {
      try {
        stream.writeByte(38);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setN(bool N) {
      try {
        stream.writeByte(39);
        stream.writeBoolean(N);
        stream.flush();
      } catch {
      }
    }

    public bool isV() {
      try {
        stream.writeByte(40);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setV(bool V) {
      try {
        stream.writeByte(41);
        stream.writeBoolean(V);
        stream.flush();
      } catch {
      }
    }

    public bool isD() {
      try {
        stream.writeByte(42);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setD(bool D) {
      try {
        stream.writeByte(43);
        stream.writeBoolean(D);
        stream.flush();
      } catch {
      }
    }

    public bool isI() {
      try {
        stream.writeByte(44);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setI(bool I) {
      try {
        stream.writeByte(45);
        stream.writeBoolean(I);
        stream.flush();
      } catch {
      }
    }

    public bool isZ() {
      try {
        stream.writeByte(46);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setZ(bool Z) {
      try {
        stream.writeByte(47);
        stream.writeBoolean(Z);
        stream.flush();
      } catch {
      }
    }

    public bool isC() {
      try {
        stream.writeByte(48);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setC(bool C) {
      try {
        stream.writeByte(49);
        stream.writeBoolean(C);
        stream.flush();
      } catch {
      }
    }

    public int getPPUv() {
      try {
        stream.writeByte(50);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setPPUv(int v) {
      try {
        stream.writeByte(51);
        stream.writeInt(v);
        stream.flush();
      } catch {
      }
    }

    public int getPPUt() {
      try {
        stream.writeByte(52);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setPPUt(int t) {
      try {
        stream.writeByte(53);
        stream.writeInt(t);
        stream.flush();
      } catch {
      }
    }

    public int getPPUx() {
      try {
        stream.writeByte(54);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setPPUx(int x) {
      try {
        stream.writeByte(55);
        stream.writeInt(x);
        stream.flush();
      } catch {
      }
    }

    public bool isPPUw() {
      try {
        stream.writeByte(56);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setPPUw(bool w) {
      try {
        stream.writeByte(57);
        stream.writeBoolean(w);
        stream.flush();
      } catch {
      }
    }

    public int getCameraX() {
      try {
        stream.writeByte(58);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setCameraX(int scrollX) {
      try {
        stream.writeByte(59);
        stream.writeInt(scrollX);
        stream.flush();
      } catch {
      }
    }

    public int getCameraY() {
      try {
        stream.writeByte(60);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setCameraY(int scrollY) {
      try {
        stream.writeByte(61);
        stream.writeInt(scrollY);
        stream.flush();
      } catch {
      }
    }

    public int getScanline() {
      try {
        stream.writeByte(62);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public int getDot() {
      try {
        stream.writeByte(63);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public bool isSpriteZeroHit() {
      try {
        stream.writeByte(64);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setSpriteZeroHit(bool sprite0Hit) {
      try {
        stream.writeByte(65);
        stream.writeBoolean(sprite0Hit);
        stream.flush();
      } catch {
      }
    }

    public int getScanlineCount() {
      try {
        stream.writeByte(66);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void requestInterrupt() {
      try {
        stream.writeByte(67);
        stream.flush();
      } catch {
      }
    }

    public void acknowledgeInterrupt() {
      try {
        stream.writeByte(68);
        stream.flush();
      } catch {
      }
    }

    public int peekCPU(int address) {
      try {
        stream.writeByte(69);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public int readCPU(int address) {
      try {
        stream.writeByte(70);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void writeCPU(int address, int value) {
      try {
        stream.writeByte(71);
        stream.writeInt(address);
        stream.writeInt(value);
        stream.flush();
      } catch {
      }
    }

    public int peekCPU16(int address) {
      try {
        stream.writeByte(72);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public int readCPU16(int address) {
      try {
        stream.writeByte(73);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void writeCPU16(int address, int value) {
      try {
        stream.writeByte(74);
        stream.writeInt(address);
        stream.writeInt(value);
        stream.flush();
      } catch {
      }
    }

    public int peekCPU32(int address) {
      try {
        stream.writeByte(75);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public int readCPU32(int address) {
      try {
        stream.writeByte(76);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void writeCPU32(int address, int value) {
      try {
        stream.writeByte(77);
        stream.writeInt(address);
        stream.writeInt(value);
        stream.flush();
      } catch {
      }
    }

    public int readPPU(int address) {
      try {
        stream.writeByte(78);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void writePPU(int address, int value) {
      try {
        stream.writeByte(79);
        stream.writeInt(address);
        stream.writeInt(value);
        stream.flush();
      } catch {
      }
    }

    public int readPaletteRAM(int address) {
      try {
        stream.writeByte(80);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void writePaletteRAM(int address, int value) {
      try {
        stream.writeByte(81);
        stream.writeInt(address);
        stream.writeInt(value);
        stream.flush();
      } catch {
      }
    }

    public int readOAM(int address) {
      try {
        stream.writeByte(82);
        stream.writeInt(address);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void writeOAM(int address, int value) {
      try {
        stream.writeByte(83);
        stream.writeInt(address);
        stream.writeInt(value);
        stream.flush();
      } catch {
      }
    }

    public bool readGamepad(int gamepad, int button) {
      try {
        stream.writeByte(84);
        stream.writeInt(gamepad);
        stream.writeInt(button);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void writeGamepad(int gamepad, int button, bool value) {
      try {
        stream.writeByte(85);
        stream.writeInt(gamepad);
        stream.writeInt(button);
        stream.writeBoolean(value);
        stream.flush();
      } catch {
      }
    }

    public bool isZapperTrigger() {
      try {
        stream.writeByte(86);
        stream.flush();
        return stream.readBoolean();
      } catch {
      }
      return false;
    }

    public void setZapperTrigger(bool zapperTrigger) {
      try {
        stream.writeByte(87);
        stream.writeBoolean(zapperTrigger);
        stream.flush();
      } catch {
      }
    }

    public int getZapperX() {
      try {
        stream.writeByte(88);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setZapperX(int x) {
      try {
        stream.writeByte(89);
        stream.writeInt(x);
        stream.flush();
      } catch {
      }
    }

    public int getZapperY() {
      try {
        stream.writeByte(90);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setZapperY(int y) {
      try {
        stream.writeByte(91);
        stream.writeInt(y);
        stream.flush();
      } catch {
      }
    }

    public void setColor(int color) {
      try {
        stream.writeByte(92);
        stream.writeInt(color);
        stream.flush();
      } catch {
      }
    }

    public int getColor() {
      try {
        stream.writeByte(93);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void setClip(int x, int y, int width, int height) {
      try {
        stream.writeByte(94);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.flush();
      } catch {
      }
    }

    public void clipRect(int x, int y, int width, int height) {
      try {
        stream.writeByte(95);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.flush();
      } catch {
      }
    }

    public void resetClip() {
      try {
        stream.writeByte(96);
        stream.flush();
      } catch {
      }
    }

    public void copyArea(int x, int y, int width, int height, int dx, int dy) {
      try {
        stream.writeByte(97);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.writeInt(dx);
        stream.writeInt(dy);
        stream.flush();
      } catch {
      }
    }

    public void drawLine(int x1, int y1, int x2, int y2) {
      try {
        stream.writeByte(98);
        stream.writeInt(x1);
        stream.writeInt(y1);
        stream.writeInt(x2);
        stream.writeInt(y2);
        stream.flush();
      } catch {
      }
    }

    public void drawOval(int x, int y, int width, int height) {
      try {
        stream.writeByte(99);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.flush();
      } catch {
      }
    }

    public void drawPolygon(int[] xPoints, int[] yPoints, int nPoints) {
      try {
        stream.writeByte(100);
        stream.writeIntArray(xPoints);
        stream.writeIntArray(yPoints);
        stream.writeInt(nPoints);
        stream.flush();
      } catch {
      }
    }

    public void drawPolyline(int[] xPoints, int[] yPoints, int nPoints) {
      try {
        stream.writeByte(101);
        stream.writeIntArray(xPoints);
        stream.writeIntArray(yPoints);
        stream.writeInt(nPoints);
        stream.flush();
      } catch {
      }
    }

    public void drawRect(int x, int y, int width, int height) {
      try {
        stream.writeByte(102);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.flush();
      } catch {
      }
    }

    public void drawRoundRect(int x, int y, int width, int height, int arcWidth,
        int arcHeight) {
      try {
        stream.writeByte(103);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.writeInt(arcWidth);
        stream.writeInt(arcHeight);
        stream.flush();
      } catch {
      }
    }

    public void draw3DRect(int x, int y, int width, int height, bool raised) {
      try {
        stream.writeByte(104);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.writeBoolean(raised);
        stream.flush();
      } catch {
      }
    }

    public void drawArc(int x, int y, int width, int height, int startAngle,
        int arcAngle) {
      try {
        stream.writeByte(105);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.writeInt(startAngle);
        stream.writeInt(arcAngle);
        stream.flush();
      } catch {
      }
    }

    public void fill3DRect(int x, int y, int width, int height, bool raised) {
      try {
        stream.writeByte(106);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.writeBoolean(raised);
        stream.flush();
      } catch {
      }
    }

    public void fillArc(int x, int y, int width, int height, int startAngle,
        int arcAngle) {
      try {
        stream.writeByte(107);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.writeInt(startAngle);
        stream.writeInt(arcAngle);
        stream.flush();
      } catch {
      }
    }

    public void fillOval(int x, int y, int width, int height) {
      try {
        stream.writeByte(108);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.flush();
      } catch {
      }
    }

    public void fillPolygon(int[] xPoints, int[] yPoints, int nPoints) {
      try {
        stream.writeByte(109);
        stream.writeIntArray(xPoints);
        stream.writeIntArray(yPoints);
        stream.writeInt(nPoints);
        stream.flush();
      } catch {
      }
    }

    public void fillRect(int x, int y, int width, int height) {
      try {
        stream.writeByte(110);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.flush();
      } catch {
      }
    }

    public void fillRoundRect(int x, int y, int width, int height, int arcWidth,
        int arcHeight) {
      try {
        stream.writeByte(111);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.writeInt(arcWidth);
        stream.writeInt(arcHeight);
        stream.flush();
      } catch {
      }
    }

    public void drawChar(char c, int x, int y) {
      try {
        stream.writeByte(112);
        stream.writeChar(c);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.flush();
      } catch {
      }
    }

    public void drawChars(char[] data, int offset, int length, int x, int y,
        bool monospaced) {
      try {
        stream.writeByte(113);
        stream.writeCharArray(data);
        stream.writeInt(offset);
        stream.writeInt(length);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeBoolean(monospaced);
        stream.flush();
      } catch {
      }
    }

    public void drawString(string str, int x, int y, bool monospaced) {
      try {
        stream.writeByte(114);
        stream.writeString(str);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeBoolean(monospaced);
        stream.flush();
      } catch {
      }
    }

    public void createSprite(int id, int width, int height, int[] pixels) {
      try {
        stream.writeByte(115);
        stream.writeInt(id);
        stream.writeInt(width);
        stream.writeInt(height);
        stream.writeIntArray(pixels);
        stream.flush();
      } catch {
      }
    }

    public void drawSprite(int id, int x, int y) {
      try {
        stream.writeByte(116);
        stream.writeInt(id);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.flush();
      } catch {
      }
    }

    public void setPixel(int x, int y, int color) {
      try {
        stream.writeByte(117);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.writeInt(color);
        stream.flush();
      } catch {
      }
    }

    public int getPixel(int x, int y) {
      try {
        stream.writeByte(118);
        stream.writeInt(x);
        stream.writeInt(y);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void powerCycle() {
      try {
        stream.writeByte(120);
        stream.flush();
      } catch {
      }
    }

    public void reset() {
      try {
        stream.writeByte(121);
        stream.flush();
      } catch {
      }
    }

    public void deleteSprite(int id) {
      try {
        stream.writeByte(122);
        stream.writeInt(id);
        stream.flush();
      } catch {
      }
    }

    public void setSpeed(int percent) {
      try {
        stream.writeByte(123);
        stream.writeInt(percent);
        stream.flush();
      } catch {
      }
    }

    public void stepToNextFrame() {
      try {
        stream.writeByte(124);
        stream.flush();
      } catch {
      }
    }

    public void showMessage(string message) {
      try {
        stream.writeByte(125);
        stream.writeString(message);
        stream.flush();
      } catch {
      }
    }

    public string getWorkingDirectory() {
      try {
        stream.writeByte(126);
        stream.flush();
        return stream.readString();
      } catch {
      }
      return null;
    }

    public string getContentDirectory() {
      try {
        stream.writeByte(127);
        stream.flush();
        return stream.readString();
      } catch {
      }
      return null;
    }

    public void open(string fileName) {
      try {
        stream.writeByte(128);
        stream.writeString(fileName);
        stream.flush();
      } catch {
      }
    }

    public void openArchiveEntry(string archiveFileName, string entryFileName) {
      try {
        stream.writeByte(129);
        stream.writeString(archiveFileName);
        stream.writeString(entryFileName);
        stream.flush();
      } catch {
      }
    }

    public string[] getArchiveEntries(string archiveFileName) {
      try {
        stream.writeByte(130);
        stream.writeString(archiveFileName);
        stream.flush();
        return stream.readDynamicStringArray();
      } catch {
      }
      return null;
    }

    public string getDefaultArchiveEntry(string archiveFileName) {
      try {
        stream.writeByte(131);
        stream.writeString(archiveFileName);
        stream.flush();
        return stream.readString();
      } catch {
      }
      return null;
    }

    public void openDefaultArchiveEntry(string archiveFileName) {
      try {
        stream.writeByte(132);
        stream.writeString(archiveFileName);
        stream.flush();
      } catch {
      }
    }

    public void close() {
      try {
        stream.writeByte(133);
        stream.flush();
      } catch {
      }
    }

    public void saveState(string stateFileName) {
      try {
        stream.writeByte(134);
        stream.writeString(stateFileName);
        stream.flush();
      } catch {
      }
    }

    public void loadState(string stateFileName) {
      try {
        stream.writeByte(135);
        stream.writeString(stateFileName);
        stream.flush();
      } catch {
      }
    }

    public void quickSaveState(int slot) {
      try {
        stream.writeByte(136);
        stream.writeInt(slot);
        stream.flush();
      } catch {
      }
    }

    public void quickLoadState(int slot) {
      try {
        stream.writeByte(137);
        stream.writeInt(slot);
        stream.flush();
      } catch {
      }
    }

    public void setTVSystem(string tvSystem) {
      try {
        stream.writeByte(138);
        stream.writeString(tvSystem);
        stream.flush();
      } catch {
      }
    }

    public string getTVSystem() {
      try {
        stream.writeByte(139);
        stream.flush();
        return stream.readString();
      } catch {
      }
      return null;
    }

    public int getDiskSides() {
      try {
        stream.writeByte(140);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void insertDisk(int disk, int side) {
      try {
        stream.writeByte(141);
        stream.writeInt(disk);
        stream.writeInt(side);
        stream.flush();
      } catch {
      }
    }

    public void flipDiskSide() {
      try {
        stream.writeByte(142);
        stream.flush();
      } catch {
      }
    }

    public void ejectDisk() {
      try {
        stream.writeByte(143);
        stream.flush();
      } catch {
      }
    }

    public void insertCoin() {
      try {
        stream.writeByte(144);
        stream.flush();
      } catch {
      }
    }

    public void pressServiceButton() {
      try {
        stream.writeByte(145);
        stream.flush();
      } catch {
      }
    }

    public void screamIntoMicrophone() {
      try {
        stream.writeByte(146);
        stream.flush();
      } catch {
      }
    }

    public void glitch() {
      try {
        stream.writeByte(147);
        stream.flush();
      } catch {
      }
    }

    public string getFileInfo() {
      try {
        stream.writeByte(148);
        stream.flush();
        return stream.readString();
      } catch {
      }
      return null;
    }

    public void setFullscreenMode(bool fullscreenMode) {
      try {
        stream.writeByte(149);
        stream.writeBoolean(fullscreenMode);
        stream.flush();
      } catch {
      }
    }

    public void saveScreenshot() {
      try {
        stream.writeByte(150);
        stream.flush();
      } catch {
      }
    }

    public void addCheat(int address, int value, int compare,
        string description, bool enabled) {
      try {
        stream.writeByte(151);
        stream.writeInt(address);
        stream.writeInt(value);
        stream.writeInt(compare);
        stream.writeString(description);
        stream.writeBoolean(enabled);
        stream.flush();
      } catch {
      }
    }

    public void removeCheat(int address, int value, int compare) {
      try {
        stream.writeByte(152);
        stream.writeInt(address);
        stream.writeInt(value);
        stream.writeInt(compare);
        stream.flush();
      } catch {
      }
    }

    public void addGameGenie(string gameGenieCode, string description,
        bool enabled) {
      try {
        stream.writeByte(153);
        stream.writeString(gameGenieCode);
        stream.writeString(description);
        stream.writeBoolean(enabled);
        stream.flush();
      } catch {
      }
    }

    public void removeGameGenie(string gameGenieCode) {
      try {
        stream.writeByte(154);
        stream.writeString(gameGenieCode);
        stream.flush();
      } catch {
      }
    }

    public void addProActionRocky(string proActionRockyCode, string description,
        bool enabled) {
      try {
        stream.writeByte(155);
        stream.writeString(proActionRockyCode);
        stream.writeString(description);
        stream.writeBoolean(enabled);
        stream.flush();
      } catch {
      }
    }

    public void removeProActionRocky(string proActionRockyCode) {
      try {
        stream.writeByte(156);
        stream.writeString(proActionRockyCode);
        stream.flush();
      } catch {
      }
    }

    public int getPrgRomSize() {
      try {
        stream.writeByte(157);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public int readPrgRom(int index) {
      try {
        stream.writeByte(158);
        stream.writeInt(index);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void writePrgRom(int index, int value) {
      try {
        stream.writeByte(159);
        stream.writeInt(index);
        stream.writeInt(value);
        stream.flush();
      } catch {
      }
    }

    public int getChrRomSize() {
      try {
        stream.writeByte(160);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public int readChrRom(int index) {
      try {
        stream.writeByte(161);
        stream.writeInt(index);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public void writeChrRom(int index, int value) {
      try {
        stream.writeByte(162);
        stream.writeInt(index);
        stream.writeInt(value);
        stream.flush();
      } catch {
      }
    }

    public int getStringWidth(string str, bool monospaced) {
      try {
        stream.writeByte(163);
        stream.writeString(str);
        stream.writeBoolean(monospaced);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }

    public int getCharsWidth(char[] chars, bool monospaced) {
      try {
        stream.writeByte(164);
        stream.writeCharArray(chars);
        stream.writeBoolean(monospaced);
        stream.flush();
        return stream.readInt();
      } catch {
      }
      return -1;
    }
  }
}