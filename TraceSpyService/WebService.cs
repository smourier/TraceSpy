using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Web;
using TraceSpyService.Configuration;

namespace TraceSpyService
{
    public class WebService : IService, IDisposable
    {
        private const string TracesRequest = "traces";
        private const string DiagnosticsRequest = "diag";

        private HttpListener _listener;

        public WebService()
        {
            _listener = new HttpListener();
        }

        public virtual void Dispose()
        {
            if (_listener != null)
            {
                _listener.Close();
                _listener = null;
            }
        }

        public void Start()
        {
            if (Status == ServiceControllerStatus.Running)
                return;

            Status = ServiceControllerStatus.StartPending;
            try
            {
                _listener.IgnoreWriteExceptions = ServiceSection.Current.WebServer.IgnoreWriteExceptions;
                _listener.UnsafeConnectionNtlmAuthentication = ServiceSection.Current.WebServer.UnsafeConnectionNtlmAuthentication;
                _listener.AuthenticationSchemes = ServiceSection.Current.WebServer.AuthenticationSchemes;
                if (!string.IsNullOrWhiteSpace(ServiceSection.Current.WebServer.Realm))
                {
                    _listener.Realm = ServiceSection.Current.WebServer.Realm;
                }

                if (_listener.Realm != null)
                {
                    Host.Log(this, "Realm: " + _listener.Realm);
                }
                Host.Log(this, "Authentication schemes: " + _listener.AuthenticationSchemes);

                foreach (PrefixElement prefix in ServiceSection.Current.WebServer.Prefixes)
                {
                    if (!prefix.Enabled)
                        continue;

                    try
                    {
                        _listener.Prefixes.Add(prefix.Uri);
                    }
                    catch (Exception e)
                    {
                        throw new TraceSpyServiceException("Cannot add prefix '" + prefix.Uri + "' to web server.", e);
                    }

                    Host.Log(this, "Now listening on '" + prefix.Uri + "'.");
                }

                _listener.Start();
                _listener.BeginGetContext(ListenerCallback, null);
            }
            catch
            {
                Status = ServiceControllerStatus.Stopped;
                throw;
            }

            Status = ServiceControllerStatus.Running;
        }

        public void Stop()
        {
            if (Status == ServiceControllerStatus.Stopped)
                return;

            Status = ServiceControllerStatus.StopPending;
            try
            {
                if (_listener.IsListening)
                {
                    _listener.Stop();
                }
            }
            catch
            {
                throw;
            }
            Status = ServiceControllerStatus.Stopped;
        }

        public void Pause()
        {
            Status = ServiceControllerStatus.Paused;
        }

        public void Continue()
        {
            Status = ServiceControllerStatus.Running;
        }

        public ServiceControllerStatus Status { get; private set; }
        public IServiceHost Host { get; set; }
        public string Name { get; set; }
        public Exception StartException { get; set; }
        public Exception StopException { get; set; }

        // from winerror.h
        private const int ERROR_NETNAME_DELETED = 64;
        private const int ERROR_CONNECTION_INVALID = 1229;

        private static bool IsDeadClient(HttpListenerException he)
        {
            return (he.ErrorCode == ERROR_NETNAME_DELETED || he.ErrorCode == ERROR_CONNECTION_INVALID);
        }

        private void Write(HttpListenerResponse response, Exception e)
        {
            SafeWrite(response, e.Message, (int)HttpStatusCode.InternalServerError, "Internal server error");
        }

        private void Write304(HttpListenerResponse response)
        {
            SafeWrite(response, null, (int)HttpStatusCode.NotModified, "Not Modified.");
        }

        private void Write404(HttpListenerResponse response, string fileName)
        {
            string text = "TraceSpy Service - " + (Environment.Is64BitProcess ? "64" : "32") + "-bit - Build Number " + Assembly.GetExecutingAssembly().GetInformationalVersion() + "<hr/>";
            SafeWrite(response, text + "File '" + fileName + "' was not found.", (int)HttpStatusCode.NotFound, "Not found");
        }

