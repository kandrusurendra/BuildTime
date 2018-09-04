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


namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    public interface IEventRouter
    {
        event System.EventHandler BuildStarted;
        event System.EventHandler BuildCompleted;
    }


    public class EventRouter : IEventRouter
    {
        public event System.EventHandler BuildStarted;
        public event System.EventHandler BuildCompleted;

        public EventRouter(MsVsShell.Package package)
        {
            IServiceContainer serviceContainer = package as IServiceContainer;
            var dte = serviceContainer.GetService(typeof(SDTE)) as EnvDTE.DTE;
            var buildEvts = dte.Events.BuildEvents;
            buildEvts.OnBuildBegin += this.OnBuildBegin;
            buildEvts.OnBuildDone += this.OnBuildCompleted;
        }

        private void OnBuildBegin(EnvDTE.vsBuildScope sc, EnvDTE.vsBuildAction ac)
        {
            BuildStarted(this, new EventArgs());
        }

        private void OnBuildCompleted(EnvDTE.vsBuildScope sc, EnvDTE.vsBuildAction ac)
        {
            BuildCompleted(this, new EventArgs());
        }


    }
}
