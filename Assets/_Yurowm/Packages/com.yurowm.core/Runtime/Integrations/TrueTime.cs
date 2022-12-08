using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.Serialization;
using Yurowm.UI;

namespace Yurowm.Services {
    public class TrueTime : Integration, ITimeProvider {
        public List<NTPServer> servers = new List<NTPServer>();
        public TimeSpan debugTimeOffset;
        
        List<ScheduledTask> tasks = new List<ScheduledTask>();
        Queue<Action> beforeUpdate = new Queue<Action>();
        bool isUpdating = false;
        
        TimeSpan timeZone;
        DateTime? utcNow = null;
        bool needToRequest = true;
        
        public Action onGetTime = delegate {};
        
        public override string GetName() {
            return "True Time";
        }

        public override void Initialize() {
            base.Initialize();
            
            if (!active) return;
            
            App.onFocus += () => needToRequest = true;
            
            Logic().Run();

            if (Application.isEditor || UnityEngine.Debug.isDebugBuild)
                DebugTime().Run();
        }
        
        IEnumerator Logic() {
            DateTime? GetTime() {
                foreach (var server in servers) {
                    var time = server.GetTime();
                    if (time.HasValue)
                        return time.Value;
                }

                Debug.Log("All NTP servers are not avaliable");
                return null;
            }
            
            DateTime lastUpdate = DateTime.UtcNow;
            
            while (true) {
            
                if (needToRequest) {
                    utcNow = null;
                    
                    timeZone = DateTime.Now - DateTime.UtcNow;
                    
                    UIRefresh.Invoke();
                    
                    while (true) {
                        var time = GetTime();
                        if (time.HasValue) {
                            utcNow = time.Value;
                            break;
                        }
            
                        yield return new Wait(20);
                    }
            
                    #if UNITY_EDITOR
                    utcNow += debugTimeOffset;
                    #endif
            
                    onGetTime.Invoke();
                    UIRefresh.Invoke();
                    
                    needToRequest = false;
                    
                    UIRefresh.Invoke();
                } else {
                    var delta = (DateTime.UtcNow - lastUpdate);
                    if (delta.Seconds > 5) {
                        needToRequest = true;
                        yield return null;
                        continue;
                    }
                    utcNow = utcNow.Value + delta;
                }

                lastUpdate = DateTime.UtcNow;
                
                while (!beforeUpdate.IsEmpty()) 
                    beforeUpdate.Dequeue().Invoke();
                
                if (tasks.Count > 0) {
                    isUpdating = true;
                    
                    tasks.RemoveAll(t => {
                        if (t.time > utcNow) 
                            return false;
                        t.action.Invoke();
                        return true;
                    });
                    
                    isUpdating = false;
                }

                yield return null;
            }
        }

        IEnumerator DebugTime() {
            while (true) {
                DebugPanel.Log("Current Time UTC", "True Time", HasTime ? UTCNow.ToString() : "Is Unknown");
                DebugPanel.Log("Current Time", "True Time", HasTime ? Now.ToString() : "Is Unknown");
                DebugPanel.Log("Current Time Zone", "True Time", timeZone);
                DebugPanel.Log("Debug Time Offset", "True Time", debugTimeOffset);
                yield return null;
            }
        }

        public override bool HasAllNecessarySDK() {
            #if UNITY_WEBGL
            return false;
            #else
            return true;
            #endif
        }

        public bool HasTime => utcNow.HasValue;

        public TimeSpan Zone => timeZone;

        public DateTime UTCNow {
            get {
                if (HasTime) return utcNow.Value;
                throw new Exception("Current time is unknown. NTP servers are not avaliable.");
            }
        }

        public DateTime Now => UTCNow + Zone;

        public void Schedule(Action action, DateTime time) {
            if (action == null)
                return;
            
            if (isUpdating)
                beforeUpdate.Enqueue(() => Schedule(action, time));
            else {
                if (HasTime)
                    tasks.Add(new ScheduledTask(action, time));
                else
                    beforeUpdate.Enqueue(() => Schedule(action, time));
            }
        }

        public class NTPServer : ISerializable {
            public string URL;
            
            public DateTime? GetTime() {
                if (URL.IsNullOrEmpty())
                    return null;
                
                uint SwapEndianness(ulong x) {
                    return (uint) (((x & 0x000000ff) << 24) +
                                   ((x & 0x0000ff00) << 8) +
                                   ((x & 0x00ff0000) >> 8) +
                                   ((x & 0xff000000) >> 24));
                }
            
                try {
                    // NTP message size - 16 bytes of the digest (RFC 2030)
                    var ntpData = new byte[48];

                    //Setting the Leap Indicator, Version Number and Mode values
                    ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
                
                    var addresses = Dns.GetHostEntry(URL).AddressList;

                    //The UDP port number assigned to NTP is 123
                    var ipEndPoint = new IPEndPoint(addresses[0], 123);
                    //NTP uses UDP

                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                        socket.Connect(ipEndPoint);

                        //Stops code hang if NTP is blocked
                        socket.ReceiveTimeout = 3000;

                        socket.Send(ntpData);
                        socket.Receive(ntpData);
                        socket.Close();
                    }

                    //Offset to get to the "Transmit Timestamp" field (time at which the reply 
                    //departed the server for the client, in 64-bit timestamp format."
                    const byte serverReplyTime = 40;

                    //Get the seconds part
                    ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

                    //Get the seconds fraction
                    ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                    //Convert From big-endian to little-endian
                    intPart = SwapEndianness(intPart);
                    fractPart = SwapEndianness(fractPart);

                    var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                    //**UTC** time
                    var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long) milliseconds);

                    return networkDateTime;
                } catch (Exception e) {
                    Debug.LogException(e);
                    return null;
                }
            }
            
            public void Serialize(Writer writer) {
                writer.Write("URL", URL);      
            }

            public void Deserialize(Reader reader) {
                reader.Read("URL", ref URL);      
            }
        }
        
        struct ScheduledTask {
            public Action action;
            public DateTime time;
            
            public ScheduledTask(Action action, DateTime time) {
                this.action = action;
                this.time = time;
            }
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("debugTimeOffset", debugTimeOffset);
            writer.Write("servers", servers.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            servers.Clear();
            servers.AddRange(reader.ReadCollection<NTPServer>("servers"));
            reader.Read("debugTimeOffset", ref debugTimeOffset);
        }
    }
    
}