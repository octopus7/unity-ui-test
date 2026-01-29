using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace UnityMCP.Editor
{
    [InitializeOnLoad]
    public class JobProcessor
    {
        static JobProcessor()
        {
            // This constructor is called after every Domain Reload (Compilation)
            EditorApplication.delayCall += ProcessQueue;
        }

        private static void ProcessQueue()
        {
            var jobList = ScriptBuilder.LoadJobs();
            if (jobList.jobs.Count == 0) return;

            Debug.Log($"[UnityMCP] Processing {jobList.jobs.Count} pending jobs...");

            bool anyChanges = false;
            List<ScriptBuilder.PendingJob> failedJobs = new List<ScriptBuilder.PendingJob>();

            foreach (var job in jobList.jobs)
            {
                try
                {
                    if (job.JobType == "AttachComponent")
                    {
                        if (!AttachComponent(job.ScriptName, job.TargetPrefabPath))
                        {
                            // If failed (maybe type not found yet?), keep it? 
                            // Or discard to avoid infinite loops?
                            // For now, let's discard but log error.
                            Debug.LogError($"[UnityMCP] Failed to attach {job.ScriptName} to {job.TargetPrefabPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UnityMCP] Job Execution Error: {ex}");
                }
            }

            // Always clear for now to prevent loops
            ScriptBuilder.ClearJobs();
        }

        private static bool AttachComponent(string scriptName, string prefabPath)
        {
            // Try to find the type
            // Note: Since we are in Editor, we might need to search assemblies if "Assembly-CSharp" isn't default context?
            // Usually "Assembly-CSharp" holds the user scripts.
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type targetType = null;
            
            foreach (var asm in assemblies)
            {
                targetType = asm.GetType(scriptName);
                if (targetType != null) break;
            }

            if (targetType == null)
            {
                Debug.LogError($"[UnityMCP] Type '{scriptName}' not found in any loaded assembly.");
                return false;
            }

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabContents == null)
            {
                Debug.LogError($"[UnityMCP] Prefab not found at {prefabPath}");
                return false;
            }

            if (prefabContents.GetComponent(targetType) == null)
            {
                prefabContents.AddComponent(targetType);
                Debug.Log($"[UnityMCP] Attached {scriptName} to {prefabPath}");
                
                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            }
            else
            {
                 Debug.Log($"[UnityMCP] {scriptName} already exists on {prefabPath}");
            }
            
            PrefabUtility.UnloadPrefabContents(prefabContents);
            return true;
        }
    }
}