        private void SafeWrite(HttpListenerResponse response, string htmlBody, int statusCode, string statusDescription)
        {
            if (Debugger.IsAttached)
            {
                Write(response, htmlBody, statusCode, statusDescription);
                return;
            }

            try
            {
                Write(response, htmlBody, statusCode, statusDescription);
            }
            catch (HttpListenerException he)
            {
                // client is probably dead ...
                Host.Log(this, "Exception (code: " + he.ErrorCode + "/" + he.NativeErrorCode + "): " + he);
            }
            catch (Exception ex)
            {
                Host.Log(this, "Exception: " + ex);
            }
        }

        private void Write(HttpListenerResponse response, string htmlBody, int statusCode, string statusDescription)
        {
            try
            {
                response.StatusCode = statusCode;
                response.StatusDescription = statusDescription;
                using (StreamWriter writer = new StreamWriter(response.OutputStream))
                {
                    if (htmlBody != null)
                    {
                        writer.Write("<html><head><link rel=\"stylesheet\" type=\"text/css\" href=\"TraceSpy.css\"></head>");
                        writer.Write(htmlBody.Replace(Environment.NewLine, "<br/>"));
                        writer.Write("</body></html>");
                    }
                }
            }
            catch (HttpListenerException he)
            {
                // client is probably dead ...
                Host.Log(this, "Exception (code: " + he.ErrorCode + "/" + he.NativeErrorCode + "): " + he);
            }
            catch (Exception ex)
            {
                Host.Log(this, "Exception: " + ex);
            }
        }

        private bool OnAuthenticate(HttpListenerContext context)
        {
            if (context.User == null || context.User.Identity == null)
                return false;

            if (!string.IsNullOrWhiteSpace(ServiceSection.Current.WebServer.Login))
            {
                HttpListenerBasicIdentity basic = context.User.Identity as HttpListenerBasicIdentity;
                if (basic != null)
                {
                    Host.Log(this, "OnAuthenticate Basic name: " + basic.Name);
                    if (basic.Name == ServiceSection.Current.WebServer.Login &&
                        basic.Password == ServiceSection.Current.WebServer.Password)
                    {
                        Host.Log(this, "OnAuthenticate Basic ok");
                        return true;
                    }
                }
            }

            if (context.User.Identity.IsAuthenticated)
            {
                Host.Log(this, "OnAuthenticate identity (" + context.User.Identity.GetType().FullName + ") name: " + context.User.Identity.Name);
                return true;
            }

            Host.Log(this, "OnAuthenticate failed");
            return false;
        }

        private void ListenerCallback(IAsyncResult result)
        {
            //Host.Log(this, "ListenerCallback result: " + result);
            if (!_listener.IsListening)
                return;

            if (!string.IsNullOrEmpty(Program.OptionLcid))
            {
                Extensions.SetCurrentThreadCulture(Program.OptionLcid);
            }

            _listener.BeginGetContext(ListenerCallback, null);
            
            HttpListenerContext context = _listener.EndGetContext(result);

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (_listener.AuthenticationSchemes != AuthenticationSchemes.Anonymous)
            {
                if (!OnAuthenticate(context))
                {
                    Host.Log(this, "Request Access is denied");
                    SafeWrite(response, "Access is denied", (int)HttpStatusCode.Unauthorized, "Access is denied");
                    response.OutputStream.Close();
                    return;
                }
            }

            //Host.Log(this, "AssemblyDate: " + Extensions.AssemblyDate);
            //Host.Log(this, "Request LocalEndPoint: " + request.LocalEndPoint);
            //Host.Log(this, "Request ServiceName: " + request.ServiceName);
            Host.Log(this, "Request Method: " + request.HttpMethod);
            //Host.Log(this, "Request RemoteEndPoint: " + request.RemoteEndPoint);
            Host.Log(this, "Request Url: " + request.Url);
            //Host.Log(this, "Request RawUrl: " + request.RawUrl);
            foreach (string name in request.Headers)
            {
                Host.Log(this, "Header " + name + ": " + request.Headers[name]);
            }

            if (request.Url == null)
                return;

            Host.Log(this, "Request RelativeUrl: " + ServiceSection.Current.WebServer.Prefixes.GetRelativePath(request.RawUrl));

            try
            {
                context.Response.AddHeader("Server", "TraceSpyServer/" + Assembly.GetExecutingAssembly().GetInformationalVersion() + "/");
                ProcessRequest(context);
            }
            catch (HttpListenerException he)
            {
                Host.Log(this, "HttpListenerException (code: " + he.ErrorCode + "/" + he.NativeErrorCode + "): " + he);
                if (IsDeadClient(he))
                {
                    // don't log this
                    Host.Log(this, "Client is dead.");
                    return;
                }
                Write(response, he);
                if (Debugger.IsAttached)
                    throw;
            }
            catch (Exception e)
            {
                Host.Log(this, "Exception: " + e);
                Write(response, e);
                if (Debugger.IsAttached)
                    throw;
            }

            try
            {
                response.OutputStream.Close();
            }
            catch (HttpListenerException he)
            {
                // client may have disappeared
                Host.Log(this, "HttpListenerException (code: " + he.ErrorCode + "/" + he.NativeErrorCode + "): " + he.Message);
            }
            catch (Exception e)
            {
                Host.Log(this, "write Exception:" + e.Message);
                if (Debugger.IsAttached)
                    throw;
            }
        }

