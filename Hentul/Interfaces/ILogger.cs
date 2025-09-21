namespace Hentul.Interfaces
{
    public interface ILogger
    {
        public void SetUpLogger(string path);

        public void WriteLogsToFile(string message);
    }
}
