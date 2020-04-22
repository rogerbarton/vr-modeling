using System;
using UnityEngine;

namespace libigl
{
    public interface IExecuteApply
    {
        void Execute(MeshData data);
        void Apply(Mesh mesh, MeshData data);
    }
    
    public class MeshAction
    {
        public readonly string Name;
        public readonly string[] SpeechKeywords;
        public readonly int GestureId;
        public readonly Func<bool> ExecuteTest;

        /// <summary>
        /// Code to execute when the action is triggered. Will be on a worker thread
        /// Expensive operations should be done here
        /// </summary>
        public readonly Action<MeshData> Execute;
    
        /// <summary>
        /// Applies the modified mesh by the job
        /// Will be called on the main thread
        /// </summary>
        public readonly Action<Mesh, MeshData> Apply;
    
        public MeshAction(string name,  string[] speechKeywords, int gestureId, Func<bool> executeTest, 
            Action<MeshData> execute, Action<Mesh, MeshData> apply)
        {
            Name = name;
            SpeechKeywords = speechKeywords;
            GestureId = gestureId;
            Execute = execute;
            Apply = apply;
            ExecuteTest = executeTest;
        }
        
        public MeshAction(string name, string[] speechKeywords, int gestureId, Func<bool> executeTest, 
            IExecuteApply executeApply)
        {
            Name = name;
            SpeechKeywords = speechKeywords;
            GestureId = gestureId;
            ExecuteTest = executeTest;
            Execute = executeApply.Execute;
            Apply = executeApply.Apply;
        }

        public void Schedule()
        {
            MeshManager.activeMesh.ScheduleAction(this);
        }
    }
}
