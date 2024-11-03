// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.PushServerTranslator
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Lightstreamer.DotNet.Client
{
    internal class PushServerTranslator
    {
        private ConnectionInfo info;
        private long enforcedEventProg = -1;
        private static readonly char comma = ',';
        private CookieContainer cookies = new CookieContainer();
        private BatchManager batchManager;
        private BatchManager mexBatchManager;
        private static ILog streamLogger = LogManager.GetLogger("com.lightstreamer.ls_client.stream");
        private static ILog protLogger = LogManager.GetLogger("com.lightstreamer.ls_client.protocol");

        internal PushServerTranslator(ConnectionInfo info)
        {
            this.batchManager = new BatchManager(this.cookies, info);
            this.mexBatchManager = new BatchManager(this.cookies, info);
            ConnectionInfo connectionInfo1 = (ConnectionInfo)info.Clone();
            if (connectionInfo1.PushServerUrl == null)
                throw new PushConnException("Connection property 'pushServerUrl' not set");
            int length1;
            ConnectionInfo connectionInfo2;
            for (; connectionInfo1.PushServerUrl.EndsWith("/"); connectionInfo2.PushServerUrl = connectionInfo2.PushServerUrl.Substring(0, length1))
            {
                length1 = connectionInfo1.PushServerUrl.Length - 1;
                connectionInfo2 = connectionInfo1;
            }
            int length2;
            ConnectionInfo connectionInfo3;
            if (connectionInfo1.PushServerControlUrl != null)
            {
                for (; connectionInfo1.PushServerControlUrl.EndsWith("/"); connectionInfo3.PushServerControlUrl = connectionInfo3.PushServerControlUrl.Substring(0, length2))
                {
                    length2 = connectionInfo1.PushServerControlUrl.Length - 1;
                    connectionInfo3 = connectionInfo1;
                }
            }
            this.info = connectionInfo1;
        }

        internal virtual Stream CallSession()
        {
            IDictionary dictionary = (IDictionary)new Dictionary<string, string>();
            if (this.info.User != null)
                dictionary[(object)"LS_user"] = (object)this.info.User;
            if (this.info.Password != null)
                dictionary[(object)"LS_password"] = (object)this.info.Password;
            dictionary[(object)"LS_op2"] = (object)"create";
            dictionary[(object)"LS_cid"] = (object)"veOfhw.i6 35e74CHfDprfc75GSy";
            dictionary[(object)"LS_adapter_set"] = (object)this.info.GetAdapterSet();
            if (!this.info.Polling && this.info.useGetForStreaming)
                PushServerTranslator.AddConnectionPropertiesForFakePolling(dictionary, this.info);
            else
                PushServerTranslator.AddConnectionProperties(dictionary, this.info);
            PushServerTranslator.AddConstraints(dictionary, this.info.Constraints);
            HttpProvider httpProvider = new HttpProvider(this.info.PushServerUrl + "/lightstreamer/create_session.txt", this.cookies, this.info.HttpExtraHeaders, this.info.ReadTimeoutMillis, this.info.ConnectTimeoutMillis);
            PushServerTranslator.protLogger.Info("Opening stream connection");
            if (PushServerTranslator.protLogger.IsDebugEnabled)
                PushServerTranslator.protLogger.Debug("Connection params: " + CollectionsSupport.ToString((ICollection)dictionary));
            IDictionary parameters = dictionary;
            return httpProvider.DoHTTP(parameters, true);
        }

        internal virtual Stream CallResync(
          PushServerProxy.PushServerProxyInfo pushInfo,
          ConnectionConstraints newConstraints,
          long recoveryProg)
        {
            IDictionary dictionary = (IDictionary)new Dictionary<string, string>();
            dictionary[(object)"LS_session"] = (object)pushInfo.sessionId;
            if (newConstraints != null)
                this.info.Constraints = (ConnectionConstraints)newConstraints.Clone();
            if (recoveryProg >= 0L)
                dictionary[(object)"LS_recovery_from"] = (object)recoveryProg.ToString();
            PushServerTranslator.AddConnectionProperties(dictionary, this.info);
            PushServerTranslator.AddConstraints(dictionary, this.info.Constraints);
            HttpProvider httpProvider = new HttpProvider(pushInfo.rebindAddress + "/lightstreamer/bind_session.txt", this.cookies, this.info.HttpExtraHeaders, this.info.ReadTimeoutMillis, this.info.ConnectTimeoutMillis);
            PushServerTranslator.protLogger.Info("Opening stream connection to rebind current session");
            if (PushServerTranslator.protLogger.IsDebugEnabled)
                PushServerTranslator.protLogger.Debug("Rebinding params: " + CollectionsSupport.ToString((ICollection)dictionary));
            bool flag = !this.info.Polling && this.info.useGetForStreaming;
            IDictionary parameters = dictionary;
            int num = !flag ? 1 : 0;
            return httpProvider.DoHTTP(parameters, num != 0);
        }

        private static void AddConstraints(IDictionary parameters, ConnectionConstraints constraints)
        {
            if (constraints.MaxBandwidth == -1.0)
                return;
            parameters[(object)"LS_requested_max_bandwidth"] = (object)constraints.MaxBandwidth.ToString();
        }

        private static void AddConnectionProperties(IDictionary parameters, ConnectionInfo properties)
        {
            if (properties.ContentLength > 0L)
                parameters[(object)"LS_content_length"] = (object)Convert.ToString(properties.ContentLength);
            if (properties.KeepaliveMillis > 0L)
                parameters[(object)"LS_keepalive_millis"] = (object)Convert.ToString(properties.KeepaliveMillis);
            if (properties.Polling)
            {
                parameters[(object)"LS_polling"] = (object)"true";
                if (properties.PollingMillis > 0L)
                    parameters[(object)"LS_polling_millis"] = (object)Convert.ToString(properties.PollingMillis);
                else
                    parameters[(object)"LS_polling_millis"] = (object)"0";
                if (properties.PollingIdleMillis > 0L)
                    parameters[(object)"LS_idle_millis"] = (object)Convert.ToString(properties.PollingIdleMillis);
            }
            parameters[(object)"LS_report_info"] = (object)"true";
        }

        private static void AddConnectionPropertiesForFakePolling(
          IDictionary parameters,
          ConnectionInfo properties)
        {
            if (properties.ContentLength > 0L)
                parameters[(object)"LS_content_length"] = (object)Convert.ToString(properties.ContentLength);
            if (properties.KeepaliveMillis > 0L)
                parameters[(object)"LS_keepalive_millis"] = (object)Convert.ToString(properties.KeepaliveMillis);
            parameters[(object)"LS_polling"] = (object)"true";
            parameters[(object)"LS_polling_millis"] = (object)"0";
            parameters[(object)"LS_idle_millis"] = (object)"0";
            parameters[(object)"LS_report_info"] = (object)"true";
        }

        internal virtual PushServerProxy.PushServerProxyInfo ReadSessionId(LSStreamReader pushStream)
        {
            string sessionId = (string)null;
            string host = (string)null;
            long keepaliveMillis = 0;
            PushServerTranslator.protLogger.Info("Reading stream connection info");
            while (true)
            {
                string str;
                do
                {
                    do
                    {
                        str = pushStream.ReadLine();
                        PushServerTranslator.streamLogger.Debug("Read info line: " + str);
                        if (str == null)
                            throw new PushServerException(4);
                        if (!str.Trim().Equals(""))
                        {
                            if (str.StartsWith("SessionId:"))
                                sessionId = str.Substring("SessionId:".Length);
                            else if (str.StartsWith("ControlAddress:"))
                                host = str.Substring("ControlAddress:".Length);
                            else if (str.StartsWith("KeepaliveMillis:"))
                            {
                                string s = str.Substring("KeepaliveMillis:".Length);
                                try
                                {
                                    keepaliveMillis = long.Parse(s);
                                }
                                catch (FormatException ex)
                                {
                                    throw new PushServerException(7);
                                }
                            }
                        }
                        else
                            goto label_22;
                    }
                    while (str.StartsWith("MaxBandwidth:"));
                    if (str.StartsWith("RequestLimit:"))
                    {
                        string s = str.Substring("RequestLimit:".Length);
                        long num;
                        try
                        {
                            num = long.Parse(s);
                        }
                        catch (FormatException ex)
                        {
                            throw new PushServerException(7);
                        }
                        PushServerTranslator.streamLogger.Debug("Using " + (object)num + " as the request maximum length");
                        this.batchManager.Limit = num;
                        this.mexBatchManager.Limit = num;
                    }
                }
                while (str.StartsWith("Preamble:"));
                if (str.StartsWith("ServerName:"))
                    PushServerTranslator.protLogger.Info("Server name received: " + str.Substring("ServerName:".Length));
                else
                    PushServerTranslator.protLogger.Info("Discarded unknown property: " + str);
            }
        label_22:
            if (sessionId == null)
                throw new PushServerException(7);
            string str1 = this.info.PushServerControlUrl == null ? this.info.PushServerUrl : this.info.PushServerControlUrl;
            string str2 = this.info.PushServerUrl;
            if (host != null)
            {
                Uri uri1 = new Uri(str1);
                str1 = new UriBuilder(uri1.Scheme, host, uri1.Port, uri1.AbsolutePath).Uri.ToString();
                if (str1.EndsWith("/"))
                    str1 = str1.Substring(0, str1.Length - 1);
                Uri uri2 = new Uri(str2);
                str2 = new UriBuilder(uri2.Scheme, host, uri2.Port, uri2.AbsolutePath).Uri.ToString();
                if (str2.EndsWith("/"))
                    str2 = str2.Substring(0, str2.Length - 1);
            }
            PushServerProxy.PushServerProxyInfo pushServerProxyInfo = new PushServerProxy.PushServerProxyInfo(sessionId, str1, str2, keepaliveMillis);
            if (PushServerTranslator.protLogger.IsDebugEnabled)
                PushServerTranslator.protLogger.Debug("Using info: " + (object)pushServerProxyInfo);
            return pushServerProxyInfo;
        }

        internal virtual void CallSendMessageRequest(
          PushServerProxy.PushServerProxyInfo pushInfo,
          string message)
        {
            IDictionary parameters = (IDictionary)new Dictionary<string, string>();
            parameters[(object)"LS_session"] = (object)pushInfo.sessionId;
            parameters[(object)"LS_message"] = (object)message;
            LSStreamReader notBatchedAnswer = this.mexBatchManager.GetNotBatchedAnswer(pushInfo.controlAddress + "/lightstreamer/send_message.txt", parameters);
            try
            {
                this.CheckAnswer(notBatchedAnswer);
            }
            catch (PushEndException ex)
            {
                throw new PushServerException(7);
            }
            finally
            {
                try
                {
                    PushServerTranslator.streamLogger.Debug("Closing message connection");
                    notBatchedAnswer.Close();
                }
                catch (IOException ex)
                {
                    PushServerTranslator.streamLogger.Debug("Error closing message connection", (Exception)ex);
                }
            }
        }

        internal virtual void CallGuaranteedSendMessageRequest(
          PushServerProxy.PushServerProxyInfo pushInfo,
          string messageProg,
          MessageManager message,
          BatchMonitor batch)
        {
            IDictionary parameters = (IDictionary)new Dictionary<string, string>();
            parameters[(object)"LS_session"] = (object)pushInfo.sessionId;
            parameters[(object)"LS_message"] = (object)message.Message;
            parameters[(object)"LS_sequence"] = (object)message.Sequence;
            parameters[(object)"LS_msg_prog"] = (object)messageProg;
            if (message.DelayTimeout > -1)
                parameters[(object)"LS_max_wait"] = (object)Convert.ToString(message.DelayTimeout);
            this.DoControlRequest(pushInfo, parameters, "/lightstreamer/send_message.txt", batch, this.mexBatchManager);
        }

        internal virtual void CallDestroyRequest(PushServerProxy.PushServerProxyInfo pushInfo)
        {
            IDictionary parameters = (IDictionary)new Dictionary<string, string>();
            parameters[(object)"LS_session"] = (object)pushInfo.sessionId;
            parameters[(object)"LS_op"] = (object)"destroy";
            LSStreamReader notBatchedAnswer = this.batchManager.GetNotBatchedAnswer(pushInfo.controlAddress + "/lightstreamer/control.txt", parameters);
            try
            {
                this.CheckAnswer(notBatchedAnswer);
            }
            finally
            {
                try
                {
                    PushServerTranslator.streamLogger.Debug("Closing destroy connection");
                    notBatchedAnswer.Close();
                }
                catch (IOException ex)
                {
                    PushServerTranslator.streamLogger.Debug("Error closing destroy connection", (Exception)ex);
                }
            }
        }

        internal virtual void CallConstrainRequest(
          PushServerProxy.PushServerProxyInfo pushInfo,
          ConnectionConstraints newConstraints)
        {
            IDictionary parameters = (IDictionary)new Dictionary<string, string>();
            parameters[(object)"LS_session"] = (object)pushInfo.sessionId;
            parameters[(object)"LS_op"] = (object)"constrain";
            this.info.Constraints = (ConnectionConstraints)newConstraints.Clone();
            PushServerTranslator.AddConstraints(parameters, this.info.Constraints);
            LSStreamReader notBatchedAnswer = this.batchManager.GetNotBatchedAnswer(pushInfo.controlAddress + "/lightstreamer/control.txt", parameters);
            try
            {
                this.CheckAnswer(notBatchedAnswer);
            }
            catch (PushEndException ex)
            {
                throw new PushServerException(7);
            }
            finally
            {
                try
                {
                    PushServerTranslator.streamLogger.Debug("Closing constrain connection");
                    notBatchedAnswer.Close();
                }
                catch (IOException ex)
                {
                    PushServerTranslator.streamLogger.Debug("Error closing constrain connection", (Exception)ex);
                }
            }
        }

        internal virtual void CallTableRequest(
          PushServerProxy.PushServerProxyInfo pushInfo,
          string tableCode,
          ITableManager table,
          BatchMonitor batch)
        {
            IDictionary parameters = (IDictionary)new Dictionary<string, string>();
            parameters[(object)"LS_session"] = (object)pushInfo.sessionId;
            parameters[(object)"LS_op"] = (object)"add";
            parameters[(object)"LS_table"] = (object)tableCode;
            parameters[(object)"LS_id"] = (object)table.Group;
            parameters[(object)"LS_mode"] = (object)table.Mode;
            parameters[(object)"LS_schema"] = (object)table.Schema;
            if (table.DataAdapter != null)
                parameters[(object)"LS_data_adapter"] = (object)table.DataAdapter;
            if (table.Selector != null)
                parameters[(object)"LS_selector"] = (object)table.Selector;
            if (table.Snapshot)
            {
                if (table.DistinctSnapshotLength != -1)
                    parameters[(object)"LS_Snapshot"] = (object)table.DistinctSnapshotLength.ToString();
                else
                    parameters[(object)"LS_Snapshot"] = (object)"true";
            }
            if (table.Start != -1)
                parameters[(object)"LS_start"] = (object)table.Start.ToString();
            if (table.End != -1)
                parameters[(object)"LS_end"] = (object)table.End.ToString();
            if (table.Unfiltered)
                parameters[(object)"LS_requested_max_frequency"] = (object)"unfiltered";
            else if (table.MaxFrequency != -1.0)
                parameters[(object)"LS_requested_max_frequency"] = (object)table.MaxFrequency.ToString();
            if (table.MaxBufferSize != -1)
                parameters[(object)"LS_requested_buffer_size"] = (object)table.MaxBufferSize.ToString();
            this.DoControlRequest(pushInfo, parameters, batch);
        }

        internal virtual void CallItemsRequest(
          PushServerProxy.PushServerProxyInfo pushInfo,
          string[] tableCodes,
          VirtualTableManager table,
          BatchMonitor batch)
        {
            IDictionary parameters = (IDictionary)new Dictionary<string, string>();
            parameters[(object)"LS_session"] = (object)pushInfo.sessionId;
            parameters[(object)"LS_op"] = (object)"add";
            parameters[(object)"LS_mode"] = (object)table.Mode;
            parameters[(object)"LS_schema"] = (object)table.Schema;
            if (table.DataAdapter != null)
                parameters[(object)"LS_data_adapter"] = (object)table.DataAdapter;
            for (int i = 0; i < table.NumItems; ++i)
            {
                parameters[(object)("LS_table" + (object)(i + 1))] = (object)tableCodes[i];
                parameters[(object)("LS_id" + (object)(i + 1))] = table.GetItemName(i);
                if (table.Selector != null)
                    parameters[(object)("LS_selector" + (object)(i + 1))] = (object)table.Selector;
                int num;
                if (table.Snapshot)
                {
                    if (table.DistinctSnapshotLength != -1)
                    {
                        IDictionary dictionary = parameters;
                        string key = "LS_Snapshot" + (object)(i + 1);
                        num = table.DistinctSnapshotLength;
                        string str = num.ToString();
                        dictionary[(object)key] = (object)str;
                    }
                    else
                        parameters[(object)("LS_Snapshot" + (object)(i + 1))] = (object)"true";
                }
                if (table.Unfiltered)
                    parameters[(object)("LS_requested_max_frequency" + (object)(i + 1))] = (object)"unfiltered";
                else if (table.MaxFrequency != -1.0)
                    parameters[(object)("LS_requested_max_frequency" + (object)(i + 1))] = (object)table.MaxFrequency.ToString();
                if (table.MaxBufferSize != -1)
                {
                    IDictionary dictionary = parameters;
                    string key = "LS_requested_buffer_size" + (object)(i + 1);
                    num = table.MaxBufferSize;
                    string str = num.ToString();
                    dictionary[(object)key] = (object)str;
                }
            }
            this.DoControlRequest(pushInfo, parameters, batch);
        }

        internal virtual void CallReconf(
          PushServerProxy.PushServerProxyInfo pushInfo,
          string[] tableCodes,
          SubscriptionConstraints constraints)
        {
            IDictionary parameters = (IDictionary)new Dictionary<string, string>();
            parameters[(object)"LS_session"] = (object)pushInfo.sessionId;
            parameters[(object)"LS_op"] = (object)"reconf";
            for (int index = 0; index < tableCodes.Length; ++index)
                parameters[(object)("LS_table" + (object)(index + 1))] = (object)tableCodes[index];
            if (constraints.MaxFrequency == -1.0)
                return;
            parameters[(object)"LS_requested_max_frequency"] = (object)constraints.MaxFrequency.ToString();
            this.DoControlRequest(pushInfo, parameters, (BatchMonitor)null);
        }

        internal virtual void CallDelete(
          PushServerProxy.PushServerProxyInfo pushInfo,
          string[] tableCodes,
          BatchMonitor batch)
        {
            IDictionary parameters = (IDictionary)new Dictionary<string, string>();
            parameters[(object)"LS_session"] = (object)pushInfo.sessionId;
            parameters[(object)"LS_op"] = (object)"delete";
            for (int index = 0; index < tableCodes.Length; ++index)
                parameters[(object)("LS_table" + (object)(index + 1))] = (object)tableCodes[index];
            this.DoControlRequest(pushInfo, parameters, batch);
        }

        private void DoControlRequest(
          PushServerProxy.PushServerProxyInfo pushInfo,
          IDictionary parameters,
          BatchMonitor batch)
        {
            this.DoControlRequest(pushInfo, parameters, "/lightstreamer/control.txt", batch, this.batchManager);
        }

        private void DoControlRequest(
          PushServerProxy.PushServerProxyInfo pushInfo,
          IDictionary parameters,
          string commandPath,
          BatchMonitor batch,
          BatchManager selectedBatchManager)
        {
            string controlUrl = pushInfo.controlAddress + commandPath;
            LSStreamReader answer = batch == null ? selectedBatchManager.GetNotBatchedAnswer(controlUrl, parameters) : selectedBatchManager.GetAnswer(controlUrl, parameters, batch);
            try
            {
                this.CheckAnswer(answer);
            }
            catch (PushEndException ex)
            {
                throw new PushServerException(7);
            }
            finally
            {
                try
                {
                    if (!(answer is BatchingHttpProvider.MyReader))
                        PushServerTranslator.streamLogger.Debug("Closing control connection");
                    answer.Close();
                }
                catch (IOException ex)
                {
                    PushServerTranslator.streamLogger.Debug("Error closing control connection", (Exception)ex);
                }
            }
        }

        internal virtual void StartControlBatch(PushServerProxy.PushServerProxyInfo pushInfo) => this.batchManager.StartBatch(pushInfo.controlAddress + "/lightstreamer/control.txt");

        internal virtual void StartMessageBatch(PushServerProxy.PushServerProxyInfo pushInfo) => this.mexBatchManager.StartBatch(pushInfo.controlAddress + "/lightstreamer/send_message.txt");

        internal virtual void CloseControlBatch() => this.batchManager.CloseBatch();

        internal virtual void CloseMessageBatch() => this.mexBatchManager.CloseBatch();

        internal virtual void AbortBatches()
        {
            this.batchManager.AbortBatch();
            this.mexBatchManager.AbortBatch();
        }

        internal virtual void CheckAnswer(LSStreamReader answer)
        {
            // Read the first line from the answer stream
            string extraMsg = answer.ReadLine();

            // Log the answer read from the stream
            PushServerTranslator.streamLogger.Debug("Read answer: " + extraMsg);

            // Switch on the answer message to handle different cases
            switch (extraMsg)
            {
                case null:
                    // If the answer is null, throw an exception indicating a server error
                    throw new PushServerException(6);

                case "OK":
                    // If the answer is "OK", log that the request was successful
                    PushServerTranslator.protLogger.Info("Request successful");

                    // Reset the enforced event progress
                    this.enforcedEventProg = -1L;
                    break;

                case "ERROR":
                    // Read the error code from the stream
                    string str1 = answer.ReadLine();
                    PushServerTranslator.streamLogger.Debug("Read error code: " + str1);

                    // If no error code is provided, throw an exception
                    if (str1 == null)
                        throw new PushServerException(4);

                    // Read the error message from the stream
                    string serverMsg = answer.ReadLine();
                    PushServerTranslator.streamLogger.Debug("Read error message: " + serverMsg);

                    // If the error message is null, use a default message
                    if (serverMsg == null)
                        serverMsg = "Request refused";

                    // Try parsing the error code as an integer
                    int serverCode;
                    try
                    {
                        serverCode = int.Parse(str1);
                    }
                    catch (FormatException ex)
                    {
                        // Log and throw an exception if the error code is not a valid integer
                        PushServerTranslator.protLogger.Debug("Error in received answer", ex);
                        throw new PushServerException(5, str1);
                    }

                    // Throw a user-specific exception with the error code and message
                    throw new PushUserException(serverCode, serverMsg);

                case "END":
                    // Handle "END" message
                    string str2 = answer.ReadLine();

                    // If there is an additional message, process it as an end cause
                    if (str2 != null && str2.Length != 0)
                    {
                        int endCause;
                        try
                        {
                            // Try parsing the end cause code
                            endCause = int.Parse(str2);
                        }
                        catch (FormatException ex)
                        {
                            // Log and throw an exception if the end cause code is not a valid integer
                            PushServerTranslator.protLogger.Debug("Error in received answer", ex);
                            throw new PushServerException(5, str2);
                        }

                        // Log and throw an end exception with the parsed cause
                        PushServerTranslator.streamLogger.Debug("Read end with code: " + endCause);
                        throw new PushEndException(endCause);
                    }

                    // If there is no additional message, log and throw a generic end exception
                    PushServerTranslator.streamLogger.Debug("Read end with no code");
                    throw new PushEndException();

                case "SYNC ERROR":
                    // Handle "SYNC ERROR" by throwing a sync-specific exception
                    throw new PushServerException(8);

                default:
                    // For any unrecognized answer, throw a generic server exception
                    throw new PushServerException(5, extraMsg);
            }
        }


        internal virtual InfoString WaitCommand(LSStreamReader pushStream)
        {
            string extraMsg = pushStream.ReadLine();
            if (PushServerTranslator.streamLogger.IsDebugEnabled)
                PushServerTranslator.streamLogger.Debug("Read data: " + extraMsg);
            switch (extraMsg)
            {
                case null:
                    throw new IOException();
                case "PROBE":
                    PushServerTranslator.protLogger.Debug("Got probe event");
                    return (InfoString)null;
                default:
                    if (extraMsg.StartsWith("PROG"))
                    {
                        string str = extraMsg.Substring("PROG".Length);
                        if (str.Length != 0)
                        {
                            if (str[0] == ' ')
                            {
                                long num;
                                try
                                {
                                    num = long.Parse(str.Substring(1));
                                }
                                catch (FormatException ex)
                                {
                                    throw new PushServerException(5, extraMsg);
                                }
                                PushServerTranslator.protLogger.Debug("Got notification of start progressive as " + (object)num);
                                this.enforcedEventProg = this.enforcedEventProg == -1L || num == this.enforcedEventProg ? num : throw new PushServerException(7, extraMsg);
                                return (InfoString)null;
                            }
                        }
                        throw new PushServerException(5, extraMsg);
                    }
                    if (extraMsg.StartsWith("LOOP"))
                    {
                        string str = extraMsg.Substring("LOOP".Length);
                        long holdingMillis;
                        if (str.Length == 0)
                        {
                            holdingMillis = 0L;
                        }
                        else
                        {
                            if (str[0] != ' ')
                                throw new PushServerException(5, extraMsg);
                            try
                            {
                                holdingMillis = long.Parse(str.Substring(1));
                            }
                            catch (FormatException ex)
                            {
                                throw new PushServerException(5, extraMsg);
                            }
                        }
                        if (holdingMillis == 0L)
                            PushServerTranslator.protLogger.Info("Got notification for Content-Length reached");
                        else
                            PushServerTranslator.protLogger.Debug("Poll completed; next in " + (object)holdingMillis + " ms");
                        return new InfoString(holdingMillis);
                    }
                    if (extraMsg.StartsWith("END"))
                    {
                        string str = extraMsg.Substring("END".Length);
                        if (str.Length == 0)
                        {
                            PushServerTranslator.protLogger.Info("Got notification for server originated connection closure with code null");
                            throw new PushEndException();
                        }
                        if (str[0] != ' ')
                            throw new PushServerException(5, extraMsg);
                        int endCause;
                        try
                        {
                            endCause = int.Parse(str.Substring(1));
                        }
                        catch (FormatException ex)
                        {
                            throw new PushServerException(5, extraMsg);
                        }
                        PushServerTranslator.protLogger.Info("Got notification for server originated connection closure with code " + (object)endCause);
                        throw new PushEndException(endCause);
                    }
                    if (this.enforcedEventProg == -1L)
                        return new InfoString(extraMsg);
                    ++this.enforcedEventProg;
                    return new InfoString(extraMsg, this.enforcedEventProg);
            }
        }

        internal virtual ServerUpdateEvent ParsePushData(string pushData)
        {
            ServerUpdateEvent pushData1 = (ServerUpdateEvent)null;
            if (pushData.StartsWith("MSG"))
            {
                int index1 = 0;
                string[] strArray = new string[5];
                int index2 = 0;
                if (pushData.Length > 3 && pushData[3] == ',')
                {
                    int startIndex = 4;
                    for (index2 = 0; index2 < 5; ++index2)
                    {
                        if (index2 == 4)
                        {
                            strArray[index2] = pushData.Substring(startIndex);
                        }
                        else
                        {
                            int num = pushData.IndexOf(PushServerTranslator.comma, startIndex);
                            if (num > -1)
                            {
                                strArray[index2] = pushData.Substring(startIndex, num - startIndex);
                                startIndex = num + 1;
                            }
                            else
                            {
                                strArray[index2] = pushData.Substring(startIndex);
                                ++index2;
                                break;
                            }
                        }
                    }
                }
                bool flag = false;
                try
                {
                    if (strArray != null && index2 == 3 && strArray[index1 + 2].Equals("DONE"))
                    {
                        pushData1 = new ServerUpdateEvent(strArray[index1], Convert.ToInt32(strArray[index1 + 1]));
                        flag = true;
                    }
                    else if (strArray != null)
                    {
                        if (index2 == 5)
                        {
                            if (strArray[index1 + 2].Equals("ERR"))
                            {
                                pushData1 = new ServerUpdateEvent(strArray[index1], Convert.ToInt32(strArray[index1 + 1]), Convert.ToInt32(strArray[index1 + 3]), strArray[index1 + 4]);
                                flag = true;
                            }
                        }
                    }
                }
                catch (FormatException ex)
                {
                }
                catch (OverflowException ex)
                {
                }
                finally
                {
                    if (!flag)
                        throw new PushServerException(5, pushData);
                }
            }
            else
            {
                int num1 = pushData.IndexOf('|');
                string str1 = (string)null;
                string str2;
                if (num1 == -1)
                {
                    int num2 = pushData.LastIndexOf(',');
                    str2 = num2 != -1 ? pushData.Substring(0, num2 - 0) : throw new PushServerException(5, pushData);
                    str1 = pushData.Substring(num2 + 1);
                }
                else
                    str2 = pushData.Substring(0, num1 - 0);
                int num3 = str2.IndexOf(',');
                string tableCode = num3 != -1 ? str2.Substring(0, num3 - 0) : throw new PushServerException(5, pushData);
                string itemCode = str2.Substring(num3 + 1);
                switch (str1)
                {
                    case null:
                        pushData1 = new ServerUpdateEvent(tableCode, itemCode);
                        int num4;
                        for (; num1 < pushData.Length; num1 = num4)
                        {
                            num4 = pushData.IndexOf('|', num1 + 1);
                            if (num4 == -1)
                                num4 = pushData.Length;
                            if (num4 == num1 + 1)
                            {
                                pushData1.AddValue(ServerUpdateEvent.UNCHANGED);
                            }
                            else
                            {
                                string src = pushData.Substring(num1 + 1, num4 - (num1 + 1));
                                if (src.Length == 1 && src[0] == '$')
                                    pushData1.AddValue("");
                                else if (src.Length == 1 && src[0] == '#')
                                    pushData1.AddValue((string)null);
                                else if (src[0] == '$' || src[0] == '#')
                                    pushData1.AddValue(PushServerTranslator.DeUNIcode(src.Substring(1)));
                                else
                                    pushData1.AddValue(PushServerTranslator.DeUNIcode(src));
                            }
                        }
                        break;
                    case "EOS":
                        return new ServerUpdateEvent(tableCode, itemCode, true);
                    default:
                        if (!str1.StartsWith("OV"))
                            throw new PushServerException(5, pushData);
                        try
                        {
                            int overflow = int.Parse(str1.Substring("OV".Length));
                            return new ServerUpdateEvent(tableCode, itemCode, overflow);
                        }
                        catch (Exception ex)
                        {
                            throw new PushServerException(5, pushData);
                        }
                }
            }
            if (PushServerTranslator.protLogger.IsDebugEnabled)
                PushServerTranslator.protLogger.Debug("Read " + (object)pushData1);
            return pushData1;
        }

        private static string DeUNIcode(string src)
        {
            int length = src.Length;
            char[] charArray = src.ToCharArray();
            StringBuilder stringBuilder = new StringBuilder(length);
            int startIndex = 0;
            int index;
            while (true)
            {
                index = startIndex;
                while (index < length && charArray[index] != '\\')
                    ++index;
                if (index < length)
                {
                    if (index + 6 <= length && charArray[index + 1] == 'u')
                    {
                        string str = new string(charArray, index + 2, 4);
                        try
                        {
                            int int32 = Convert.ToInt32(str, 16);
                            charArray[index] = (char)int32;
                        }
                        catch (Exception ex)
                        {
                            throw new PushServerException(5, src);
                        }
                        stringBuilder.Append(charArray, startIndex, index + 1 - startIndex);
                        startIndex = index + 6;
                    }
                    else
                        break;
                }
                else
                    goto label_11;
            }
            PushServerTranslator.protLogger.Debug("Encoding error in received answer");
            throw new PushServerException(5, src);
        label_11:
            stringBuilder.Append(charArray, startIndex, index - startIndex);
            return stringBuilder.ToString();
        }
    }
}