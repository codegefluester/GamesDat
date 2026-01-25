namespace GameasDat.Core.Writer
{
    public interface ISessionWriter : IDisposable
    {
        void Start(string filePath);
        void WriteFrame<T>(T data, long timestamp) where T : unmanaged;
        void Stop();
    }
}
