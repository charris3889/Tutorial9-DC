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
using System.Diagnostics;
using System.Net;

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
            MainWindow.newJob = EnterJobBox.Text;
        }

        private void ServerThread()
        {
            //string url = "net.tcp://localhost:" + thisUser.port + "/Server";
            // "net.tcp://localhost:8100/Server
            string url = "net.tcp://" + thisUser.ipAddress + ":" + thisUser.port + "/Server";
            NetTcpBinding binding = new NetTcpBinding();
            ServiceHost host = new ServiceHost(typeof(Server));
            host.AddServiceEndpoint(typeof(ServerInterface), binding, url);
            host.Open();
            


            ChannelFactory<ServerInterface> foobFactory;
            //NetTcpBinding tcp = new NetTcpBinding();
            foobFactory = new ChannelFactory<ServerInterface>(binding, new EndpointAddress(url));
            ServerInterface myServer = foobFactory.CreateChannel();

            while (programRunning)
            {
                if (newJob != null)
                {
                    Job job = new Job();
                    job.Id = numberJobs;
                    job.data = newJob;
                    MainWindow.newJob = null;

                    myServer.postJob(job.data);
                    Debug.WriteLine("Just posted job");
                }
                System.Threading.Thread.Sleep(5000);
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
                    doJob(); 
                }
                System.Threading.Thread.Sleep(5000);
            }
        }

        private void getClientList()
        {
            RestClient client = new RestClient(WEBSERVICEADDRESS);
            RestRequest restRequest = new RestRequest("api/Users");
            RestResponse response = client.Execute(restRequest);

            //Console.WriteLine(response.Content);
            //Debug.WriteLine("Client list" + response.Content); 
            
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
                        string URL = "net.tcp://" + user.ipAddress + ":" + user.port + "/Server"; 
                        //string URL = "net.tcp://localhost:" + user.port + "/Server";
                        //string URL = "net.tcp://localhost:8200/Server";
                        foobFactory = new ChannelFactory<ServerInterface>(tcp, new EndpointAddress(URL));
                        foob = foobFactory.CreateChannel();

                        Debug.WriteLine(foob.hasJobs());

                        if (foob.hasJobs())
                        {
                            Debug.WriteLine("Found jobs");
                            currentJob = foob.GetFirstJob();
                            if (currentJob != null)
                            {
                                return true;

                            }
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }

            return false; //No job
        }

        private void onJobComplete(Job job) {
            RestClient restClient = new RestClient(WEBSERVICEADDRESS);
            RestRequest restRequest = new RestRequest("/api/Jobs", Method.Post);

            restRequest.RequestFormat = RestSharp.DataFormat.Json;
            restRequest.AddBody(job);

            RestResponse restResponse = restClient.Execute(restRequest);

            if(restResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception();
            }
            //Check response for errors later

            foob.submitJobResult(job);
        }

        private void doJob()
        {
            Debug.WriteLine("Doing job");
            Dispatcher.BeginInvoke(new Action(() => {
                JobProgressBar.Visibility = Visibility.Visible;            }));
            
            isDoingJob = true;

            string code = currentJob.data;
            dynamic result;
            try {  
                ScriptEngine scriptEngine = Python.CreateEngine();
                ScriptScope scriptScope = scriptEngine.CreateScope();
                result = scriptEngine.Execute(code, scriptScope);

                Dispatcher.BeginInvoke(new Action(() => {
                    JobProgressBar.Visibility = Visibility.Hidden;
                }));

                MainWindow.isDoingJob = false;
                MainWindow.numberJobsDone++;
            }
            catch(Exception e)
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    JobProgressBar.Visibility = Visibility.Hidden;
                }));
                
                MainWindow.isDoingJob = false;
                result = e.Message;
            }
            string strId = thisUser.Id.ToString() + numberJobsDone.ToString();
            int id = Int32.Parse(strId);
            Job job = new Job { Id = id, data = code, result = result};

            onJobComplete(job);
            return;
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
