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
            this.m_sbm = buildManager;
            this.m_sbm.AdviseUpdateSolutionEvents(this, out uint cookie);

            this.m_package = package;
            this.m_projectBuildInfo = new Dictionary<string, ProjectBuildInfo>();
        }

        public List<ProjectBuildInfo> GetBuildProgressInfo()
        {
            List<ProjectBuildInfo> projInfo = m_projectBuildInfo.Values.ToList();
            return projInfo;
        }

        private void Solution_Opened()
        {
            //solution = new BuildMonitor.Domain.Solution { Name = GetSolutionName() };
            //PrintLine("\nSolution loaded:  \t{0}", solution.Name);
            //PrintLine("{0}", 60.Times("-"));
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
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
            // This method is called when the entire solution is done building.
            return VSConstants.S_OK;
        }

        #region empty impl. of solution events interface

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
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        #endregion

        private Dictionary<string, ProjectBuildInfo> m_projectBuildInfo;
        //private IVsSolution2 m_vsSolution;
        private IVsSolutionBuildManager2 m_sbm;
        private MsVsShell.Package m_package;
    }
}
