using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreWars.Logging
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogError(string message, Exception exc);
    }

    public class AppLogger : ILogger
    {
        public void LogError(string message, Exception exc)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(string message)
        {
            System.Diagnostic.
        }
    }
}
