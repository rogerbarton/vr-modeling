using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RogerBarton
{
    public static class BuildPipelineCustomization
    {
        /// <returns>e.g. win64/myApp.exe</returns>
        public static string GetBuildName(this BuildPipeline pipeline, BuildConfig config)
        {
            return config.name + "/" + pipeline.appName + "-" + Application.version + config.fileExt;
        }
        
        /// <returns>Name of the folder for the current build iteration of the whole pipeline</returns>
        public static string GetBuildIterationName(this BuildPipeline pipeline)
        {
            DateTime currentDate = DateTime.Now;
            return Application.productName + " (" + currentDate.Day + "-" + currentDate.Month +
                   "-" + currentDate.Year.ToString().Substring(currentDate.Year.ToString().Length - 2) + ')';
        }
        
        #region Callbacks
        public static void InitCallbacks()
        {
            BuildPipeline.OnPreBuild += RunCMakeRelease;
        }
        
        private static void RunCMakeRelease(BuildConfig config, string path, StreamWriter log)
        {
            return; // Skip this experimental feature
            
            #if !UNITY_EDITOR_WIN
            log.WriteLine("Automatic CMake release building is not yet implemented. Please make sure you build a release" +
                          "version of the library manually.");
            return;
            #endif
            
            log.WriteLine("Pre-Build: " + config.name);
            var cppProjectRoot =
                Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"), "/Assets".Length)
                + "/Interface/";
            
            var cmakeProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    #if UNITY_EDITOR_WIN
                    FileName = "cmd.exe",
                    Arguments = "/C cmake -S . -B cmake-build-release & cmake --build cmake-build-release --target __libigl-interface --config Release",
                    #else
                    FileName = "cmake -S . -B cmake-build-release & cmake --build cmake-build-release --target __libigl-interface --config Release",
                    Arguments = ""
                    #endif
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false,
                    WorkingDirectory = cppProjectRoot
                }
            };
            
            cmakeProcess.Start();
            log.WriteLine(cmakeProcess.StartInfo.FileName  + " " + cmakeProcess.StartInfo.Arguments);
            cmakeProcess.WaitForExit();
            log.WriteLine("Cmake finished with exit code: " + cmakeProcess.ExitCode);
        }

        #endregion
    }
}