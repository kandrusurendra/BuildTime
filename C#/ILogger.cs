using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    public enum LogLevel
    {
        DebugInfo = 0,
        UserInfo,
        Warning,
        Error
    }

    public interface ILogger
    {
        void LogMessage(string msg, LogLevel level);
    }

    public class NullLogger : ILogger
    {
        public void LogMessage(string msg, LogLevel level)
        {
            // do nothing...
        }
    }
}
