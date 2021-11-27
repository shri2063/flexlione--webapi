using System;
using System.Collections.Generic;
using System.IO;
using m_sort_server.Interfaces;
using Newtonsoft.Json;

namespace m_sort_server.Services
{
    public class LogFileService:ILogFileService
    {
        private static string DirectoryPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())); 
        private static string logFilePath = DirectoryPath + "\\M-Sort\\m-sort-server-dev\\Communication_Server\\"  ;

         public  dynamic ReadFromJsonFile<T>(string name)
        {
            String filename = logFilePath + name  + "." + "json";
            FileInfo logFileInfo = new FileInfo(filename);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            
            if (!logFileInfo.Exists)
            {
                logDirInfo.Create();
            }

            try
            {
                using (StreamReader r = new StreamReader(filename))
                {
                    string json = r.ReadToEnd() + "]";
                    r.Close();
                    return JsonConvert.DeserializeObject<List<T>>(json);
                   
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString()); 
            }
        }

        

         public  void WriteIntoJsonFile<T>(T data, string name)
        {
            String filename = logFilePath + name  + "." + "json";
            FileInfo logFileInfo = new FileInfo(filename);
            DirectoryInfo  logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            FileStream fileStream;
            string json = JsonConvert.SerializeObject(data);
            if (!logFileInfo.Exists)
            {
               fileStream = logFileInfo.Create();
               StreamWriter objStreamWriter = new StreamWriter((Stream)fileStream);
               objStreamWriter.WriteLine("["+json);  
               objStreamWriter.Close();  
               fileStream.Close(); 
            }
            else
            {
                fileStream = new FileStream(filename, FileMode.Append);
                StreamWriter objStreamWriter = new StreamWriter((Stream)fileStream);
                objStreamWriter.WriteLine(","+json);  
                objStreamWriter.Close();  
                fileStream.Close(); 
            }
        }

         public void DeleteJsonFile(string name)
         {
             String filename = logFilePath + name  + "." + "json";
             File.Delete(filename);
         }
    }
}