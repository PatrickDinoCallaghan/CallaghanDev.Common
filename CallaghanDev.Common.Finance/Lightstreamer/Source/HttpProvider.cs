// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.HttpProvider
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;

namespace Lightstreamer.DotNet.Client
{
  internal class HttpProvider
  {
    private string address;
    private string request;
    private CookieContainer cookies;
    private IDictionary<string, string> extraHeaders;
    private int httpConnectTimeout = -1;
    private int httpReadTimeout = -1;
    private static ILog streamLogger = LogManager.GetLogger("com.lightstreamer.ls_client.stream");
    private static ILog protLogger = LogManager.GetLogger("com.lightstreamer.ls_client.protocol");

    public HttpProvider(
      string address,
      CookieContainer cookies,
      IDictionary<string, string> extraHeaders,
      int rt,
      int ct)
    {
      this.extraHeaders = extraHeaders;
      this.cookies = cookies;
      this.address = address;
      this.httpReadTimeout = rt;
      this.httpConnectTimeout = ct;
    }

    private int GetTimeoutProp(string name) => name.Equals("Read", StringComparison.CurrentCultureIgnoreCase) ? (this.httpReadTimeout <= 0 ? -1 : this.httpReadTimeout) : (name.Equals("Connect", StringComparison.CurrentCultureIgnoreCase) && this.httpConnectTimeout > 0 ? this.httpConnectTimeout : -1);

    private static void SetHeader(HttpWebRequest Request, string Header, string Value)
    {
      PropertyInfo declaredProperty = Request.GetType().GetTypeInfo().GetDeclaredProperty(Header.Replace("-", string.Empty));
      if (declaredProperty != null)
        declaredProperty.SetValue((object) Request, (object) Value, (object[]) null);
      else
        Request.Headers[Header] = Value;
    }

    private static void WriteCallback(IAsyncResult asynchronousResult)
    {
      HttpProvider.MyRequestState asyncState = (HttpProvider.MyRequestState) asynchronousResult.AsyncState;
      try
      {
        Stream requestStream = asyncState.request.EndGetRequestStream(asynchronousResult);
        asyncState.stream = requestStream;
      }
      catch (WebException ex)
      {
        asyncState.webException = ex;
      }
      catch (IOException ex)
      {
        asyncState.ioException = ex;
      }
      catch (SecurityException ex)
      {
        asyncState.webException = new WebException("Security exception", (Exception) ex);
      }
      catch (Exception ex)
      {
        asyncState.ioException = new IOException("Unexpected exception", ex);
      }
      finally
      {
        asyncState.allDone.Set();
      }
    }

    private static void ReadCallback(IAsyncResult asynchronousResult)
    {
      HttpProvider.MyRequestState asyncState = (HttpProvider.MyRequestState) asynchronousResult.AsyncState;
      try
      {
        WebResponse response = asyncState.request.EndGetResponse(asynchronousResult);
        asyncState.response = response;
      }
      catch (WebException ex)
      {
        asyncState.webException = ex;
      }
      catch (IOException ex)
      {
        asyncState.ioException = ex;
      }
      catch (SecurityException ex)
      {
        asyncState.webException = new WebException("Security exception", (Exception) ex);
      }
      catch (Exception ex)
      {
        asyncState.ioException = new IOException("Unexpected exception", ex);
      }
      finally
      {
        asyncState.allDone.Set();
      }
    }

    protected internal virtual bool AddLine(IDictionary parameters, long limit)
    {
      string str1 = this.HashToString(parameters);
      string str2 = this.request != null ? this.request + "\r\n" + str1 : str1;
      if (limit > 0L && (long) str2.Length > limit)
        return false;
      this.request = str2;
      return true;
    }

