using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ICraftLauncher
{
    class Launcher
    {
        public bool didGameLaunch = false;

        public string versionsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions");
        public string gameUnzipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unzip");
        public string gameRunningPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game");
        
        private MainWindow mainWindow;

        public Process p;

        public Launcher(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            if(!File.Exists(this.gameUnzipPath))
            {
                DirectoryInfo d = new DirectoryInfo(this.gameUnzipPath);
                d.Create();
            }
        }

        private void initCmdProcess()
        {
            this.p = new Process();

            this.p.StartInfo.FileName = "cmd.exe";
            this.p.StartInfo.UseShellExecute = false;
            this.p.StartInfo.RedirectStandardInput = true;
            this.p.StartInfo.RedirectStandardOutput = true;
            this.p.StartInfo.RedirectStandardError = true;
            this.p.StartInfo.CreateNoWindow = true;
            this.p.Start();
        }

        public void stopGame()
        {
            this.mainWindow.Title = "ICraft 启动器";
            this.p.Close();
            this.p.Dispose();
            this.didGameLaunch = false;
        }

        public void launchGame(string version)
        {
            this.mainWindow.Title = "ICraft 启动器 (正在准备)";
            this.initCmdProcess();

            this.unzipGameFiles(version, true);

            // Install modules & Launch game
            this.mainWindow.Title = "ICraft 启动器 (正在启动游戏 这个过程可能需要一些时间)";
            this.runCommand("cd "+ this.gameRunningPath);
            this.runCommand("npm run ic-launch");
            this.didGameLaunch = true;
        }

        public void unzipGameFiles(string version, bool needNM)
        {
            string zipPath = Path.Combine(this.versionsPath, version + ".zip");
            ZipFileHelper.UnZip(zipPath, this.gameUnzipPath);

            // Move game file to the base dir and rename to "game" (Save ".icraft" dir in "game" dir)
            DirectoryInfo gameDir = new DirectoryInfo(this.gameUnzipPath).GetDirectories()[0];
            if (Directory.Exists(Path.Combine(this.gameRunningPath, ".icraft")))
            {
                Directory.Move(Path.Combine(this.gameRunningPath, ".icraft"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".icraft"));
            }
            if (needNM && Directory.Exists(Path.Combine(this.gameRunningPath, "node_modules")))
            {
                Directory.Move(Path.Combine(this.gameRunningPath, "node_modules"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "node_modules"));
            }
            this.clearPath(this.gameRunningPath);
            Directory.Move(gameDir.FullName, this.gameRunningPath);
            if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".icraft")))
            {
                Directory.Move(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".icraft"), Path.Combine(this.gameRunningPath, ".icraft"));
            }
            if (needNM && Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "node_modules")))
            {
                Directory.Move(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "node_modules"), Path.Combine(this.gameRunningPath, "node_modules"));
            }
        }

        /**
         * From: https://www.cnblogs.com/chxl800/p/10498399.html
         * 
         * Edited by me
         */
        public void clearPath(string file)
        {
            if(!Directory.Exists(file))
            {
                return;
            }

            try
            {
                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                File.SetAttributes(file, FileAttributes.Normal);

                //判断文件夹是否还存在
                if (Directory.Exists(file))
                {
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (File.Exists(f))
                        {
                            //如果有子文件删除文件
                            File.Delete(f);
                        }
                        else
                        {
                            //循环递归删除子文件夹
                            this.clearPath(f);
                        }
                    }

                    //删除空文件夹
                    Directory.Delete(file);
                }
            }
            catch (Exception ex) // 异常处理
            {
                Console.WriteLine(ex.Message.ToString()); // 异常信息
            }
        }

        private void runCommand(string command)
        {
            try
            {
                this.p.StandardInput.WriteLine(command);
                this.p.StandardInput.AutoFlush = true;
            } catch (Exception ex)
            {
                MessageBox.Show("启动错误");
            }
        }
    }
}
