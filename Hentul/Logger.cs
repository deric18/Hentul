using Hentul.Interfaces;
using OpenCvSharp;

namespace Hentul
{
    public class Logger : ILogger
    {
        public string Path = string.Empty;

        public Logger(string path)
        {
            Path = path;
        }
        public void SetUpLogger(string path)
        {
            Path = path;
        }

        public void WriteLogsToFile(string logMsg)
        {
            if (Path == string.Empty)
            {
                throw new InvalidOperationException("Path should not be empty");
            }

            File.WriteAllText(Path, logMsg);
        }
    }
}