    protected internal static HttpWebRequest CreateWebRequest(
      string address,
      CookieContainer cookies,
      IDictionary<string, string> extraHeaders)
    {
      Uri uri = new Uri(address);
      HttpWebRequest Request = (HttpWebRequest) WebRequest.Create(uri);
      if (Request == null)
      {
        HttpProvider.streamLogger.Debug("Failed connection to " + address);
        throw new IOException("Connection failed");
      }
      try
      {
        if (extraHeaders != null)
        {
          foreach (KeyValuePair<string, string> extraHeader in (IEnumerable<KeyValuePair<string, string>>) extraHeaders)
          {
            try
            {
              if (extraHeader.Key.Equals("accept", StringComparison.OrdinalIgnoreCase))
                throw new IOException("Can't manually change the Accept header");
              if (extraHeader.Key.Equals("connection", StringComparison.OrdinalIgnoreCase))
                throw new IOException("Can't manually change the Connection header");
              if (extraHeader.Key.Equals("content-length", StringComparison.OrdinalIgnoreCase))
                throw new IOException("Can't manually change the Content-Length header");
              if (extraHeader.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                throw new IOException("Can't manually change the Content-Type header");
              if (extraHeader.Key.Equals("expect", StringComparison.OrdinalIgnoreCase))
              {
                HttpProvider.SetHeader(Request, "expect", extraHeader.Value);
              }
              else
              {
                if (extraHeader.Key.Equals("date", StringComparison.OrdinalIgnoreCase))
                  throw new IOException("Can't manually change the Date header");
                if (extraHeader.Key.Equals("host", StringComparison.OrdinalIgnoreCase))
                  throw new IOException("Can't manually change the Host header");
                if (extraHeader.Key.Equals("if-modified-since", StringComparison.OrdinalIgnoreCase))
                  throw new IOException("Can't manually change the If-Modified-Since header");
                if (extraHeader.Key.Equals("range", StringComparison.OrdinalIgnoreCase))
                  throw new IOException("Can't manually change the Range header");
                if (extraHeader.Key.Equals("referrer", StringComparison.OrdinalIgnoreCase))
                {
                  HttpProvider.SetHeader(Request, "referrer", extraHeader.Value);
                }
                else
                {
                  if (extraHeader.Key.Equals("transfer-encoding", StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Can't manually change the Transfer-Encoding header");
                  if (extraHeader.Key.Equals("user-agent", StringComparison.OrdinalIgnoreCase))
                    HttpProvider.SetHeader(Request, "user-agent", extraHeader.Value);
                  else if (extraHeader.Key.Equals("cookie", StringComparison.OrdinalIgnoreCase))
                  {
                    string str = extraHeader.Value;
                    char[] chArray = new char[1]{ ';' };
                    foreach (string cookieHeader in str.Split(chArray))
                      cookies.SetCookies(uri, cookieHeader);
                  }
                  else
                    Request.Headers[extraHeader.Key] = extraHeader.Value;
                }
              }
            }
            catch (IOException ex)
            {
              throw new IOException("Failure setting extra headers", (Exception) ex);
            }
            catch (InvalidOperationException ex)
            {
              throw new IOException("Failure setting extra headers", (Exception) ex);
            }
            catch (CookieException ex)
            {
              throw new IOException("Failure setting cookies", (Exception) ex);
            }
          }
        }
        Request.CookieContainer = cookies;
        Request.AllowReadStreamBuffering = false;
      }
      catch (NotImplementedException ex)
      {
      }
      return Request;
    }

    protected internal virtual HttpWebRequest SendPost()
    {
      HttpProvider.streamLogger.Debug("Opening connection to " + this.address);
      int timeoutProp = this.GetTimeoutProp("Connect");
      HttpWebRequest webRequest = HttpProvider.CreateWebRequest(this.address, this.cookies, this.extraHeaders);
      webRequest.Method = "POST";
      webRequest.ContentType = "application/x-www-form-urlencoded";
      HttpProvider.MyRequestState state = new HttpProvider.MyRequestState()
      {
        request = webRequest
      };
      if (webRequest.BeginGetRequestStream(new AsyncCallback(HttpProvider.WriteCallback), (object) state) == null)
        throw new IOException("Request submission failed unexpectedly (1)");
      if (!state.allDone.WaitOne(timeoutProp))
      {
        webRequest.Abort();
        throw new IOException("Connection timed out");
      }
      if (state.ioException != null)
        throw state.ioException;
      if (state.webException != null)
        throw state.webException;
      if (state.stream == null)
      {
        webRequest.Abort();
        throw new IOException("Request submission failed unexpectedly (2)");
      }
      Stream stream = state.stream;
      if (this.request != null)
      {
        if (HttpProvider.streamLogger.IsDebugEnabled)
          HttpProvider.streamLogger.Debug("Posting data: " + this.request);
        byte[] bytes = new UTF8Encoding().GetBytes(this.request);
        stream.Write(bytes, 0, bytes.Length);
      }
      stream.Flush();
      stream.Dispose();
      return webRequest;
    }

    protected internal virtual HttpWebRequest SendGet()
    {
      HttpProvider.streamLogger.Debug("Opening connection to " + this.address);
      string address = this.address;
      if (this.request != null)
        address = this.address + "?" + this.request;
      HttpWebRequest webRequest = HttpProvider.CreateWebRequest(address, this.cookies, this.extraHeaders);
      webRequest.Method = "GET";
      return webRequest;
    }

    public virtual Stream DoHTTP(IDictionary parameters, bool isPost)
    {
      this.AddLine(parameters, 0L);
      return this.DoHTTP(isPost);
    }

    internal virtual Stream DoHTTP(bool isPost)
    {
      int timeoutProp = this.GetTimeoutProp("Read");
      HttpWebRequest httpWebRequest = !isPost ? this.SendGet() : this.SendPost();
      WebResponse webResponse = (WebResponse) null;
      try
      {
        HttpProvider.MyRequestState state = new HttpProvider.MyRequestState();
        state.request = httpWebRequest;
        if (httpWebRequest.BeginGetResponse(new AsyncCallback(HttpProvider.ReadCallback), (object) state) == null)
          throw new IOException("Response gathering failed unexpectedly (1)");
        if (!state.allDone.WaitOne(timeoutProp))
        {
          httpWebRequest.Abort();
          throw new IOException("Connection timed out");
        }
        if (state.ioException != null)
          throw state.ioException;
        if (state.webException != null)
          throw state.webException;
        if (state.response == null)
        {
          httpWebRequest.Abort();
          throw new IOException("Response gathering failed unexpectedly (2)");
        }
        webResponse = state.response;
        Stream responseStream = webResponse.GetResponseStream();
        if (responseStream.CanTimeout)
          responseStream.ReadTimeout = timeoutProp;
        return responseStream;
      }
      catch (Exception ex1)
      {
        if (webResponse != null)
        {
          try
          {
            webResponse.Dispose();
          }
          catch (Exception ex2)
          {
          }
        }
        throw ex1;
      }
    }

    private string HashToString(IDictionary parameters)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string key in (IEnumerable) parameters.Keys)
      {
        string str1 = (string) parameters[(object) key] ?? "";
        string str2;
        try
        {
          int num1 = 32000;
          if (str1.Length <= num1)
          {
            str2 = Uri.EscapeDataString(str1);
          }
          else
          {
            string str3 = "";
            do
            {
              int[] combiningCharacters = StringInfo.ParseCombiningCharacters(str1);
              int index = 0;
              while (index < combiningCharacters.Length && combiningCharacters[index] <= num1)
                ++index;
              int length = combiningCharacters.Length;
              int num2 = combiningCharacters[index - 1];
              if (num2 == 0)
                num2 = num1;
              HttpProvider.protLogger.Debug("Encoding long value, extracting " + (object) num2 + " characters");
              str3 += Uri.EscapeDataString(str1.Substring(0, num2));
              str1 = str1.Substring(num2);
            }
            while (str1.Length > num1);
            HttpProvider.protLogger.Debug("Encoding long value on the last " + (object) str1.Length + " characters");
            str2 = str3 + Uri.EscapeDataString(str1);
          }
        }
        catch (Exception ex)
        {
          HttpProvider.protLogger.Debug("Error sending command", ex);
          throw new IOException("Encoding error");
        }
        if (stringBuilder.Length != 0)
          stringBuilder.Append("&");
        stringBuilder.Append(key);
        stringBuilder.Append("=");
        stringBuilder.Append(str2);
      }
      return stringBuilder.ToString();
    }

    private class MyRequestState
    {
      internal Stream stream;
      internal HttpWebRequest request;
      internal WebResponse response;
      internal WebException webException;
      internal IOException ioException;
      internal ManualResetEvent allDone = new ManualResetEvent(false);
    }
  }
}
