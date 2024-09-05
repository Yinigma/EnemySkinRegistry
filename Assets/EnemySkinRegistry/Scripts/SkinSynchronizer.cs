using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Netcode;


namespace AntlerShed.SkinRegistry
{
    internal class ConfigServer
    {
        private const string REQUEST_SYNC_MESSAGE_NAME = "RequestSync";
        private const string PROFILE_MESSAGE_NAME = "ServerProfile";
        private const string SYNC_REJECTION_MESSAGE_NAME = "SyncRejection";
        private ProfileConverter profileConverter = new ProfileConverter();
        public bool Running { get; private set; } = false;

        internal void Start()
        {
            if (NetworkManager.Singleton?.CustomMessagingManager != null && NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(REQUEST_SYNC_MESSAGE_NAME, HandleSyncRequest);
                Running = true;
            }
        }

        internal void Stop()
        {
            if (NetworkManager.Singleton?.CustomMessagingManager != null && NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(REQUEST_SYNC_MESSAGE_NAME);
                Running = false;
            }
        }

        internal void BroadcastSyncMessage(Profile workingProfile) => sendSyncMessage(null, workingProfile);

        internal void SendSyncMessageToClient(ulong clientId, Profile workingProfile) => sendSyncMessage(clientId, workingProfile);

        private void sendSyncMessage(ulong? clientId, Profile workingProfile)
        {
            if (NetworkManager.Singleton?.IsHost ?? false)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(workingProfile, new JsonConverter[] { profileConverter }));
                using (FastBufferWriter stream = new FastBufferWriter(bytes.Length + 4, Allocator.Temp))
                {
                    byte[] val = BitConverter.IsLittleEndian ? BitConverter.GetBytes(bytes.Length).Reverse().ToArray() : BitConverter.GetBytes((uint)bytes.Length);
                    stream.WriteBytesSafe(val, 4);
                    stream.WriteBytesSafe(bytes, bytes.Length);
                    
                    if (clientId.HasValue)
                    {
                        if(EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Sending Sync Message");
                        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(PROFILE_MESSAGE_NAME, clientId.Value, stream, NetworkDelivery.ReliableFragmentedSequenced);
                    }
                    else
                    {
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Sending Sync Message");
                        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(PROFILE_MESSAGE_NAME, stream, NetworkDelivery.ReliableFragmentedSequenced);
                    }
                }
            }
        }

        internal void HandleSyncRequest(ulong senderId, FastBufferReader messagePayload)
        {
            if (EnemySkinRegistry.AllowSyncSetting)
            {
                SendSyncMessageToClient(senderId, EnemySkinRegistry.GetWorkingProfile());
            }
            else
            {
                using (FastBufferWriter stream = new FastBufferWriter(0, Allocator.Temp))
                {
                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(SYNC_REJECTION_MESSAGE_NAME, senderId, stream);
                }
            }
        }
    }

    internal class ConfigClient
    {
        private const string REQUEST_SYNC_MESSAGE_NAME = "RequestSync";
        private const string PROFILE_MESSAGE_NAME = "ServerProfile";
        private const string SYNC_REJECTION_MESSAGE_NAME = "SyncRejection";
        private ProfileConverter profileConverter = new ProfileConverter();
        public bool Running { get; private set; } = false;

        internal void Start()
        {
            if(NetworkManager.Singleton?.CustomMessagingManager != null && !NetworkManager.Singleton.IsHost)
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Start client");
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(PROFILE_MESSAGE_NAME, HandleProfileMessage);
                Running = true;
            }
        }

        internal void Stop()
        {
            if (NetworkManager.Singleton?.CustomMessagingManager != null && !NetworkManager.Singleton.IsHost)
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Kill client");
                NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(PROFILE_MESSAGE_NAME);
                Running = false;
            }
        }

        internal void HandleProfileMessage(ulong senderId, FastBufferReader messagePayload)
        {
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Got profile message from server");

            byte[] sizeBytes = new byte[4];
            messagePayload.ReadBytesSafe(ref sizeBytes, 4);
            uint size = BitConverter.ToUInt32(BitConverter.IsLittleEndian ? sizeBytes.Reverse().ToArray() : sizeBytes);
            byte[] bytes = new byte[size];
            messagePayload.ReadBytesSafe(ref bytes, (int)size);
            string json = Encoding.UTF8.GetString(bytes);
            try
            {
                Profile cfg = JsonConvert.DeserializeObject<Profile>(json, new JsonConverter[] { profileConverter });
                EnemySkinRegistry.SyncWithRemoteProfile(cfg);
            }
            catch(Exception e)
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.ERROR) EnemySkinRegistry.SkinLogger.LogError(e.Message);
            }
        }

        internal void SendSyncRequestToServer()
        {
            if (NetworkManager.Singleton?.CustomMessagingManager != null && !NetworkManager.Singleton.IsHost)
            {
                using (FastBufferWriter stream = new FastBufferWriter(0, Allocator.Temp))
                {
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Sending sync request from client");
                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(REQUEST_SYNC_MESSAGE_NAME, NetworkManager.ServerClientId, stream);
                }
            }
        }
    }
}
