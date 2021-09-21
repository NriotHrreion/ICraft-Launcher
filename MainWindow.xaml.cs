using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ICraftLauncher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private StackPanel[] pages;
        private ListBox listBoxInstalledVersions;

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += this.onWindowClose;

            StackPanel[] stackPanels = {
                this.FindName("PageLauncher") as StackPanel,
                this.FindName("PageManager") as StackPanel,
                this.FindName("PageAbout") as StackPanel
            };
            this.pages = stackPanels;
            this.listBoxInstalledVersions = (ListBox) this.FindName("ListBoxInstalledVersions");
        }

        public void refreshVersionsList()
        {
            string versionsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions");
            if (!File.Exists(versionsPath))
            {
                DirectoryInfo d = new DirectoryInfo(versionsPath);
                d.Create();
            }

            DirectoryInfo directory = new DirectoryInfo(versionsPath);
            FileInfo[] files = directory.GetFiles();

            this.ListBoxInstalledVersions.Items.Clear();
            for(var i = files.Length - 1; i >= 0; i--)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = files[i].Name.Replace(".zip", "");
                this.ListBoxInstalledVersions.Items.Add(item);
            }
        }

        // Tab标签页切换
        private void tabOnClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button) sender;
            string buttonName = button.Name;

            foreach(StackPanel i in this.pages)
            {
                if (i.Name.Contains(buttonName.Replace("Tab", "")))
                {
                    i.Visibility = Visibility.Visible;
                } else
                {
                    i.Visibility = Visibility.Hidden;
                }
            }
        }

        private void installOnClick(object sender, RoutedEventArgs e)
        {
            Install window = new Install(this);
            window.Show();
        }

        private void onWindowOpen(object sender, RoutedEventArgs e)
        {
            this.refreshVersionsList();
        }

        private void onWindowClose(object sender, CancelEventArgs e)
        {
            foreach(Window window in App.Current.Windows)
            {
                if(window != this)
                {
                    window.Close();
                }
            }
        }
    }
}
