using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace VSBuildTimer
{
    public class OutputWndEventArgs : System.EventArgs
    {
        public OutputWindowPane WindowPane { get; set; }
    }

    public interface IEventRouter
    {
        event System.EventHandler BuildStarted;
        event System.EventHandler BuildCompleted;
        event System.EventHandler<OutputWndEventArgs> OutputPaneUpdated;
        event System.EventHandler OnShutdown;
    }
}
