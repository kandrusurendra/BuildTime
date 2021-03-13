﻿using System;
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


    public class EventRouter : IEventRouter
    {
        public event System.EventHandler BuildStarted = (sender, args) => { };
        public event System.EventHandler BuildCompleted = (sender, args) => { };
        public event System.EventHandler<OutputWndEventArgs> OutputPaneUpdated = (sender, args) => { };
        public event System.EventHandler OnShutdown = (sender, args) => { };

        public EventRouter(VSBuildTimerPackage package, EnvDTE.DTE dte)
        {
            this.buildEvents = dte.Events.BuildEvents;
            this.outputWndEvents = dte.Events.OutputWindowEvents;

            this.buildEvents.OnBuildBegin += this.OnBuildBegin;
            this.buildEvents.OnBuildDone += this.OnBuildCompleted;
            this.outputWndEvents.PaneUpdated += this.OnOutputPaneUpdated;
            package.OnQueryClose += this.OnShutdown;
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
            var args = new OutputWndEventArgs
            {
                WindowPane = wndPane
            };
            OutputPaneUpdated(this, args);
        }

        
        private readonly BuildEvents buildEvents;
        private readonly OutputWindowEvents outputWndEvents;
    }
}
