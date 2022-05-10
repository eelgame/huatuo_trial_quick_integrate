using System;
using System.Collections.Generic;
using System.IO;
using NiceIO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HuaTuo
{
    public class HuaTuo_BuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            try
            {
                Debug.Log("IL2CPP: " + IsIL2CPPEnabled());
                if (IsIL2CPPEnabled())
                {
                    var il2cppFolder = Path.Combine(EditorApplication.applicationContentsPath, "il2cpp");
                    var huatuoIl2cppFolder = Path.Combine(Environment.CurrentDirectory, "UnityData", "il2cpp");

                    if (!Directory.Exists(il2cppFolder))
                    {
                        EditorUtility.DisplayDialog("Error", "请安装il2cpp.", "我知道了");
                        throw new BuildFailedException("尚未安装il2cpp.");
                    }

                    foreach (var file in Directory.GetFiles(il2cppFolder))
                        File.Copy(file, Path.Combine(huatuoIl2cppFolder, Path.GetFileName(file)), true);

                    foreach (var directory in Directory.GetDirectories(il2cppFolder))
                    {
                        var directoryName = Path.GetFileName(directory);
                        if (directoryName != "libil2cpp")
                        {
                            var nPath = new NPath(Path.Combine(huatuoIl2cppFolder, directoryName));
                            if (!nPath.Exists()) nPath.CreateSymbolicLink(new NPath(directory), false);
                        }
                    }

                    var symbolicLinkMapping = new Dictionary<string, string>
                    {
                        {
                            Path.Combine(Environment.CurrentDirectory, "UnityData", "MonoBleedingEdge"),
                            Path.Combine(EditorApplication.applicationContentsPath, "MonoBleedingEdge")
                        },
                        {
                            Path.Combine(Environment.CurrentDirectory, "UnityData", "il2cpp", "libil2cpp", "huatuo"),
                            Path.Combine(Environment.CurrentDirectory, "UnityData", "huatuo", "huatuo")
                        }
                    };

                    foreach (var kv in symbolicLinkMapping)
                    {
                        var nPath = new NPath(kv.Key);
                        if (!nPath.Exists()) nPath.CreateSymbolicLink(new NPath(kv.Value), false);
                    }


                    Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", huatuoIl2cppFolder);
                    // Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", "");
                }
            }
            catch (Exception e)
            {
                throw new BuildFailedException(e);
            }
        }

        public static bool IsIL2CPPEnabled()
        {
            return PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup) ==
                   ScriptingImplementation.IL2CPP;
        }
    }
}