using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientDesktopApp
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, UseSynchronizationContext = false, IncludeExceptionDetailInFaults = true)]
    public class Server : ServerInterface
    {
        private static ServerDataSingleton instance = new ServerDataSingleton(); 

        public Job GetFirstJob()
        {
            Job job = null;
            
            for(int i = 0; i < instance.jobs.Count; i++)
            {
                if(instance.jobs.ContainsKey(i))
                {
                    job = instance.jobs[i];
                    
                    instance.jobs.Remove(job.Id);
                    instance.takenJobs.Add(job.Id, job);
                }
            } 

            return job;
        }

        public void postJob(string jobContent)
        {
            Job job = new Job();
            job.Id = instance.currJobNumber;
            job.data = jobContent;
            
            instance.jobs.Add(instance.currJobNumber, job);
            Debug.WriteLine(instance.jobs.Count);
            instance.currJobNumber++;
        }

        public void submitJobResult(Job job)
        {
                instance.takenJobs[job.Id] = job; 
        }

        public bool hasJobs()
        {
            //return jobs.Count > 0;  
        
            if(instance.jobs.Count > 0 )
            {
                return true;
            } 
            
            return false;
        }
    }
}