        private string GetContentType(HttpListenerContext context)
        {
            string contentType = null;
            if (context.Request.RawUrl.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "application/json";
            }
            else if (context.Request.RawUrl.EndsWith("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "image/x-icon";
            }
            else if (context.Request.RawUrl.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "text/html";
            }
            else if (context.Request.RawUrl.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "text/css";
            }
            else if (context.Request.RawUrl.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "text/javascript";
            }
            return contentType;
        }

        //private void WriteAbout(HttpListenerContext context)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("<h1>TraceSpy Service - " + (Environment.Is64BitProcess ? "64" : "32") + "-bit - Build Number " + Assembly.GetExecutingAssembly().GetInformationalVersion() + "</h1>");
        //    sb.AppendLine("Copyright (C) SoftFluent S.A.S 2012-" + DateTime.Now.Year + ". All rights reserved.");
        //    sb.Append("<table cellspacing=\"0\">");
        //    AppendRow(sb, "Listener Buffer Capacity", Program._service._listenerService.Buffer.Capacity);
        //    AppendRow(sb, "Listener Buffer Count", Program._service._listenerService.Buffer.Count);
        //    AppendRow(sb, "Listener Buffer TotalCount", Program._service._listenerService.Buffer.TotalCount);
        //    sb.Append("</table>");

        //    Write(context.Response, sb.ToString(), 200, "OK");
        //}

        //private static void AppendRow(StringBuilder sb, string key, object value)
        //{
        //    sb.Append("<tr><td>");
        //    sb.Append(key);
        //    sb.Append("</td><td>");
        //    sb.Append(HttpUtility.HtmlEncode(string.Format(CultureInfo.InstalledUICulture, "{0}", value)));
        //    sb.Append("</td></tr>");
        //}

        private void WriteAssemblyStream(HttpListenerContext context, string fileName)
        {
            using (Stream stream = typeof(WebService).Assembly.GetManifestResourceStream("TraceSpyService.html." + fileName))
            {
                if (stream == null)
                {
                    Write404(context.Response, fileName);
                    return;
                }

                DateTime ifModifiedSince = DateTime.MaxValue;
                string ifm = context.Request.Headers["If-Modified-Since"];
                //Host.Log(this, "WriteAssemblyStream ifm: " + ifm);
                if (!string.IsNullOrEmpty(ifm))
                {
                    try
                    {
                        ifModifiedSince = DateTime.Parse(ifm, DateTimeFormatInfo.InvariantInfo);
                    }
                    catch
                    {
                        // do nothing
                    }

                    //Host.Log(this, "WriteAssemblyStream ifModifiedSince: " + ifModifiedSince + " asm:" + Extensions.AssemblyDate.TruncateMilliseconds() + " same:" + (ifModifiedSince == Extensions.AssemblyDate.TruncateMilliseconds()));
                    if (ifModifiedSince == Extensions.AssemblyDate.ToLocalTime().TruncateMilliseconds())
                    {
                        Write304(context.Response);
                        return;
                    }
                }

                context.Response.Headers[HttpResponseHeader.LastModified] = Extensions.AssemblyDate.ToString("r", DateTimeFormatInfo.InvariantInfo);

                stream.CopyTo(context.Response.OutputStream);
            }
        }

        [DataContract]
        private class JsonDiag
        {
            [DataMember]
            public string Status { get; set; }

            [DataMember]
            public string Version { get; set; }
            
            [DataMember]
            public string Time { get; set; }

            [DataMember]
            public string AssemblyDate { get; set; }

            [DataMember]
            public string ClrVersion { get; set; }

            [DataMember]
            public string OSVersion { get; set; }
        
            [DataMember]
            public int ProcessorCount { get; set; }

            [DataMember]
            public string WorkingSet { get; set; }

            [DataMember]
            public string CpuUsage { get; set; }

            [DataMember]
            public string MemoryUsage { get; set; }

            [DataMember]
            public string SessionId { get; set; }

            [DataMember]
            public int BufferCapacity { get; set; }
            
            [DataMember]
            public int BufferCount { get; set; }

            [DataMember]
            public long BufferTotalCount { get; set; }

            [DataMember]
            public JsonPrefix[] Prefixes { get; set; }

            [DataMember]
            public JsonEtwProvider[] EtwProviders { get; set; }
        }

        [DataContract]
        private class JsonPrefix
        {
            [DataMember]
            public string Uri { get; set; }

            [DataMember]
            public bool Enabled { get; set; }

            [DataMember]
            public string BasePath { get; set; }
        }

        [DataContract]
        private class JsonEtwProvider
        {
            [DataMember]
            public string Description { get; set; }

            [DataMember]
            public bool Enabled { get; set; }

            [DataMember]
            public EtwTraceLevel TraceLevel { get; set; }
        
            [DataMember]
            public string Guid { get; set; }
        }

        [DataContract]
        private class JsonTraces
        {
            [DataMember]
            public string SessionId { get; set; }

            [DataMember]
            public int BufferCapacity { get; set; }

            [DataMember]
            public int BufferCount { get; set; }

            [DataMember]
            public long BufferTotalCount { get; set; }

            [DataMember]
            public long LostCount { get; set; }

            [DataMember]
            public EtwRecord[] Records { get; set; }
        }

        private void SafeProcessJsonRequest(HttpListenerContext context, string path)
        {
            if (Debugger.IsAttached)
            {
                ProcessJsonRequest(context, path);
                return;
            }

            try
            {
                ProcessJsonRequest(context, path);
            }
            catch (HttpListenerException he)
            {
                // client is probably dead ...
                Host.Log(this, "Exception (code: " + he.ErrorCode + "/" + he.NativeErrorCode + "): " + he);
            }
            catch (Exception ex)
            {
                Host.Log(this, "Exception: " + ex);
            }
        }

        private void ProcessJsonRequest(HttpListenerContext context, string path)
        {
            context.Response.AddHeader("Cache-Control", "no-cache");
            if (path.StartsWith(DiagnosticsRequest, StringComparison.OrdinalIgnoreCase))
            {
                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(JsonDiag));
                JsonDiag diag = new JsonDiag();

                diag.Version = Assembly.GetExecutingAssembly().GetInformationalVersion() + " - " + Assembly.GetExecutingAssembly().GetConfiguration();
                diag.Time = DateTime.UtcNow.ToString("r");
                diag.AssemblyDate = Extensions.AssemblyDate.ToString("r");
                diag.OSVersion = Environment.OSVersion.ToString();
                diag.ClrVersion = Environment.Version.ToString() + " - " + (IntPtr.Size == 8 ? "64" : "32") + "-bit";
                diag.ProcessorCount = Environment.ProcessorCount;
                diag.WorkingSet = Extensions.FormatFileSize(Environment.WorkingSet);
                diag.CpuUsage = AppDomain.MonitoringIsEnabled ? AppDomainMonitor.CpuUsage + " %" : "n/a";
                diag.MemoryUsage = AppDomain.MonitoringIsEnabled ? Extensions.FormatFileSize(AppDomainMonitor.MemoryUsage) : "n/a";

                diag.BufferCapacity = Program._service._listenerService.Buffer.Capacity;
                diag.BufferCount = Program._service._listenerService.Buffer.Count;
                diag.BufferTotalCount = Program._service._listenerService.Buffer.TotalCount;
                diag.SessionId = Program._service._listenerService.SessionId.ToString("N");

                List<JsonPrefix> prefixes = new List<JsonPrefix>();
                foreach (PrefixElement prefix in ServiceSection.Current.WebServer.Prefixes)
                {
                    JsonPrefix jp = new JsonPrefix();
                    jp.Enabled = prefix.Enabled;
                    jp.Uri = prefix.Uri;
                    jp.BasePath = prefix.BasePath;
                    prefixes.Add(jp);
                }
                diag.Prefixes = prefixes.ToArray();

                List<JsonEtwProvider> providers = new List<JsonEtwProvider>();
                foreach (EtwProviderElement provider in ServiceSection.Current.EtwListener.Providers)
                {
                    JsonEtwProvider jpr = new JsonEtwProvider();
                    jpr.Enabled = provider.Enabled;
                    jpr.Description = provider.Description;
                    jpr.Guid = provider.Guid.ToString("N");
                    jpr.TraceLevel = provider.TraceLevel;
                    providers.Add(jpr);
                }
                diag.EtwProviders = providers.ToArray();

                s.WriteObject(context.Response.OutputStream, diag);
            }
            else if (path.StartsWith(TracesRequest, StringComparison.OrdinalIgnoreCase))
            {
                Guid sessionId;
                long startIndex = GetStartIndex(path, out sessionId);
                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(JsonTraces));
                JsonTraces traces = new JsonTraces();
                traces.BufferCapacity = Program._service._listenerService.Buffer.Capacity;
                traces.BufferCount = Program._service._listenerService.Buffer.Count;
                traces.BufferTotalCount = Program._service._listenerService.Buffer.TotalCount;
                traces.SessionId = Program._service._listenerService.SessionId.ToString("N");

                long lostCount;
                long totalCount;
                if (sessionId != Program._service._listenerService.SessionId)
                {
                    Console.WriteLine("new session");
                    startIndex = 0;
                    lostCount = -1; // new session
                    totalCount = -1;
                }
                else
                {
                    // an error here also sets lostcount to -1
                    int maxCount = 1000;
                    traces.Records = Program._service._listenerService.Buffer.GetTail(startIndex, maxCount, false, out totalCount, out lostCount);
                }
                Console.WriteLine("start index:" + startIndex + " totalCount:" + totalCount + " lostCount:" + lostCount + " records:" + (traces.Records != null ? traces.Records.Length : 0));

                traces.LostCount = lostCount;
                s.WriteObject(context.Response.OutputStream, traces);
            }
            else
            {
                Write404(context.Response, path);
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json; charset=utf-8";
        }

