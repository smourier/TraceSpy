using System;
using System.ServiceProcess;

namespace TraceSpyService
{
    public interface IService
    {
        void Start();
        void Stop();
        void Pause();
        void Continue();
        ServiceControllerStatus Status { get;}
        IServiceHost Host { get;set;}
        string Name { get;set;}
        Exception StartException { get; set; }
        Exception StopException { get; set; }
    }
}
