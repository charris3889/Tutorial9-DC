using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace ClientDesktopApp
{
    [ServiceContract]
    public interface ServerInterface 
    {
        [OperationContract]
        void postJob(string jobContent);
        [OperationContract]
        void submitJobResult(int jobId, string jobResult);
        [OperationContract]
        Job GetFirstJob();
        [OperationContract]
        bool hasJobs();
    }
}
