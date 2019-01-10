using System;
using System.Collections.Generic;
using System.Linq;


namespace VSBuildTimer
{
    public interface IBuildInfoExtractionStrategy
    {
        List<ProjectBuildInfo> GetBuildProgressInfo();

        event EventHandler BuildInfoUpdated;

        System.DateTime LastUpdateTime();
    }
}
