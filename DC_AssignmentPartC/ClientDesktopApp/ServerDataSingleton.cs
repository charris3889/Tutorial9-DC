using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDesktopApp
{
    public class ServerDataSingleton
    {
        public Dictionary<int, Job> jobs;// = new Dictionary<int, Job>();
        public Dictionary<int, Job> takenJobs;// = new Dictionary<int, Job>();
        public int currJobNumber;// = 0;

        public ServerDataSingleton() { 
            if(jobs == null)
            {
                jobs = new Dictionary<int, Job>();
                takenJobs = new Dictionary<int, Job>();
                currJobNumber = 0;
            } 
        }
    }
}