        private static long GetStartIndex(string path, out Guid sessionId)
        {
            sessionId = Guid.Empty;
            string newPath = path.Substring(TracesRequest.Length);
            if (string.IsNullOrWhiteSpace(newPath))
                return 0;

            while (newPath.StartsWith("/"))
            {
                newPath = newPath.Substring(1);
            }

            int pos = newPath.IndexOf('/');
            if (pos > 0)
            {
                Guid.TryParse(newPath.Substring(pos + 1), out sessionId);
                newPath = newPath.Substring(0, pos);
            }
            long index;
            long.TryParse(newPath, out index);
            return index;
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string contentType = GetContentType(context);
            if (contentType != null)
            {
                context.Response.ContentType = contentType;
            }

            if (context.Request.RawUrl.EndsWith("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                WriteAssemblyStream(context, "favicon.ico");
                return;
            }

            string path = ServiceSection.Current.WebServer.Prefixes.GetRelativePath(context.Request.RawUrl);
            if (path == null)
            {
                Write404(context.Response, context.Request.RawUrl);
                return;
            }

            const string jsonToken = "json/";
            if (path.StartsWith(jsonToken, StringComparison.OrdinalIgnoreCase))
            {
                SafeProcessJsonRequest(context, path.Substring(jsonToken.Length).Trim());
                return;
            }

            WriteAssemblyStream(context, path); 
        }
    }
}
