using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using System.Windows;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    public class OutputWindowInfoExtractor : IBuildInfoExtractionStrategy
    {
        public List<ProjectBuildInfo> GetBuildProgressInfo()
        {
            EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(DTE));
            Debug.Assert(dte != null);
            OutputWindowPanes panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;

            OutputWindowPane pane = panes.Cast<OutputWindowPane>().First(wnd => wnd.Name == "Build");
            if (pane != null)
            {
                pane.Activate();
                pane.TextDocument.Selection.SelectAll();
                var buildOutputStr = pane.TextDocument.Selection.Text;

                return BuildInfoUtils.ExtractBuildInfo(buildOutputStr);
            }
            else
            {
                MessageBox.Show("Build output window not found");
                return null;
            }
        }
    }
}
