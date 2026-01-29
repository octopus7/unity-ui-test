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
            EditorApplication.delayCall += ProcessQueue;
        }

        private static void ProcessQueue()
        {
            var jobList = ScriptBuilder.LoadJobs();
            if (jobList.jobs.Count == 0) return;

            Debug.Log($"[UnityMCP] Processing {jobList.jobs.Count} pending jobs...");

            foreach (var job in jobList.jobs)
            {
                try
                {
                    if (job.JobType == "AttachComponent")
                    {
                        if (!AttachComponent(job.ScriptName, job.TargetPrefabPath))
                        {
                            Debug.LogError($"[UnityMCP] Failed to attach {job.ScriptName} to {job.TargetPrefabPath}");
                        }
                    }
                    else if (job.JobType == "BindComponent")
                    {
                        var args = new BinderOps.BindComponentArgs
                        {
                            scriptName = job.ScriptName,
                            prefabPath = job.TargetPrefabPath,
                            uiElementName = job.UiElementName,
                            fieldName = job.FieldName,
                            targetElementName = job.BindTargetName
                        };

                        string error;
                        if (!BinderOps.TryBind(args, out error))
                        {
                             Debug.LogError($"[UnityMCP] Failed to bind {job.FieldName}: {error}");
                        }
                        else
                        {
                             Debug.Log($"[UnityMCP] Successfully bound {job.FieldName} on {job.ScriptName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UnityMCP] Job Execution Error: {ex}");
                }
            }

            ScriptBuilder.ClearJobs();
        }

        private static bool AttachComponent(string scriptName, string prefabPath)
        {
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
            if (prefabContents == null) return false;

            if (prefabContents.GetComponent(targetType) == null)
            {
                prefabContents.AddComponent(targetType);
                Debug.Log($"[UnityMCP] Attached {scriptName} to {prefabPath}");
                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            }
            
            PrefabUtility.UnloadPrefabContents(prefabContents);
            return true;
        }
    }
}
