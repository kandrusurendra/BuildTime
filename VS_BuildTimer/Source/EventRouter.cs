using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShell = Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

namespace VSBuildTimer
{
    public class EventRouter : IEventRouter
    {
        public event System.EventHandler BuildStarted = (sender, args) => { };
        public event System.EventHandler BuildCompleted = (sender, args) => { };
        public event System.EventHandler<OutputWndEventArgs> OutputPaneUpdated = (sender, args) => { };
        public event System.EventHandler OnShutdown = (sender, args) => { };

        public EventRouter(BuildTimerPackage package, EnvDTE.DTE dte)
        {
            this.buildEvents = dte.Events.BuildEvents;
            this.outputWndEvents = dte.Events.OutputWindowEvents;

            this.buildEvents.OnBuildBegin += this.OnBuildBeginHandler;
            this.buildEvents.OnBuildDone += this.OnBuildCompletedHandler;
            this.outputWndEvents.PaneUpdated += this.OnOutputPaneUpdatedHandler;
            package.OnQueryClose += this.OnShutdownHandler;
        }

        private void OnBuildBeginHandler(EnvDTE.vsBuildScope sc, EnvDTE.vsBuildAction ac)
        {
            BuildStarted(this, new EventArgs());
        }

        private void OnBuildCompletedHandler(EnvDTE.vsBuildScope sc, EnvDTE.vsBuildAction ac)
        {
            BuildCompleted(this, new EventArgs());
        }

        private void OnOutputPaneUpdatedHandler(OutputWindowPane wndPane)
        {
            var args = new OutputWndEventArgs
            {
                WindowPane = wndPane
            };
            OutputPaneUpdated(this, args);
        }

        private void OnShutdownHandler(System.Object sender, System.EventArgs args)
        {
            OnShutdown(sender, args);
        }
        
        private readonly BuildEvents buildEvents;
        private readonly OutputWindowEvents outputWndEvents;
    }
}
