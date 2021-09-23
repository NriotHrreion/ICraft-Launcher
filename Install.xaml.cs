using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace ICraftLauncher
{

    /// <summary>
    /// Install.xaml 的交互逻辑
    /// </summary>
    public partial class Install : Window
    {
        private const string versionListUrl = "https://api.github.com/repos/NriotHrreion/ICraft-App/releases";
        
        private ListBox listElem;
        private Button buttonInstall;

        private MainWindow mainWindow;

        public Install(MainWindow mainWindow)
        {
            InitializeComponent();

            this.listElem = (ListBox) this.FindName("ListBoxVersions");
            this.buttonInstall = (Button) this.FindName("ButtonInstall");
            this.mainWindow = mainWindow;
        }

        private void onWindowOpen(object sender, RoutedEventArgs e)
        {
            this.Title = "ICraft 安装 (正在获取版本列表)";
            this.fetchVersionsList();
        }

        private void refreshOnClick(object sender, RoutedEventArgs e)
        {
            this.listElem.Items.Clear();
            this.Title = "ICraft 安装 (正在刷新)";
            this.fetchVersionsList();
        }

        private void installOnClick(object sender, RoutedEventArgs e)
        {
            this.buttonInstall.IsEnabled = false;
            this.Title = "ICraft 安装 (正在下载)";
            ListBoxItem itemToInstall = (ListBoxItem) this.listElem.SelectedItem;
            
            // Get the path to install (server path & local target path)
            string fileUrlPath = (string) itemToInstall.DataContext;
            string serverFileName = string.Empty;
            int nameIndex = fileUrlPath.LastIndexOf("/");
            if(nameIndex > 0)
            {
                serverFileName = fileUrlPath.Substring(nameIndex + 1);
            }
            string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions");
            if(!Directory.Exists(dirPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
                directoryInfo.Create();
            }
            string targetPath = Path.Combine(dirPath, serverFileName +".zip");

            // Fetch file and download
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(fileUrlPath);
            request.Method = "GET";
            request.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 74.0) Gecko / 20100101 Firefox / 74.0";

            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            using (Stream fileStream = new FileStream(targetPath, FileMode.Create))
            {
                byte[] read = new byte[1024];
                int realReadLen = responseStream.Read(read, 0, read.Length);
                while (realReadLen > 0)
                {
                    fileStream.Write(read, 0, realReadLen);
                    realReadLen = responseStream.Read(read, 0, read.Length);
                }

                responseStream.Close();
                fileStream.Close();
            }

            this.Title = "ICraft 安装";
            MessageBox.Show(itemToInstall.Content +" 安装成功");

            this.mainWindow.refreshVersionsList();
        }

        private void versionsOnSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if(!this.listElem.Items.IsEmpty)
            {
                this.buttonInstall.IsEnabled = true;
            } else
            {
                this.buttonInstall.IsEnabled = false;
            }
        }

        private void fetchVersionsList()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(versionListUrl);
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                request.ContentType = "application/json";

                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                JArray list = (JArray) JsonConvert.DeserializeObject(streamReader.ReadToEnd());
                streamReader.Close();
                responseStream.Close();

                foreach (var i in list as JArray)
                {
                    ListBoxItem itemElem = new ListBoxItem();
                    itemElem.Content = i.Value<string>("target_commitish") +" "+ i.Value<string>("tag_name") +" - "+ i.Value<string>("published_at");
                    itemElem.DataContext = i.Value<string>("zipball_url");
                    this.listElem.Items.Add(itemElem);
                }

                this.Title = "ICraft 安装";
            } catch (Exception e)
            {
                MessageBox.Show("版本列表获取失败 "+ e.ToString());
            }
        }
    }
}
