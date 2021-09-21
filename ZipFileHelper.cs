using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

/**
 * From: https://www.cnblogs.com/Chary/p/No0000DF.html
 */

namespace ICraftLauncher
{
    public static class ZipFileHelper

    {
        #region  Methods

        /// <summary>
        ///     创建 zip 存档，该存档包含指定目录的文件和目录。
        /// </summary>
        /// <param name="sourceDirectoryName">要存档的目录的路径，指定为相对路径或绝对路径。 相对路径是指相对于当前工作目录的路径。</param>
        /// <param name="destinationArchiveFileName">要生成的存档路径，指定为相对路径或绝对路径。 相对路径是指相对于当前工作目录的路径。</param>
        /// <param name="compressionLevel"></param>
        /// <param name="includeBaseDirectory">压缩包中是否包含父目录</param>
        public static bool CreatZipFileFromDirectory(string sourceDirectoryName, string destinationArchiveFileName,
            CompressionLevel compressionLevel = CompressionLevel.NoCompression,
            bool includeBaseDirectory = true)
        {
            try
            {
                if (Directory.Exists(sourceDirectoryName)) //目录
                    if (!File.Exists(destinationArchiveFileName))
                    {
                        ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName,
                            compressionLevel, includeBaseDirectory);
                    }
                    else
                    {
                        var toZipFileDictionaryList = GetAllDirList(sourceDirectoryName, includeBaseDirectory);

                        using (
                            var archive = ZipFile.Open(destinationArchiveFileName, ZipArchiveMode.Update)
                        )
                        {
                            foreach (var toZipFileKey in toZipFileDictionaryList.Keys)
                                if (toZipFileKey != destinationArchiveFileName)
                                {
                                    var toZipedFileName = Path.GetFileName(toZipFileKey);
                                    var toDelArchives = new List<ZipArchiveEntry>();
                                    foreach (var zipArchiveEntry in archive.Entries)
                                        if (toZipedFileName != null &&
                                            (zipArchiveEntry.FullName.StartsWith(toZipedFileName) ||
                                             toZipedFileName.StartsWith(zipArchiveEntry.FullName)))
                                            toDelArchives.Add(zipArchiveEntry);
                                    foreach (var zipArchiveEntry in toDelArchives)
                                        zipArchiveEntry.Delete();
                                    archive.CreateEntryFromFile(toZipFileKey, toZipFileDictionaryList[toZipFileKey],
                                        compressionLevel);
                                }
                        }
                    }
                else if (File.Exists(sourceDirectoryName))
                    if (!File.Exists(destinationArchiveFileName))
                        ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName,
                            compressionLevel, false);
                    else
                        using (
                            var archive = ZipFile.Open(destinationArchiveFileName, ZipArchiveMode.Update)
                        )
                        {
                            if (sourceDirectoryName != destinationArchiveFileName)
                            {
                                var toZipedFileName = Path.GetFileName(sourceDirectoryName);
                                var toDelArchives = new List<ZipArchiveEntry>();
                                foreach (var zipArchiveEntry in archive.Entries)
                                    if (toZipedFileName != null &&
                                        (zipArchiveEntry.FullName.StartsWith(toZipedFileName) ||
                                         toZipedFileName.StartsWith(zipArchiveEntry.FullName)))
                                        toDelArchives.Add(zipArchiveEntry);
                                foreach (var zipArchiveEntry in toDelArchives)
                                    zipArchiveEntry.Delete();
                                archive.CreateEntryFromFile(sourceDirectoryName, toZipedFileName, compressionLevel);
                            }
                        }
                else
                    return false;
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     创建 zip 存档，该存档包含指定目录的文件和目录。
        /// </summary>
        /// <param name="sourceDirectoryName">要存档的目录的路径，指定为相对路径或绝对路径。 相对路径是指相对于当前工作目录的路径。</param>
        /// <param name="destinationArchiveFileName">要生成的存档路径，指定为相对路径或绝对路径。 相对路径是指相对于当前工作目录的路径。</param>
        /// <param name="compressionLevel"></param>
        public static bool CreatZipFileFromDictionary(Dictionary<string, string> sourceDirectoryName,
            string destinationArchiveFileName,
            CompressionLevel compressionLevel = CompressionLevel.NoCompression)
        {
            try
            {
                using (FileStream zipToOpen = new FileStream(destinationArchiveFileName, FileMode.OpenOrCreate))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        foreach (var toZipFileKey in sourceDirectoryName.Keys)
                            if (toZipFileKey != destinationArchiveFileName)
                            {
                                var toZipedFileName = Path.GetFileName(toZipFileKey);
                                var toDelArchives = new List<ZipArchiveEntry>();
                                foreach (var zipArchiveEntry in archive.Entries)
                                    if (toZipedFileName != null &&
                                        (zipArchiveEntry.FullName.StartsWith(toZipedFileName) ||
                                         toZipedFileName.StartsWith(zipArchiveEntry.FullName)))
                                        toDelArchives.Add(zipArchiveEntry);
                                foreach (var zipArchiveEntry in toDelArchives)
                                    zipArchiveEntry.Delete();
                                archive.CreateEntryFromFile(toZipFileKey, sourceDirectoryName[toZipFileKey],
                                    compressionLevel);
                            }
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     递归删除文件夹目录及文件
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
        public static bool DeleteFolder(string baseDirectory)
        {
            var successed = true;
            try
            {
                if (Directory.Exists(baseDirectory)) //如果存在这个文件夹删除之 
                {
                    foreach (var directory in Directory.GetFileSystemEntries(baseDirectory))
                        if (File.Exists(directory))
                            File.Delete(directory); //直接删除其中的文件  
                        else
                            successed = DeleteFolder(directory); //递归删除子文件夹 
                    Directory.Delete(baseDirectory); //删除已空文件夹     
                }
            }
            catch (Exception exception)
            {
                successed = false;
            }
            return successed;
        }

        /// <summary>
        /// 调用bat删除目录，以防止系统底层的异步删除机制
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static bool DeleteDirectoryWithCmd(string dirPath)
        {
            var process = new Process(); //string path = ...;//bat路径  
            var processStartInfo = new ProcessStartInfo("CMD.EXE", "/C rd /S /Q \"" + dirPath + "\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true
            }; //第二个参数为传入的参数，string类型以空格分隔各个参数  
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            if (string.IsNullOrWhiteSpace(output))
                return true;
            return false;
        }

        /// <summary>
        /// 调用bat删除文件，以防止系统底层的异步删除机制
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool DelFileWithCmd(string filePath)
        {
            var process = new Process(); //string path = ...;//bat路径  
            var processStartInfo = new ProcessStartInfo("CMD.EXE", "/C del /F /S /Q \"" + filePath + "\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true
            }; //第二个参数为传入的参数，string类型以空格分隔各个参数  
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            if (output.Contains(filePath))
                return true;
            return false;
        }

        /// <summary>
        /// 获取目录下所有[文件名，要压缩的相对文件名]字典
        /// </summary>
        /// <param name="strBaseDir"></param>
        /// <param name="includeBaseDirectory"></param>
        /// <param name="namePrefix"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetAllDirList(string strBaseDir,
            bool includeBaseDirectory = false, string namePrefix = "")
        {
            var resultDictionary = new Dictionary<string, string>();
            var directoryInfo = new DirectoryInfo(strBaseDir);
            var directories = directoryInfo.GetDirectories();
            var fileInfos = directoryInfo.GetFiles();
            if (includeBaseDirectory)
                namePrefix += directoryInfo.Name + "\\";
            foreach (var directory in directories)
                resultDictionary =
                    resultDictionary.Concat(GetAllDirList(directory.FullName, true, namePrefix))
                        .ToDictionary(k => k.Key, k => k.Value); //.FullName是某个子目录的绝对地址，
            foreach (var fileInfo in fileInfos)
                if (!resultDictionary.ContainsKey(fileInfo.FullName))
                    resultDictionary.Add(fileInfo.FullName, namePrefix + fileInfo.Name);
            return resultDictionary;
        }

        /// <summary>
        ///     Zip解压并更新目标文件
        /// </summary>
        /// <param name="zipFilePath">Zip压缩包路径</param>
        /// <param name="unZipDir">解压目标路径</param>
        /// <returns></returns>
        public static bool UnZip(string zipFilePath, string unZipDir)
        {
            bool resualt;
            try
            {
                unZipDir = unZipDir.EndsWith(@"\") ? unZipDir : unZipDir + @"\";
                var directoryInfo = new DirectoryInfo(unZipDir);
                if (!directoryInfo.Exists)
                    directoryInfo.Create();
                var fileInfo = new FileInfo(zipFilePath);
                if (!fileInfo.Exists)
                    return false;
                using (
                    var zipToOpen = new FileStream(zipFilePath, FileMode.Open, FileAccess.ReadWrite,
                        FileShare.Read))
                {
                    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                    {
                        foreach (var zipArchiveEntry in archive.Entries)
                            if (!zipArchiveEntry.FullName.EndsWith("/"))
                            {
                                var entryFilePath = Regex.Replace(zipArchiveEntry.FullName.Replace("/", @"\"),
                                    @"^\\*", "");
                                var filePath = directoryInfo + entryFilePath; //设置解压路径
                                var content = new byte[zipArchiveEntry.Length];
                                zipArchiveEntry.Open().Read(content, 0, content.Length);

                                if (File.Exists(filePath) && content.Length == new FileInfo(filePath).Length)
                                    continue; //跳过相同的文件，否则覆盖更新

                                var sameDirectoryNameFilePath = new DirectoryInfo(filePath);
                                if (sameDirectoryNameFilePath.Exists)
                                {
                                    sameDirectoryNameFilePath.Delete(true);
                                    DeleteDirectoryWithCmd(filePath);
                                    /*if (!DeleteDirectoryWithCmd(filePath))
                                    {
                                        Console.WriteLine(filePath + "删除失败");
                                        resualt = false;
                                        break;
                                    }*/
                                }
                                var sameFileNameFilePath = new FileInfo(filePath);
                                if (sameFileNameFilePath.Exists)
                                {
                                    sameFileNameFilePath.Delete();
                                    DelFileWithCmd(filePath);
                                    /*if (!DelFileWithCmd(filePath))
                                    {
                                        Console.WriteLine(filePath + "删除失败");
                                        resualt = false;
                                        break;
                                    }*/
                                }
                                var greatFolder = Directory.GetParent(filePath);
                                if (!greatFolder.Exists) greatFolder.Create();
                                File.WriteAllBytes(filePath, content);
                            }
                    }
                }
                resualt = true;
            }
            catch
                (Exception exception)
            {
                resualt = false;
            }
            return resualt;
        }

        #endregion
    }
}