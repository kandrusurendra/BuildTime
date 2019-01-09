using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShell = Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    class SDKBasedInfoExtractor : IVsUpdateSolutionEvents2, IBuildInfoExtractionStrategy
    {
        public SDKBasedInfoExtractor(MsVsShell.Package package, IVsSolutionBuildManager2 buildManager, ILogger logger = null)
        {
            this.m_buildManager = buildManager;
            this.m_buildManager.AdviseUpdateSolutionEvents(this, out uint cookie);
            this.m_package = package;
            this.m_timer = new System.Timers.Timer(1000);
            this.m_timer.Elapsed += this.OnTimerTick;
            this.m_logger = logger;
            this.m_projectBuildInfo = new Dictionary<string, ProjectBuildInfo>();
        }

        public List<ProjectBuildInfo> GetBuildProgressInfo()
        {
            List<ProjectBuildInfo> projInfo = m_projectBuildInfo.Values.ToList();
            return projInfo;
        }

        public event EventHandler BuildInfoUpdated;

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            this.m_timer.Enabled = true;
            this.m_projectBuildInfo.Clear();
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            this.LogBuildEvent(pHierProj, pCfgProj, "Build started.");

            if (pHierProj != null)
            {
                pHierProj.GetCanonicalName((uint)VSConstants.VSITEMID.Root, out string canonicalName);
                pHierProj.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_Name, out object propVal);
                var name = propVal as string;
                int projIdx = m_projectBuildInfo.Count+1;
                var info = new ProjectBuildInfo(name, projIdx, System.DateTime.Now, null, null);
                m_projectBuildInfo[canonicalName] = info;
            }

            this.BuildInfoUpdated(this, null);

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            this.LogBuildEvent(pHierProj, pCfgProj, "Build completed.");
            if (pHierProj != null)
            {
                pHierProj.GetCanonicalName((uint)VSConstants.VSITEMID.Root, out string canonicalName);
                var info = m_projectBuildInfo[canonicalName];
                info.BuildDuration = System.DateTime.Now - info.BuildStartTime;
                info.BuildSucceeded = (fSuccess!=0);
            }

            this.BuildInfoUpdated(this, null);

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            this.m_timer.Enabled = false;
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
        {
            this.m_timer.Enabled = false;
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        private void OnTimerTick(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (this.m_projectBuildInfo!=null)
            {
                foreach (var kv in this.m_projectBuildInfo)
                {
                    ProjectBuildInfo info = kv.Value;
                    if (!info.BuildSucceeded.HasValue)
                    {
                        info.BuildDuration = System.DateTime.Now - info.BuildStartTime.Value;
                    }
                }

                this.BuildInfoUpdated(this, null);
            }
        }

        private void LogBuildEvent(IVsHierarchy pHierProj, IVsCfg pCfgProj, string msg)
        {
            if (m_logger!=null)
            {
                object projName = String.Empty;
                string configName = String.Empty;
                if (pHierProj != null)
                    pHierProj.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_Name, out projName);
                if (pCfgProj != null)
                    pCfgProj.get_DisplayName(out configName);

                string s = string.Format("{0} - {1}:  {2}", projName as string, configName, msg);
                m_logger.LogMessage(s, LogLevel.UserInfo);
            }
        }

        private Dictionary<string, ProjectBuildInfo> m_projectBuildInfo;
        private readonly IVsSolutionBuildManager2 m_buildManager;
        private readonly MsVsShell.Package m_package;
        private readonly System.Timers.Timer m_timer;
        private readonly ILogger m_logger;
    }
}
