using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        private ConfigJson config;
        private Launcher launcher;

        private StackPanel[] pages;
        private Button buttonLaunch;
        private Button buttonSetToLaunch;
        private Button buttonDelete;
        private ListBox listBoxInstalledVersions;

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += this.onWindowClose;

            this.config = new ConfigJson();
            this.launcher = new Launcher(this);

            StackPanel[] stackPanels = {
                this.FindName("PageLauncher") as StackPanel,
                this.FindName("PageManager") as StackPanel,
                this.FindName("PageAbout") as StackPanel
            };
            this.pages = stackPanels;

            this.buttonLaunch = (Button) this.FindName("ButtonLaunch");
            this.buttonSetToLaunch = (Button) this.FindName("ButtonSetToLaunch");
            this.buttonDelete = (Button) this.FindName("ButtonDelete");
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

        // For Debug
        private void dbOnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(this.launcher.p.StandardOutput.ReadToEnd());
        }

        private void launchOnClick(object sender, RoutedEventArgs e)
        {
            if(this.config.getLaunchVersion() == "")
            {
                MessageBox.Show("请先在\"管理\"中设置启动项");
                return;
            }

            if(!this.launcher.didGameLaunch)
            {
                this.launcher.launchGame(this.config.getLaunchVersion());
                this.buttonLaunch.Content = "停止游戏";
            } else
            {
                this.launcher.stopGame();
                this.buttonLaunch.Content = "启动游戏";
            }
        }

        private void installOnClick(object sender, RoutedEventArgs e)
        {
            Install window = new Install(this);
            window.Show();
        }

        [DllImport("Installer.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void start_install();

        private void npmInstallOnClick(object sender, RoutedEventArgs e)
        {
            if(!Directory.Exists(Path.Combine(this.launcher.gameRunningPath, "node_modules")))
            {
                if(this.config.getLaunchVersion() == "")
                {
                    MessageBox.Show("请先设置启动项");
                    return;
                }

                this.launcher.unzipGameFiles(this.config.getLaunchVersion(), false);

                // npm install
                try
                {
                    start_install();
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            } else
            {
                MessageBox.Show("你已安装依赖, 无需重复安装");
            }
        }

        private void setToLaunchOnClick(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedItem = (ListBoxItem) this.listBoxInstalledVersions.SelectedItem;
            this.config.setLaunchVersion(selectedItem.Content.ToString());
            this.buttonSetToLaunch.IsEnabled = false;
            this.buttonDelete.IsEnabled = false;
        }

        private void deleteOnClick(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedItem = (ListBoxItem)this.listBoxInstalledVersions.SelectedItem;
            FileInfo file = new FileInfo(Path.Combine(this.launcher.versionsPath, selectedItem.Content + ".zip"));
            MessageBoxResult result = MessageBox.Show("你确定要删除这个版本吗?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                file.Delete();
                this.config.setLaunchVersion("");
                this.refreshVersionsList();
            }
        }

        private void installedVersionsOnSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem selectedItem = (ListBoxItem) this.listBoxInstalledVersions.SelectedItem;
            
            if(selectedItem != null)
            {
                this.buttonDelete.IsEnabled = true;
            } else
            {
                this.buttonDelete.IsEnabled = false;
                return;
            }

            if(selectedItem.Content.ToString() == this.config.getLaunchVersion())
            {
                this.buttonSetToLaunch.IsEnabled = false;
                this.buttonDelete.IsEnabled = false;
            } else
            {
                this.buttonSetToLaunch.IsEnabled = true;
                this.buttonDelete.IsEnabled = true;
            }
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
