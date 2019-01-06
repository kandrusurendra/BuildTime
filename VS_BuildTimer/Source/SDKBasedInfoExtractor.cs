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
        public SDKBasedInfoExtractor(MsVsShell.Package package, IVsSolutionBuildManager2 buildManager)
        {
            this.m_buildManager = buildManager;
            this.m_buildManager.AdviseUpdateSolutionEvents(this, out uint cookie);
            this.m_package = package;
            this.m_timer = new System.Timers.Timer(1000);
            this.m_timer.Elapsed += this.OnTimerTick;
            this.m_projectBuildInfo = new Dictionary<string, ProjectBuildInfo>();
        }

        public List<ProjectBuildInfo> GetBuildProgressInfo()
        {
            List<ProjectBuildInfo> projInfo = m_projectBuildInfo.Values.ToList();
            return projInfo;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            this.m_timer.Enabled = true;
            this.m_projectBuildInfo.Clear();
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            if (pHierProj != null)
            {
                pHierProj.GetCanonicalName((uint)VSConstants.VSITEMID.Root, out string canonicalName);
                pHierProj.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_Name, out object propVal);
                var name = propVal as string;
                int projIdx = m_projectBuildInfo.Count+1;
                var info = new ProjectBuildInfo(name, projIdx, System.DateTime.Now, null, null);
                m_projectBuildInfo[canonicalName] = info;
            }
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            if (pHierProj != null)
            {
                pHierProj.GetCanonicalName((uint)VSConstants.VSITEMID.Root, out string canonicalName);
                var info = m_projectBuildInfo[canonicalName];
                info.BuildDuration = System.DateTime.Now - info.BuildStartTime;
                info.BuildSucceeded = (fSuccess!=0);
            }
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
            }
        }

        private Dictionary<string, ProjectBuildInfo> m_projectBuildInfo;
        private IVsSolutionBuildManager2 m_buildManager;
        private MsVsShell.Package m_package;
        private System.Timers.Timer m_timer;
    }
}
