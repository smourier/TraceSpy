using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using TraceSpyService.Configuration;

namespace TraceSpyService
{
    public class Program : ServiceBase, IServiceHost
    {
        public const string DefaultName = "TraceSpySvc";
        public const string DefaultDisplayName = "TraceSpy Service";
        public const string DefaultDescription = "Provides remote ETW tracing for TraceSpy clients.";

        public static string OptionName; // defines the service name
        public static string OptionDisplayName; // defines the service display name
        public static string OptionDescription; // defines the service description
        public static bool OptionKillOnException; // kill brutally the service on exception
        public static bool OptionLogToConsole = true;
        public static bool OptionHelp; // display help
        public static bool OptionUninstall; // command to uninstall the service
        public static bool OptionInstall;// command to install the service
        public static bool OptionService; // run as service or console app?
        public static ServiceStartMode OptionStartType;
        public static ServiceAccount OptionAccount; // defines the service account to use
        public static string OptionUser; // defines the user who runs the service
        public static string OptionPassword; // defines the password of the user who runs the service
        public static Encoding OptionEncoding; // defines the console encoding to use
        public static bool OptionTrace; // use a Console Trace listener
        public static string OptionLcid; // defines the current thread Lcid
        public static string[] OptionDependsOn; // defines the service dependency
        public static string OptionConfigPath; // defines the service configuration path

        private delegate void ServiceCallDelegate(IService service);

        private static Encoding _oldEncoding;
        private static AutoResetEvent _closed;
        private static ServiceSection _configuration;
        internal static Program _service;
        private static bool _stopping;
        private List<IService> _services;
        internal EtwListenerService _listenerService;
        internal WebService _webService;

        private Program()
        {
            AppDomainMonitor.Start();
            _services = new List<IService>();

            _listenerService = new EtwListenerService(ServiceSection.Current.EtwListener.Capacity);
            _listenerService.Host = this;
            _listenerService.Name = "Etw Listener";
            _services.Add(_listenerService);

            _webService = new WebService();
            _webService.Host = this;
            _webService.Name = "Web Server";
            _services.Add(_webService);
        }

        static void Main(string[] args)
        {
            bool newConsole = ConsoleControl.AllocConsole();
            ConsoleControl.SetConsoleIcon(ConsoleControl.ApplicationIcon);

            // we need that early on (for error reporting)
            OptionName = CommandLineUtilities.GetArgument<string>(args, "name", DefaultName);
            OptionEncoding = CommandLineUtilities.GetArgument<Encoding>(args, "encoding", null);
            if (OptionEncoding != null)
            {
                _oldEncoding = Console.OutputEncoding;
                Console.WriteLine("Switching output encoding from '" + _oldEncoding.WebName + "' to '" + OptionEncoding.WebName + "'");
                Console.OutputEncoding = OptionEncoding;
            }

            _closed = new AutoResetEvent(true);
            _service = new Program();
            _service.AutoLog = false;
            _service.EventLog.Source = OptionName;
            SetErrorMode(SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS);
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            try
            {
                SafeMain(args);
            }
            catch (Exception e)
            {
                bool reported = false;
                if (_service != null)
                {
                    try
                    {
                        _service.EventLog.WriteEntry("Service Host '" + OptionName + "' was stopped due to an error: " + e, EventLogEntryType.Error);
                        reported = true;
                    }
                    catch
                    {
                    }
                }

                if (!reported)
                {
                    EventLog.WriteEntry(OptionName, "Service Host '" + OptionName + "' was stopped due to an error: " + e, EventLogEntryType.Error);
                }

                EventLog.WriteEntry(OptionName, "Service Host '" + OptionName + "' was stopped due to an error: " + e, EventLogEntryType.Error);
                Log("Fatal error. Details:");
                Log(e);
            }

            if (_oldEncoding != null)
            {
                Console.OutputEncoding = _oldEncoding;
            }

            if (newConsole)
            {
                ConsoleControl.FreeConsole();
            }
            ConsoleControl.SetConsoleIcon(0);
        }

