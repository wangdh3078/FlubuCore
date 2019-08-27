
//-----------------------------------------------------------------------
// <auto-generated />
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using FlubuCore.Tasks.Docker.Context;

namespace FlubuCore.Context.FluentInterface.Docker
{
    public class Context
    {  
        
            public DockerContextCreateTask ContextCreate(string context)
            {
                return new DockerContextCreateTask(context);
            }

            public DockerContextExportTask ContextExport(string context ,  string file = null)
            {
                return new DockerContextExportTask(context,  file);
            }

            public DockerContextInspectTask ContextInspect(params string[] context)
            {
                return new DockerContextInspectTask(context);
            }

            public DockerContextLsTask ContextLs()
            {
                return new DockerContextLsTask();
            }

            public DockerContextRmTask ContextRm(params string[] context)
            {
                return new DockerContextRmTask(context);
            }

            public DockerContextUpdateTask ContextUpdate(string context)
            {
                return new DockerContextUpdateTask(context);
            }
        
    }
}
