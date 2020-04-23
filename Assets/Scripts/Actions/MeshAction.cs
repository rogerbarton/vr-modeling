using System;
using UnityEngine;

namespace libigl
{
    /// <summary>
    /// Implements functions required for a MeshAction.
    /// </summary>
    public interface IMeshAction
    {
        /// <summary>
        /// See <see cref="MeshAction.ExecuteCondition"/>
        /// </summary>
        bool ExecuteCondition();
        /// <summary>
        /// See <see cref="MeshAction.PreExecute"/>
        /// </summary>
        void PreExecute(MeshData libiglMesh);
        /// <summary>
        /// See <see cref="MeshAction.Execute"/>
        /// </summary>
        void Execute(MeshData data);
        /// <summary>
        /// See <see cref="MeshAction.PostExecute"/>
        /// </summary>
        void PostExecute(Mesh mesh, MeshData data);
    }

    /// <summary>
    /// Stores information on one action that can be performed on a mesh.
    /// This will handle execution on a separate thread.<br/>
    /// Create an instance with lambdas, a IMeshAction class or derive from this class and set the values in the constructor.
    /// </summary>
    public class MeshAction
    {
        public readonly string Name;
        public readonly string[] SpeechKeywords;
        public readonly int GestureId;
        public const int InvalidGesture = -1;
        public readonly bool AllowQueueing;
        
        /// <summary>
        /// Add any additional condition, such as a shortcut, to trigger execution.
        /// This is in addition to the <see cref="SpeechKeywords"/> and <see cref="GestureId"/>.
        /// </summary>
        public readonly Func<bool> ExecuteCondition;

        /// <summary>
        /// Called on the main thread before <see cref="Execute"/> to gather required data for it.
        /// This may include reading the current input state.
        /// May be null.
        /// </summary>
        public readonly Action<MeshData> PreExecute;
        
        /// <summary>
        /// Code to execute when the action is triggered. Will be on a worker thread.
        /// Expensive operations should be done here.
        /// </summary>
        public readonly Action<MeshData> Execute;
    
        /// <summary>
        /// Called on the main thread after <see cref="Execute"/> to apply changes.
        /// Applies the modified mesh by the job.
        /// </summary>
        public readonly Action<Mesh, MeshData> PostExecute;
    
        public MeshAction(string name,  string[] speechKeywords, int gestureId, Func<bool> executeCondition, 
            Action<MeshData> execute, Action<Mesh, MeshData> postExecute, 
            Action<MeshData> preExecute = default, 
            bool allowQueueing = true)
        {
            Name = name;
            SpeechKeywords = speechKeywords;
            GestureId = gestureId;
            AllowQueueing = allowQueueing;
            
            ExecuteCondition = executeCondition;
            PreExecute = preExecute;
            Execute = execute;
            PostExecute = postExecute;
        }
        
        public MeshAction(string name, string[] speechKeywords, int gestureId, 
            IMeshAction meshAction,  
            bool allowQueueing = true)
        {
            Name = name;
            SpeechKeywords = speechKeywords;
            GestureId = gestureId;
            AllowQueueing = allowQueueing;
            
            ExecuteCondition = meshAction.ExecuteCondition;
            PreExecute = meshAction.PreExecute;
            Execute = meshAction.Execute;
            PostExecute = meshAction.PostExecute;
        }
        
        /// <summary>
        /// For creating a MeshAction dynamically without any UI generation
        /// </summary>
        public MeshAction(string name, 
            Action<MeshData> execute, Action<Mesh, MeshData> postExecute, Action<MeshData> preExecute = default, 
            bool allowQueueing = true)
        {
            Name = name;
            SpeechKeywords = new string[0];
            GestureId = InvalidGesture;
            AllowQueueing = allowQueueing;
            
            ExecuteCondition = default;
            PreExecute = preExecute;
            Execute = execute;
            PostExecute = postExecute;
        }

        /// <summary>
        /// Schedules the action on the active mesh
        /// </summary>
        public MeshAction Schedule()
        {
            MeshManager.activeMesh.ScheduleAction(this);
            return this;
        }
    }
}