        protected override void Dispose(bool disposing)
        {
            AppDomainMonitor.Stop();
            base.Dispose(disposing);
            Stop();
            foreach (IService service in _services)
            {
                IDisposable disposable = service as IDisposable;
                if (disposable != null)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // do nothing
                    }
                }
            }
        }

        public static ServiceSection Configuration
        {
            get
            {
                return _configuration;
            }
        }

        static void SafeMain(string[] args)
        {
            AddERExcludedApplication(Process.GetCurrentProcess().MainModule.ModuleName);
            Console.WriteLine("TraceSpy Service - " + (Environment.Is64BitProcess ? "64" : "32") + "-bit - Build Number " + Assembly.GetExecutingAssembly().GetInformationalVersion());
            Console.WriteLine("Copyright (C) SoftFluent S.A.S 2012-" + DateTime.Now.Year + ". All rights reserved.");

            var token = Extensions.GetTokenElevationType();
            if (token != TokenElevationType.Full)
            {
                Console.WriteLine("");
                Console.WriteLine("Warning: token elevation type (UAC level) is " + token + ". You may experience access denied errors from now on. You may fix these errors if you restart with Administrator rights or without UAC.");
                Console.WriteLine("");
            }

            OptionHelp = CommandLineUtilities.GetArgument(args, "?", false);
            if (!OptionHelp)
            {
                OptionHelp = CommandLineUtilities.GetArgument(args, "h", false);
                if (!OptionHelp)
                {
                    OptionHelp = CommandLineUtilities.GetArgument(args, "help", false);
                }
            }
            OptionService = CommandLineUtilities.GetArgument(args, "s", false);

            if (!OptionService)
            {
                if (OptionHelp)
                {
                    Console.WriteLine("Format is " + Assembly.GetExecutingAssembly().GetName().Name + ".exe [options]");
                    Console.WriteLine("[options] can be a combination of the following:");
                    Console.WriteLine("    /?                     Displays this help");
                    Console.WriteLine("    /i                     Installs the <name> service");
                    Console.WriteLine("    /k                     Kills this process on any exception");
                    Console.WriteLine("    /u                     Uninstalls the <name> service");
                    Console.WriteLine("    /t                     Displays traces on the console");
                    Console.WriteLine("    /l:<name>              Locale used");
                    Console.WriteLine("                           default is " + CultureInfo.CurrentCulture.LCID);
                    Console.WriteLine("    /name:<name>           (Un)Installation uses <name> for the service name");
                    Console.WriteLine("                           default is \"" + DefaultName + "\"");
                    Console.WriteLine("    /displayName:<dname>   (Un)Installation uses <dname> for the display name");
                    Console.WriteLine("                           default is \"" + DefaultDisplayName + "\"");
                    Console.WriteLine("    /description:<desc.>   Installation ses <desc.> for the service description");
                    Console.WriteLine("                           default is \"" + DefaultDisplayName + "\"");
                    Console.WriteLine("    /startType:<type>      Installation uses <type> for the service start mode");
                    Console.WriteLine("                           default is \"" + ServiceStartMode.Manual + "\"");
                    Console.WriteLine("                           Values are " + ServiceStartMode.Automatic + ", " + ServiceStartMode.Disabled + " or " + ServiceStartMode.Manual);
                    Console.WriteLine("    /user:<name>           Name of the account under which the service should run");
                    Console.WriteLine("                           default is Local System");
                    Console.WriteLine("    /password:<text>       Password to the account name");
                    Console.WriteLine("    /config:<path>         Path to the configuration file");
                    Console.WriteLine("    /dependson:<list>      A comma separated list of service to depend on");
                    Console.WriteLine("");
                    Console.WriteLine("Examples:");
                    Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name + " /i /name:MyService /displayName:\"My Service\" /startType:Automatic");
                    Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name + " /u /name:MyOtherService");
                    return;
                }
            }

            OptionTrace = CommandLineUtilities.GetArgument(args, "t", false);
            OptionKillOnException = CommandLineUtilities.GetArgument(args, "k", false);
            OptionInstall = CommandLineUtilities.GetArgument(args, "i", false);
            OptionStartType = (ServiceStartMode)CommandLineUtilities.GetArgument(args, "starttype", ServiceStartMode.Manual);
            OptionLcid = CommandLineUtilities.GetArgument<string>(args, "l", null);
            OptionUninstall = CommandLineUtilities.GetArgument(args, "u", false);
            OptionAccount = (ServiceAccount)CommandLineUtilities.GetArgument(args, "user", ServiceAccount.User);
            OptionPassword = CommandLineUtilities.GetArgument<string>(args, "password", null);
            OptionUser = CommandLineUtilities.GetArgument<string>(args, "user", null);
            string dependsOn = CommandLineUtilities.GetArgument<string>(args, "dependson", null);
            if (!string.IsNullOrEmpty(dependsOn))
            {
                OptionDependsOn = dependsOn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                OptionDependsOn = null;
            }
            OptionConfigPath = CommandLineUtilities.GetArgument<string>(args, "config", null);
            if (!string.IsNullOrEmpty(OptionConfigPath) && !Path.IsPathRooted(OptionConfigPath))
            {
                OptionConfigPath = Path.GetFullPath(OptionConfigPath);
            }
            _configuration = ServiceSection.Get(OptionConfigPath);

            OptionDisplayName = CommandLineUtilities.GetArgument(args, "displayname", DefaultDisplayName);
            OptionDescription = CommandLineUtilities.GetArgument(args, "description", DefaultDescription);

            if (OptionInstall)
            {
                ServiceInstaller si = new ServiceInstaller();
                ServiceProcessInstaller spi = new ServiceProcessInstaller();
                si.ServicesDependedOn = OptionDependsOn;
                Console.WriteLine("OptionAccount=" + OptionAccount);
                Console.WriteLine("OptionUser=" + OptionUser);
                if (OptionAccount == ServiceAccount.User)
                {
                    if (string.IsNullOrEmpty(OptionUser))
                    {
                        // the default
                        spi.Account = ServiceAccount.LocalService;
                    }
                    else
                    {
                        spi.Account = ServiceAccount.User;
                        if (string.IsNullOrWhiteSpace(OptionPassword))
                        {
                            Console.WriteLine();
                            Console.WriteLine("Password cannot be empty if Account is set to User.");
                            return;
                        }

                        spi.Username = OptionUser;
                        spi.Password = OptionPassword;
                    }
                }
                else
                {
                    spi.Account = OptionAccount;
                }

                si.Parent = spi;
                si.DisplayName = OptionDisplayName;
                si.Description = OptionDescription;
                si.ServiceName = OptionName;
                si.StartType = OptionStartType;
                si.Context = new InstallContext(Assembly.GetExecutingAssembly().GetName().Name + ".install.log", null);

                string asmpath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, AppDomain.CurrentDomain.FriendlyName);

                // TODO: add instance specific parameters here (ports, etc...)
                string binaryPath = "\"" + asmpath + "\""       // exe path
                    + " /s"                                     // we run as a service
                    + " /name:" + OptionName;                    // our name

                if (!string.IsNullOrEmpty(OptionConfigPath))
                {
                    binaryPath += " /c:\"" + OptionConfigPath + "\"";
                }

                si.Context.Parameters["assemblypath"] = binaryPath;

                IDictionary stateSaver = new Hashtable();
                si.Install(stateSaver);

                // see remarks in the function
                FixServicePath(si.ServiceName, binaryPath);
                return;
            }

            if (OptionUninstall)
            {
                ServiceInstaller si = new ServiceInstaller();
                ServiceProcessInstaller spi = new ServiceProcessInstaller();
                si.Parent = spi;
                si.ServiceName = OptionName;

                si.Context = new InstallContext(Assembly.GetExecutingAssembly().GetName().Name + ".uninstall.log", null);
                si.Uninstall(null);
                return;
            }

            if (!OptionService)
            {
                if (OptionTrace)
                {
                    Trace.Listeners.Add(new ConsoleListener());
                }

                if (!string.IsNullOrEmpty(OptionLcid))
                {
                    Extensions.SetCurrentThreadCulture(OptionLcid);
                }

                Console.WriteLine("Console Mode");
                Console.WriteLine("Service Host name: " + OptionName);
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                Console.WriteLine("Service Host identity: " + (identity != null ? identity.Name : "null"));
                Console.WriteLine("Service Host bitness: " + (IntPtr.Size == 4 ? "32-bit" : "64-bit"));
                Console.WriteLine("Service Host display name: '" + OptionDisplayName + "'");
                Console.WriteLine("Service Host event log source: " + _service.EventLog.Source);
                Console.WriteLine("Service Host trace enabled: " + OptionTrace);
                Console.WriteLine("Service Host administrator mode: " + IsAdministrator());

                string configPath = OptionConfigPath;
                if (configPath == null)
                {
                    configPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                }
                Console.WriteLine("Service Host config file path: " + configPath);
                Console.WriteLine("Service Host current locale: " + Thread.CurrentThread.CurrentCulture.LCID + " (" + Thread.CurrentThread.CurrentCulture.Name + ")");

                Console.Title = OptionDisplayName;

                ConsoleControl cc = new ConsoleControl();
                cc.Event += OnConsoleControlEvent;

                _service.InternalStart(args);
                if (!_stopping)
                {
                    _service.InternalStop();
                }
                else
                {
                    int maxWaitTime = Configuration.ConsoleCloseMaxWaitTime;
                    if (maxWaitTime <= 0)
                    {
                        maxWaitTime = Timeout.Infinite;
                    }
                    _closed.WaitOne(maxWaitTime, Configuration.WaitExitContext);
                }
                return;
            }

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { _service };
            Run(ServicesToRun);
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return identity != null && new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }

        protected override void OnStart(string[] args)
        {
            ThreadPool.QueueUserWorkItem(InternalStart, args);
        }

        protected override void OnStop()
        {
            InternalStop();
        }

        private static void ServiceStart(IService service)
        {
            try
            {
                service.Start();
            }
            catch (Exception e)
            {
                service.StartException = e;
            }
        }

        private static void ServiceStop(IService service)
        {
            try
            {
                service.Stop();
            }
            catch (Exception e)
            {
                service.StopException = e;
            }
        }

        private static void HandleServiceStarted(IService service)
        {
            if (service.StartException != null)
            {
                Log("Fatal error starting hosted service '" + service.Name + "'. Details:");
                Log(service.StartException);
                EventLog.WriteEntry(OptionName, "Service '" + service.Name + "' was not started due to an error: " + service.StartException, EventLogEntryType.Error);
                if (Configuration.StopOnServiceStartError)
                {
                    try
                    {
                        _closed.Reset();
                        return;
                    }
                    catch (AccessViolationException)
                    {
                        // Catches a strange error that occurs sometimes...
                        _closed.Reset();
                        return;
                    }
                }
            }
            else
            {
                Log("Service '" + service.Name + "' started successfully");
            }
        }

        private static void HandleServiceStopped(IService service)
        {
            if (service.StopException != null)
            {
                Log("Fatal error stopping hosted service '" + service.Name + "'. Details:");
                Log(service.StopException);
                EventLog.WriteEntry(OptionName, "Service '" + service.Name + "' was stopped with an error: " + service.StopException, EventLogEntryType.Error);
                // always continue
            }
            else
            {
                Log("Service '" + service.Name + "' stopped successfully");
            }
        }

        private void InternalStart(object args)
        {
            if (!string.IsNullOrEmpty(OptionLcid))
            {
                Extensions.SetCurrentThreadCulture(OptionLcid);
            }

            if (_services.Count == 0)
            {
                Log("Service Host '" + OptionName + "' has no hosted services configured. Stopping.");
                return;
            }

            if (Configuration.StartServicesAsync)
            {
                Log("Service Host '" + OptionName + "' starting asynchronously.");
                List<IAsyncResult> _results = new List<IAsyncResult>(_services.Count);
                List<WaitHandle> _waits = new List<WaitHandle>(_services.Count);
                foreach (IService service in _services)
                {
                    if (service.Status != ServiceControllerStatus.StartPending && service.Status != ServiceControllerStatus.Running)
                    {
                        Log("Service '" + service.Name + "' starting");
                        ServiceCallDelegate d = new ServiceCallDelegate(ServiceStart);
                        IAsyncResult result = d.BeginInvoke(service, null, service);
                        _results.Add(result);
                        _waits.Add(result.AsyncWaitHandle);
                    }
                }

                if (_waits.Count > 0)
                {
                    WaitHandle.WaitAll(_waits.ToArray(), Timeout.Infinite, Configuration.WaitExitContext);
                }

                int successCount = 0;
                foreach (IAsyncResult result in _results)
                {
                    IService service = (IService)result.AsyncState;
                    if (service.StartException == null)
                    {
                        successCount++;
                    }
                    HandleServiceStarted(service);

                    if (service.StartException is HttpListenerException)
                    {
                        Log("Vital service '" + service.Name + "' cannot be started. Stopping.");
                        return;
                    }
                }
                if (successCount == 0)
                {
                    Log("Service Host '" + OptionName + "' has no hosted services successfully started. Stopping.");
                    return;
                }
            }
            else
            {
                Log("Service Host '" + OptionName + "' starting");
                foreach (IService service in _services)
                {
                    try
                    {
                        if (service.Status != ServiceControllerStatus.StartPending && service.Status != ServiceControllerStatus.Running)
                        {
                            Log("Service '" + service.Name + "' starting");
                            service.Start();
                            Log("Service '" + service.Name + "' started");
                        }
                    }
                    catch (Exception e)
                    {
                        service.StartException = e;
                    }
                    HandleServiceStarted(service);
                }
            }

            Log("Service Host '" + OptionName + "' started");
            EventLog.WriteEntry("Service Host '" + OptionName + "' was started successfully.", EventLogEntryType.Information);
            _closed.Reset();

            if (!OptionService)
            {
                do
                {
                    ConsoleKeyInfo i;
                    try
                    {
                        i = Console.ReadKey(true);
                    }
                    catch (InvalidOperationException)
                    {
                        // ok, someone has killed the console, bail out
                        break;
                    }

                    if (i.Key == ConsoleKey.Enter || i.Key == ConsoleKey.Escape || i.KeyChar == 'q' || i.KeyChar == 'Q')
                        break;

                    if ((i.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                    {
                        if (i.Key == ConsoleKey.Pause || i.KeyChar == 'c' || i.KeyChar == 'C')
                            break;
                    }

                    if (i.KeyChar == 'c' || i.KeyChar == 'C')
                    {
                        Console.Clear();
                    }
                }
                while (true);
            }
        }

        private void InternalStop()
        {
            _stopping = true;
            // stop in reverse start order
            if (Configuration.StopServicesAsync)
            {
                int maxWaitTime = Configuration.StopServicesMaxWaitTime;
                if (maxWaitTime <= 0)
                {
                    maxWaitTime = Timeout.Infinite;
                }
                Log("Service Host '" + OptionName + "' stopping asynchronously. Max wait time: " + ((maxWaitTime == Timeout.Infinite) ? "infinite" : maxWaitTime + " ms"));
                List<IAsyncResult> _results = new List<IAsyncResult>(_services.Count);
                List<WaitHandle> _waits = new List<WaitHandle>(_services.Count);
                foreach (IService service in _services)
                {
                    ServiceControllerStatus status = service.Status;
                    if (status == ServiceControllerStatus.Running || status == ServiceControllerStatus.Paused)
                    {
                        Log("Service '" + service.Name + "' stopping");
                        ServiceCallDelegate d = new ServiceCallDelegate(ServiceStop);
                        IAsyncResult result = d.BeginInvoke(service, null, service);
                        _results.Add(result);
                        _waits.Add(result.AsyncWaitHandle);
                    }
                }

                if (_waits.Count > 0)
                {
                    WaitHandle.WaitAll(_waits.ToArray(), maxWaitTime, Configuration.WaitExitContext);
                }

                foreach (IAsyncResult result in _results)
                {
                    IService service = (IService)result.AsyncState;
                    HandleServiceStopped(service);
                }
            }
            else
            {
                Log("Service Host '" + OptionName + "' stopping synchronously");
                foreach (IService service in _services)
                {
                    ServiceControllerStatus status = service.Status;
                    if (status == ServiceControllerStatus.Running || status == ServiceControllerStatus.Paused)
                    {
                        try
                        {
                            Log("Service '" + service.Name + "' stopping");
                            service.Stop();
                            Log("Service '" + service.Name + "' stopped");
                        }
                        catch (Exception e)
                        {
                            service.StopException = e;
                        }
                        HandleServiceStopped(service);
                    }
                }
            }

            Log("Service Host '" + OptionName + "' stopped.");
            EventLog.WriteEntry("Service Host '" + OptionName + "' was stopped successfully.", EventLogEntryType.Information);
            _closed.Set();
        }

        private static void OnConsoleControlEvent(object sender, ConsoleControlEventArgs e)
        {
            if (_service != null)
            {
                Log("Service Host console closing.");
                _service.InternalStop();
                int maxWaitTime = Configuration.ConsoleCloseMaxWaitTime;
                if (maxWaitTime <= 0)
                {
                    maxWaitTime = Timeout.Infinite;
                }
                Log("Service Host console closing. Max wait time: " + ((maxWaitTime == Timeout.Infinite) ? "infinite" : maxWaitTime + " ms"));
                _closed.WaitOne(maxWaitTime, Configuration.WaitExitContext);
            }

            ConsoleControl.SetConsoleIcon(0);
            if (_oldEncoding != null)
            {
                Console.OutputEncoding = _oldEncoding;
            }
            Process.GetCurrentProcess().Kill();
        }

        private const int SEM_FAILCRITICALERRORS = 0x0001;
        private const int SEM_NOGPFAULTERRORBOX = 0x0002;

        [DllImport("kernel32.dll")]
        private extern static int SetErrorMode(int mode);

        [DllImport("faultrep.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private extern static bool AddERExcludedApplication(string applicationName);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private extern static IntPtr OpenSCManager(string machineName, string databaseName, int desiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private extern static bool CloseServiceHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private extern static IntPtr OpenService(IntPtr scm, string serviceName, int desiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private extern static bool ChangeServiceConfig(IntPtr serviceHandle, int serviceType, int startType, int errorControl,
            string binaryPath, string loadOrderGroup, IntPtr pTagId, char[] dependencies, string userName,
            string password, string displayName);

        private const int SC_MANAGER_ALL_ACCESS = 0xF003F;
        private const int SERVICE_CHANGE_CONFIG = 0x0002;
        private const int SERVICE_NO_CHANGE = -1; // 0xffffffff

        private static void FixServicePath(string serviceName, string binaryPath)
        {
            // NOTE: stupid .NET 2.0 ServiceInstaller class now puts quotes around assemblypath which breaks official SCM path + args syntax
            // this wasn't the case with .NET 1.1, so we have to fix this up the hard way (as ServiceController is not of any help for this)

            IntPtr scm = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scm == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            IntPtr service = IntPtr.Zero;
            try
            {
                service = OpenService(scm, serviceName, SERVICE_CHANGE_CONFIG);
                if (service == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // we only change the binaryPath
                if (!ChangeServiceConfig(service, SERVICE_NO_CHANGE, SERVICE_NO_CHANGE, SERVICE_NO_CHANGE,
                    binaryPath,
                    null, IntPtr.Zero, null, null, null, null))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                if (service != IntPtr.Zero)
                {
                    CloseServiceHandle(service);
                }

                if (scm != IntPtr.Zero)
                {
                    CloseServiceHandle(scm);
                }
            }
        }

        void IServiceHost.Log(IService service, object value)
        {
            Log(service, value);
        }

        public static void Log(object value)
        {
            Log(null, value);
        }

        public static void Log(IService service ,object value)
        {
            if (!OptionService)
            {
                if (OptionLogToConsole)
                {
                    try
                    {
                        if (service != null && service.Name != null)
                        {
                            Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "][" + service.Name + "]: " + value);
                        }
                        else
                        {
                            Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]: " + value);
                        }
                    }
                    catch
                    {
                        ConsoleControl.AllocConsole();
                        try
                        {
                            if (service != null && service.Name != null)
                            {
                                Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "][" + service.Name + "]: " + value);
                            }
                            else
                            {
                                Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]: " + value);
                            }
                        }
                        catch
                        {
                            // do nothing
                        }
                    }
                }
            }
            else
            {
                if (service != null && service.Name != null)
                {
                    Trace.WriteLine(value, service.Name);
                }
                else
                {
                    Trace.WriteLine(value);
                }
            }
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ConsoleControl.SetConsoleIcon(0);
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                Log("Fatal error. Details:");
                Log(ex);
                EventLog.WriteEntry(OptionName, "Service Host '" + OptionName + "' was stopped due to a fatal error: " + ex, EventLogEntryType.Error);
            }
            else
            {
                Log("Fatal error.");
                EventLog.WriteEntry(OptionName, "Service Host '" + OptionName + "' was stopped due to a fatal error.", EventLogEntryType.Error);
            }

            if (_oldEncoding != null)
            {
                Console.OutputEncoding = _oldEncoding;
            }

            if (OptionKillOnException)
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
