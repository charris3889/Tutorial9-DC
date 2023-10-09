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
        void submitJobResult(Job job);
        
        [OperationContract]
        Job GetFirstJob();
        
        [OperationContract]
        bool hasJobs();
    }
}
