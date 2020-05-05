using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Windows.Speech;

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
        void PreExecute(LibiglMesh libiglMesh);
        /// <summary>
        /// See <see cref="MeshAction.Execute"/>
        /// </summary>
        void Execute(MeshData data);
        /// <summary>
        /// See <see cref="MeshAction.PostExecute"/>
        /// </summary>
        void PostExecute(LibiglMesh libiglMesh);
    }
    
    /// <summary>
    /// When an action should be executed.
    /// </summary>
    public enum MeshActionType
    {
        /// <summary>
        /// Executed when a new mesh is loaded, use this to initialize a mesh, do pre-calculations.
        /// These actions have no UI.
        /// </summary>
        OnInitialize,
        /// <summary>
        /// Executed every frame, when there is nothing running currently.
        /// Executed after all <see cref="OnInitialize"/>.
        /// </summary>
        OnUpdate
    };
    
    /// <summary>
    /// Stores information on one action that can be performed on a mesh.
    /// This will handle execution on a separate thread.<br/>
    /// Create an instance with lambdas, a <see cref="IMeshAction"/> class or derive from this class and set the values in the constructor.<br/>
    /// Actions always operate on a <see cref="LibiglMesh"/> instance.
    /// </summary>
    public class MeshAction : IDisposable
    {
        public readonly MeshActionType Type;
        
        public readonly string Name;
        public string[] SpeechKeywords;
        public readonly int GestureId;
        public const int InvalidGesture = -1;
        private KeywordRecognizer _keywordRecognizer;

        /// <summary>
        /// Add any additional condition, such as a shortcut, to trigger execution.
        /// This is in addition to the UI, <see cref="SpeechKeywords"/> and <see cref="GestureId"/>.
        /// <returns>True if the action should be executed</returns>
        /// </summary>
        public readonly Func<bool> ExecuteCondition;

        /// <summary>
        /// Called on the main thread before <see cref="Execute"/> to gather required data for it.
        /// This may include reading the current input state.
        /// May be null.<br/>
        /// Use <see cref="LibiglMesh.Data"/> or <see cref="LibiglMesh.DataRowMajor"/> to get the mesh data.
        /// In most cases MeshData will not be used. Expensive operations should be done in Execute.
        /// <typeparam name="LibiglMesh">The LibiglMesh Monobehaviour. </typeparam>
        /// </summary>
        public readonly Action<LibiglMesh> PreExecute;
        
        /// <summary>
        /// Code to execute when the action is triggered. Will be on a worker thread.
        /// Expensive operations should be done here.
        /// MeshData is given in ColumnMajor.<br/>
        /// You need to set the <see cref="MeshData.DirtyState"/> in order for changes to be applied.<br/>
        /// Behind the scenes this will transfer the changes to the RowMajor <see cref="MeshData"/> copy which is required by Unity.
        /// <typeparam name="MeshData">ColumnMajor mesh data to be modified</typeparam> 
        /// </summary>
        public readonly Action<MeshData> Execute;

        /// <summary>
        /// Called on the main thread after <see cref="Execute"/> just before dirty changes are applied to the Unity mesh.<br/>
        /// Note: Unity Mesh functions, such as mesh.SetVertices(), require a RowMajor format.<br/>
        /// Expensive operations should be done in Execute.
        /// <typeparam name="LibiglMesh.Mesh">The Unity mesh to apply changes to.</typeparam>
        /// <typeparam name="LibiglMesh.DataRowMajor">RowMajor mesh data that should be used to apply custom changes.</typeparam>
        /// </summary>
        public readonly Action<LibiglMesh> PostExecute;

        public MeshAction(MeshActionType type, string name,
            [NotNull] Action<MeshData> execute, [NotNull] Func<bool> executeCondition, Action<LibiglMesh> preExecute = null,
            Action<LibiglMesh> postExecute = null, string[] speechKeywords = null, int gestureId = InvalidGesture)
        {
            Type = type;
            Name = name;
            SpeechKeywords = speechKeywords;
            GestureId = gestureId;

            ExecuteCondition = executeCondition;
            PreExecute = preExecute;
            Execute = execute;
            PostExecute = postExecute;
        }

        public MeshAction(MeshActionType type, string name, [NotNull] IMeshAction meshAction,
            string[] speechKeywords = null, int gestureId = InvalidGesture)
        {
            Type = type;
            Name = name;
            SpeechKeywords = speechKeywords;
            GestureId = gestureId;

            ExecuteCondition = meshAction.ExecuteCondition;
            PreExecute = meshAction.PreExecute;
            Execute = meshAction.Execute;
            PostExecute = meshAction.PostExecute;
        }

        /// <summary>
        /// Schedules the action on the active mesh
        /// </summary>
        public MeshAction Schedule()
        {
            MeshManager.ActiveMesh.ScheduleAction(this);
            return this;
        }

        public void InitializeSpeech()
        {
            if (SpeechKeywords != null)
            {
                // Create one speech keywords recognizer for all actions 
                Speech.CheckSpeechKeywords(ref SpeechKeywords);
                if(SpeechKeywords.Length > 0)
                {
                    _keywordRecognizer = new KeywordRecognizer(SpeechKeywords);
                    _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
                    _keywordRecognizer.Start();
                }
            } 
        }
        
        /// <summary>
        /// Invoked by the KeywordRecognizer, this will schedule the according action.
        /// </summary>
        /// <param name="args"></param>
        private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            Schedule();
        }

        public void Dispose()
        {
            _keywordRecognizer?.Dispose();
        }
    }
}
