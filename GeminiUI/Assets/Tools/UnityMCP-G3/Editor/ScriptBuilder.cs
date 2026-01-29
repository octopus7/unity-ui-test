using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityMCP.Editor
{
    public static class ScriptBuilder
    {
        [Serializable]
        public class CreateScriptArgs
        {
            public string scriptName;
            public string content;
            public string prefabPath; // Optional: if provided, we attach after compile
        }

        [Serializable]
        public class PendingJob
        {
            public string JobType; // "AttachComponent"
            public string ScriptName;
            public string TargetPrefabPath;
            public long Timestamp;
        }

        [Serializable]
        public class JobList
        {
            public List<PendingJob> jobs = new List<PendingJob>();
        }

        private const string JOB_FILE_PATH = "Temp/UnityMCP_Jobs.json";

        public static object CreateScript(string argsJson)
        {
            var args = JsonUtility.FromJson<CreateScriptArgs>(argsJson);

            // 1. Write File
            string directory = "Assets/Scripts/Generated";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            string filePath = Path.Combine(directory, args.scriptName + ".cs");
            File.WriteAllText(filePath, args.content);

            // 2. Queue Job if attachment is requested
            if (!string.IsNullOrEmpty(args.prefabPath))
            {
                var job = new PendingJob
                {
                    JobType = "AttachComponent",
                    ScriptName = args.scriptName,
                    TargetPrefabPath = args.prefabPath,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                
                AddJob(job);
            }

            // 3. Trigger Recompile
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.Refresh();

            return new { status = "success", message = "Script created. Compilation triggered.", pendingJob = !string.IsNullOrEmpty(args.prefabPath) };
        }

        private static void AddJob(PendingJob job)
        {
            JobList list = new JobList();
            if (File.Exists(JOB_FILE_PATH))
            {
                try
                {
                    string json = File.ReadAllText(JOB_FILE_PATH);
                    list = JsonUtility.FromJson<JobList>(json);
                }
                catch { /* Ignore corrupt file */ }
            }
            
            list.jobs.Add(job);
            File.WriteAllText(JOB_FILE_PATH, JsonUtility.ToJson(list, true));
        }

        public static JobList LoadJobs()
        {
            if (!File.Exists(JOB_FILE_PATH)) return new JobList();
            try
            {
                return JsonUtility.FromJson<JobList>(File.ReadAllText(JOB_FILE_PATH));
            }
            catch
            {
                return new JobList();
            }
        }

        public static void ClearJobs()
        {
            if (File.Exists(JOB_FILE_PATH)) File.Delete(JOB_FILE_PATH);
        }
    }
}
