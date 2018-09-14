using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Design;
//using System.Diagnostics;
//using System.Globalization;
//using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShell = Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
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
    }


    public class EventRouter : IEventRouter
    {
        public event System.EventHandler BuildStarted = (sender, args) => { };
        public event System.EventHandler BuildCompleted = (sender, args) => { };
        public event System.EventHandler<OutputWndEventArgs> OutputPaneUpdated = (sender, args) => { };

        public EventRouter(MsVsShell.Package package)
        {
            IServiceContainer serviceContainer = package as IServiceContainer;
            var dte = serviceContainer.GetService(typeof(SDTE)) as EnvDTE.DTE;
            this.buildEvents = dte.Events.BuildEvents;
            this.outputWndEvents = dte.Events.OutputWindowEvents;

            this.buildEvents.OnBuildBegin += this.OnBuildBegin;
            this.buildEvents.OnBuildDone += this.OnBuildCompleted;
            this.outputWndEvents.PaneUpdated += this.OnOutputPaneUpdated;
        }

        private void OnBuildBegin(EnvDTE.vsBuildScope sc, EnvDTE.vsBuildAction ac)
        {
            BuildStarted(this, new EventArgs());
        }

        private void OnBuildCompleted(EnvDTE.vsBuildScope sc, EnvDTE.vsBuildAction ac)
        {
            BuildCompleted(this, new EventArgs());
        }

        private void OnOutputPaneUpdated(OutputWindowPane wndPane)
        {
            var args = new OutputWndEventArgs();
            args.WindowPane = wndPane;
            OutputPaneUpdated(this, args);
        }

        
        private BuildEvents buildEvents;
        private OutputWindowEvents outputWndEvents;

        //private Events2 events;
        //private PublishEvents publishEvents;

    }
}
