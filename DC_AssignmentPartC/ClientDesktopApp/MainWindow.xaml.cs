using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using RestSharp;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.CSharp.RuntimeBinder;

namespace ClientDesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public delegate void serverThread();
    public delegate void networkThread();

    public partial class MainWindow : Window
    {
        static string WEBSERVICEADDRESS = "http://localhost:5014";
        User thisUser;
        private List<User> userList;
        private ServerInterface foob;
        private Job currentJob;
        bool programRunning = true;
        int numberJobs = 0;
        static string newJob = null;
        static int numberJobsDone = 0;
        static bool isDoingJob = false;
        public MainWindow()
        {
            InitializeComponent();
            //Begin other threads
            JobProgressBar.Visibility = Visibility.Hidden;
        }

        public void startThreads()
        {
            serverThread sThread = this.ServerThread;
            networkThread nThread = this.NetworkThread;

            sThread.BeginInvoke(null, null);
            nThread.BeginInvoke(null, null);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            IsDoingJobBox.Text = "Doing job: " + isDoingJob;
            NumJobsBox.Text = "Number of jobs done: " + numberJobsDone;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            newJob = EnterJobBox.Text;
        }

        private void ServerThread()
        {
            string url = "net.tcp://localhost:" + thisUser.port + "/Server";
            // "net.tcp://localhost:8100/Server
            //string url = "net.tcp://" + thisUser.ipAddress + ":" + thisUser.port + "/Server";
            NetTcpBinding binding = new NetTcpBinding();
            ServiceHost host = new ServiceHost(typeof(Server));
            host.AddServiceEndpoint(typeof(ServerInterface), binding, url);
            host.Open();


            ChannelFactory<ServerInterface> foobFactory;
            NetTcpBinding tcp = new NetTcpBinding();
            foobFactory = new ChannelFactory<ServerInterface>(tcp, new EndpointAddress(url));
            ServerInterface myServer = foobFactory.CreateChannel();

            while (programRunning)
            {
                if (newJob != null)
                {
                    Job job = new Job();
                    job.Id = numberJobs;
                    job.data = newJob;
                    newJob = null;

                    myServer.postJob(job.data);
                }
            }

            host.Close();
        }

        private void NetworkThread()
        {
            while (programRunning)
            {
                getClientList();
                if(checkClientJobs())
                {
                    //do job
                    string result = doJob(); 
                    onJobComplete(currentJob.Id, result);
                }

            }
        }

        private void getClientList()
        {
            RestClient client = new RestClient(WEBSERVICEADDRESS);
            RestRequest restRequest = new RestRequest("api/Users");
            RestResponse response = client.Execute(restRequest);

            Console.WriteLine(response.Content);
            userList = JsonConvert.DeserializeObject<List<User>>(response.Content);
           // List<Users> userList = response.Content;
        }

        private bool checkClientJobs()
        {
            ChannelFactory<ServerInterface> foobFactory;
            NetTcpBinding tcp = new NetTcpBinding();
            
            for(int i = 0; (i < userList.Count); i++)
            {
                try {
                    User user = userList[i];
                    if (!(user.Id == thisUser.Id))
                    {
                        //string URL = user.ipAddress + ":" + user.port;
                        //string URL = "net.tcp://" + user.ipAddress + ":" + user.port + "/Server"; 
                        string URL = "net.tcp://localhost:" + user.port + "/Server";
                        //string URL = "net.tcp://localhost:8200/Server";
                        foobFactory = new ChannelFactory<ServerInterface>(tcp, new EndpointAddress(URL));
                        foob = foobFactory.CreateChannel();

                        if (foob.hasJobs())
                        {
                            currentJob = foob.GetFirstJob();
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }

            return false; //No job
        }

        private void onJobComplete(int id, string jobResult) {
            Job job = new Job {Id = id, result = jobResult};
            RestClient restClient = new RestClient(WEBSERVICEADDRESS);
            RestRequest restRequest = new RestRequest("/api/Jobs/jobresult", Method.Post);

            restRequest.RequestFormat = RestSharp.DataFormat.Json;
            restRequest.AddBody(job);

            RestResponse restResponse = restClient.Execute(restRequest);
            //Check response for errors later

            foob.submitJobResult(id, jobResult);
        }

        private string doJob()
        {
            JobProgressBar.Visibility = Visibility.Visible;
            isDoingJob = true;

            string code = currentJob.data;
            try {  
                ScriptEngine scriptEngine = Python.CreateEngine();
                ScriptScope scriptScope = scriptEngine.CreateScope();
                dynamic result = scriptEngine.Execute(code, scriptScope);

                JobProgressBar.Visibility = Visibility.Hidden;
                isDoingJob = false;
                numberJobsDone++;
                return result.toString(); 
            }
            catch(Exception e)
            {
                JobProgressBar.Visibility = Visibility.Hidden;
                isDoingJob = false;
                return e.Message;
            } 

        }

        private void SubmitDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            thisUser = new User();
            thisUser.Id = Int32.Parse(IdBox.Text);
            thisUser.ipAddress = IpBox.Text;
            thisUser.port = PortBox.Text;

            RestClient client = new RestClient(WEBSERVICEADDRESS);
            RestRequest restRequest = new RestRequest("api/Users/usercreate", Method.Post);
            restRequest.AddBody(thisUser);

            RestResponse response = client.Execute(restRequest);

            IdBox.Text = response.Content;
            IpBox.Text = response.StatusCode.ToString();
            if(response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                rectangleBackground.Visibility = Visibility.Hidden;
                IdBox.Visibility = Visibility.Hidden;
                IpBox.Visibility = Visibility.Hidden;
                PortBox.Visibility = Visibility.Hidden;
                SubmitDetailsButton.Visibility = Visibility.Hidden;

                startThreads();
            }

        }
    }
}
