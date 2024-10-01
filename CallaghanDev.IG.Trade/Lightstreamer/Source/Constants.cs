// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.Constants
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System.Runtime.InteropServices;

namespace Lightstreamer.DotNet.Client
{
  internal class Constants
  {
    public const string pushServerCmd = "/lightstreamer/create_session.txt";
    public const string requestUserAgent = "Lightstreamer .NET Client";
    public const string requestContentType = "application/x-www-form-urlencoded";
    public const string pushServerBindCmd = "/lightstreamer/bind_session.txt";
    public const string pushServerControlCmd = "/lightstreamer/control.txt";
    public const string pushServerSendMessageCmd = "/lightstreamer/send_message.txt";

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct FastItemsListener
    {
      public static readonly string UNCHANGED = Lightstreamer.DotNet.Client.ServerUpdateEvent.UNCHANGED;
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct SimpleTableListener
    {
      public static readonly string UNCHANGED = Lightstreamer.DotNet.Client.ServerUpdateEvent.UNCHANGED;
    }

    public class SubscribedTableKey
    {
      public const int keyValueNotSet = -1;
    }

    public class ConnectionConstraints
    {
      public const double maxBandwidthNotSet = -1.0;
    }

    public class SubscriptionConstraints
    {
      public const double maxFrequencyNotSet = -1.0;
    }

    public class TableManager
    {
      public const int distinctSnapshotLengthNotSet = -1;
      public const int startNotSet = -1;
      public const int endNotSet = -1;
      public const double maxFrequencyNotSet = -1.0;
      public const int bufferSizeNotSet = -1;
    }

    public class ServerUpdateEvent
    {
      public const int tableCodeNotSet = -1;
      public const int itemCodeNotSet = -1;
    }

    public class PushServerQuery
    {
      public const string maxBandwidthKey = "LS_requested_max_bandwidth";
      public const string contentLengthKey = "LS_content_length";
      public const string keepaliveMillisKey = "LS_keepalive_millis";
      public const string pollingKey = "LS_polling";
      public const string pollingMillisKey = "LS_polling_millis";
      public const string pollingIdleKey = "LS_idle_millis";
      public const string recoveryFromKey = "LS_recovery_from";
      public const string reportKey = "LS_report_info";
      public const string userIdKey = "LS_user";
      public const string passwordKey = "LS_password";
      public const string cidKey = "LS_cid";
      public const string adapterSetKey = "LS_adapter_set";
      public const string sessionIdKey = "LS_session";
      public const string opKey = "LS_op";
      public const string opKey2 = "LS_op2";
      public const string opCreate = "create";
      public const string opAdd = "add";
      public const string opDelete = "delete";
      public const string opReconf = "reconf";
      public const string opConstrain = "constrain";
      public const string opDestroy = "destroy";
      public const string tableCodeBase = "LS_table";
      public const string itemNameBase = "LS_id";
      public const string tableStartKey = "LS_start";
      public const string tableEndKey = "LS_end";
      public const string pushModeBase = "LS_mode";
      public const string schemaKey = "LS_schema";
      public const string dataAdapterKey = "LS_data_adapter";
      public const string selectorKey = "LS_selector";
      public const string tableBufferSizeKey = "LS_requested_buffer_size";
      public const string tableFrequencyKey = "LS_requested_max_frequency";
      public const string unfilteredDispatching = "unfiltered";
      public const string snapshotKey = "LS_Snapshot";
      public const string snapshotOn = "true";
      public const string messageKey = "LS_message";
      public const string messageProgKey = "LS_msg_prog";
      public const string messageSequenceKey = "LS_sequence";
      public const string messageDelayKey = "LS_max_wait";
    }

    public class PushServerPage
    {
      public const string controlAddress = "ControlAddress:";
      public const string sessionId = "SessionId:";
      public const string keepaliveMillis = "KeepaliveMillis:";
      public const string maxBandwidth = "MaxBandwidth:";
      public const string requestLimit = "RequestLimit:";
      public const string serverName = "ServerName:";
      public const string preamble = "Preamble:";
      public const string okCommand = "OK";
      public const string probeCommand = "PROBE";
      public const string progCommand = "PROG";
      public const string loopCommand = "LOOP";
      public const string endCommand = "END";
      public const string syncErrorCommand = "SYNC ERROR";
      public const string errorCommand = "ERROR";
      public const string eosMarker = "EOS";
      public const string overflowMarker = "OV";
      public const string doneMarker = "DONE";
      public const string errMarker = "ERR";
      public const string msgMarker = "MSG";
    }

    public class CommandMode
    {
      public const string keyField = "key";
      public const string commandField = "command";
      public const string addCommand = "ADD";
      public const string updateCommand = "UPDATE";
      public const string deleteCommand = "DELETE";
    }
  }
}
