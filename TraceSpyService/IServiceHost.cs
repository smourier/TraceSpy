namespace TraceSpyService
{
    public interface IServiceHost
    {
        void Log(IService service, object value);
    }
}
