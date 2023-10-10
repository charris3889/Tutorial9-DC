using DC_AssignmentPartC.Data;
using DC_AssignmentPartC.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Timers;

namespace DC_AssignmentPartC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbManager _context;
        public HomeController(ILogger<HomeController> logger, DbManager context)
        {
            _logger = logger;
            _context = context;

            if (_context.Users == null)
            {
                Debug.WriteLine("contextUsers is null");
            }
            else
            {
                List<User> users = _context.Users.ToList();
                ChannelFactory<ServerInterface> foobFactory;
                NetTcpBinding tcp = new NetTcpBinding();

                foreach (User user in users)
                {
                    string url = "net.tcp://" + user.ipAddress + ":" + user.port + "/Server";
                    try
                    {
                        foobFactory = new ChannelFactory<ServerInterface>(tcp, new EndpointAddress(url));
                        ServerInterface foob = foobFactory.CreateChannel();
                        foob.hasJobs();
                    }
                    catch (System.ServiceModel.EndpointNotFoundException)
                    {
                        Debug.WriteLine("Removing " + user.Id);
                        _context.Users.Remove(user);
                        _context.SaveChanges();
                    }
                }
            }
            //Debug.WriteLine("Leaving home constructor"); 
        }

        public void ClientCloser(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Debug.WriteLine("Enters client closer");
            Console.WriteLine("Enters client closer");
            if (_context.Users == null)
            {
                Debug.WriteLine("contextUsers is null");
                return;
            }
            
            List<User> users = _context.Users.ToList();
            ChannelFactory<ServerInterface> foobFactory;
            NetTcpBinding tcp = new NetTcpBinding();
           
            foreach (User user in users)
            {
                string url = "net.tcp://" + user.ipAddress + ":" + user.port + "/Server";
                try
                {
                    foobFactory = new ChannelFactory<ServerInterface>(tcp, new EndpointAddress(url));
                    ServerInterface foob = foobFactory.CreateChannel();
                    foob.hasJobs(); 
                }
                catch(System.ServiceModel.EndpointNotFoundException)
                {
                    Debug.WriteLine("Removing " + user.Id);
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                } 
                }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public ViewResult List()
        {
            Debug.WriteLine("In list");
            List<UserData> userDataList = new List<UserData>();
            
            if(_context.Users == null)
            {
                ViewBag.model = userDataList;
                return View();
            } 
            List<User> userList = _context.Users.ToList();

            if(_context.Jobs == null)
            {
                foreach(User user in userList)
                {
                    UserData userData = new UserData();
                    userData.Id = user.Id;
                    userData.numberJobs = 0;
                    userDataList.Add(userData);
                }

                ViewBag.model = userDataList;

                return View();
            }
            List<Job> jobList = _context.Jobs.ToList();

            foreach(User User in userList)
            {
                UserData userData = new UserData();
                userData.Id = User.Id;
                int numberJobsCompleted = 0; 
                foreach(Job job in jobList)
                {
                    if (job.completedBy == User.Id)
                    {
                        numberJobsCompleted++;
                    } 
                }
                userData.numberJobs = numberJobsCompleted;
                userDataList.Add(userData);
            }
            ViewBag.model = userDataList;

            return View();
        } 
    }
}