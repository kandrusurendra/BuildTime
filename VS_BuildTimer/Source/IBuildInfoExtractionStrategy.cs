using System;
using System.Collections.Generic;
using System.Linq;


namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    public interface IBuildInfoExtractionStrategy
    {
        List<ProjectBuildInfo> GetBuildProgressInfo();

        event EventHandler BuildInfoUpdated;
    }
}
