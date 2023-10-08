using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDesktopApp
{
    public class Server : ServerInterface
    {
        Dictionary<int, Job> jobs = new Dictionary<int, Job>();
        Dictionary<int, Job> takenJobs = new Dictionary<int, Job>();
        int currJobNumber = 0;

        public Job GetFirstJob()
        {
            Job job = jobs[0];
            jobs.Remove(job.Id);
            takenJobs.Add(job.Id, job);

            return job;

        }

        public void postJob(string jobContent)
        {
            Job job = new Job();
            job.Id = currJobNumber;
            job.data = jobContent;
            
            jobs.Add(currJobNumber, job);
            currJobNumber++;
        }

        public void submitJobResult(int jobId, string jobResult)
        {
            jobs[jobId].result = jobResult; 
        }

        public bool hasJobs()
        {
            return jobs.Count > 0;
        }
    }
}
