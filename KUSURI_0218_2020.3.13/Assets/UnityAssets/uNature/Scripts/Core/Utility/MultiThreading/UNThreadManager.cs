using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Threading;

using uNature.Core.Settings;
using uNature.Core.FoliageClasses;

namespace uNature.Core.Threading
{
    /// <summary>
    /// This class handles the multi-threading mechanics.
    /// </summary>
    [ExecuteInEditMode]
    public class UNThreadManager : MonoBehaviour
    {
        /// <summary>
        /// our thread instance;
        /// </summary>
        internal static UNThreadManager _instance;
        public static UNThreadManager instance
        {
            get
            {
                if (_instance == null)
                {
                    if (inUnityThread)
                    {
                        _instance = FindObjectOfType<UNThreadManager>();

                        if (_instance == null)
                        {
                            var go = new GameObject("Thread Manager");
                            _instance = go.AddComponent<UNThreadManager>();

                            Debug.Log("Thread Manager Created!");
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Is the multi-thread option enabled?
        /// </summary>
        public bool threadEnabled
        {
            get { return UNSettings.instance.UN_Threading_Enabled; }
        }

        /// <summary>
        /// Thread workers count
        /// </summary>
        public int threadWorkersCount
        {
            get
            {
                return (int)UNSettings.instance.UN_Threading_WorkersCount;
            }
        }

        /// <summary>
        /// How often will the thread manager update the thread items.
        /// </summary>
        public static float updateThreadItemsTime = 0.1f;

        /// <summary>
        /// List of all queued unity thread actions
        /// </summary>
        static List<IThreadTask> UnityThreadQueuedActions = new List<IThreadTask>();

        public static bool inUnityThread
        {
            get
            {
                return Thread.CurrentThread == unityThreadIdentifier;
            }
        }

        /// <summary>
        /// Grab the unity thread to check if we are in it in the future.
        /// </summary>
        static Thread unityThreadIdentifier;

        public static void InitializeIfNotAvailable()
        {
            if(instance == null)
            {
                GameObject go = GameObject.Find("UN Threading Manager");

                if (go != null)
                {
                    GameObject.DestroyImmediate(go);
                }

                go = new GameObject("UN Threading Manager");
                _instance = go.AddComponent<UNThreadManager>();

                Debug.Log("uNature Threading Manager Has Been Initialized Successfully !!");
            }
        }

        /// <summary>
        /// Called when the object is enabled
        /// </summary>
        void OnEnable()
        {
            _instance = this;

            unityThreadIdentifier = Thread.CurrentThread;

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SceneView.onSceneGUIDelegate += OnSceneView;
            }
            #endif

            ThreadItem._threadItems = null;

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(threadWorkersCount, threadWorkersCount);
        }

        /// <summary>
        /// Called when the object is disabled
        /// </summary>
        void OnDisable()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneView;
            }
            #endif
        }

        /// <summary>
        /// Called when the object firstly enabled.
        /// </summary>
        void Awake()
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                UnityEditor.SceneView.onSceneGUIDelegate += OnSceneView;
            }
            #endif
        }

        /// <summary>
        /// Updates the thread items in the scene.
        /// </summary>
        public void UpdateThreadItems()
        {
            List<ThreadItem> items = ThreadItem.threadItems;

            for(int i = 0; i < items.Count; i++)
            {
                items[i].UpdateItem();
            }
        }

        /// <summary>
        /// Access to unity thread
        /// </summary>
        void Update()
        {
            if (!Application.isPlaying) return;

            threadUpdate();
        }

        /// <summary>
        /// Update the unity thread
        /// </summary>
        private void threadUpdate()
        {
            IThreadTask queuedData;

            for (int i = 0; i < UnityThreadQueuedActions.Count; i++)
            {
                queuedData = UnityThreadQueuedActions[i];

                if (queuedData != null)
                {
                    queuedData.Invoke();
                }
            }

            UnityThreadQueuedActions.Clear();
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Called on scene view
        /// </summary>
        private void OnSceneView(UnityEditor.SceneView sView)
        {
            if (Application.isPlaying) return;

            threadUpdate();
        }

        private void OnDrawGizmos()
        {
            if(UNSettings.instance.UN_Console_Debugging_Enabled && UNSettings.instance.UN_Debugging_Enabled && FoliageCore_MainManager.instance != null && FoliageCore_MainManager.instance.editorQueueInstance != null)
            {
                RenderingPipielineUtility.RenderQueueDebugMode(FoliageCore_MainManager.instance.editorQueueInstance.renderingQueue);
            }
        }
        #endif

        /// <summary>
        /// Add an action to the unity thread
        /// </summary>
        /// <param name="action">the action</param>
        public void RunOnUnityThread(IThreadTask action)
        {
            if (action == null) return;

            if (inUnityThread)
            {
                action.Invoke();
            }
            else
            {
                UnityThreadQueuedActions.Add(action);
            }
        }

        /// <summary>
        /// Add an action to the UN thread
        /// </summary>
        /// <param name="action">the action</param>
        public void RunOnThread(IThreadTask action)
        {
            if (threadEnabled)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(OnThreadProcess), action);
            }
            else
            {
                RunOnUnityThread(action);
            }
        }

        /// <summary>
        /// Called when the thread needs to process the task.
        /// </summary>
        /// <param name="processObject"></param>
        protected void OnThreadProcess(System.Object processObject)
        {
            IThreadTask task = processObject as IThreadTask;

            if (task != null)
            {
                try
                {
                    task.Invoke();
                }
                catch(UnityException ex)
                {
                    Debug.LogError("uNature Thread Manager : Error caught while running thread action : \n" + ex);
                }
            }
            else
            {
                Debug.LogError("uNature Thread Manager : Unrecognized thread process : " + processObject.ToString());
            }
        }

        #region CoroutinesUtility

        /// <summary>
        /// Run any action with a specific delay of seconds.
        /// </summary>
        /// <param name="task">the task you want to run after the specific amount of seconds</param>
        /// <param name="time">the specific amount of seconds to wait</param>
        public void DelayActionSeconds(IThreadTask task, float time)
        {
            StartCoroutine(DelayActionSecondsCoroutine(task, time));
        }

        /// <summary>
        /// Run any action after 1 frame
        /// </summary>
        /// <param name="task">the task you want to run after 1 frame</param>
        public void DelayActionFrames(int frames, IThreadTask task)
        {
            StartCoroutine(DelayActionFrameCoroutine(frames, task));
        }

        /// <summary>
        /// Run any action with a specific delay of seconds.
        /// </summary>
        /// <param name="task">the task you want to run after the specific amount of seconds</param>
        /// <param name="time">the specific amount of seconds to wait</param>
        private IEnumerator DelayActionSecondsCoroutine(IThreadTask task, float time)
        {
            yield return new WaitForSeconds(time);

            task.Invoke();
        }

        /// <summary>
        /// Run any action after 1 frame
        /// </summary>
        /// <param name="task">the task you want to run after 1 frame</param>
        private IEnumerator DelayActionFrameCoroutine(int frames, IThreadTask task)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            task.Invoke();
        }

        #endregion
    }

    #region Tasks
    /// <summary>
    /// A thread task that takes no parameters.
    /// </summary>
    public class ThreadTask : IThreadTask
    {
        System.Action action;
        int frame;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }

        public ThreadTask(System.Action _action)
        {
            action = _action;

            if (UNThreadManager.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action();
        }
    }
    /// <summary>
    /// A thread task that takes 1 parameter.
    /// <typeparam name="T">Type 1</typeparam>
    /// </summary>
    public class ThreadTask<T> : IThreadTask
    {
        System.Action<T> action;
        T data;
        int frame;

        System.Action<ThreadTask<T>> onDone;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }
        
        public ThreadTask(System.Action<T> _action, T _data)
        {
            action = _action;
            data = _data;

            if (UNThreadManager.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action(data);
        }
    }
    /// <summary>
    /// A thread task that takes 2 parameters.
    /// <typeparam name="T">Type 1</typeparam>
    /// <typeparam name="T1">Type 2</typeparam>
    /// </summary>
    public class ThreadTask<T, T1> : IThreadTask
    {
        System.Action<T, T1> action;
        T data1;
        T1 data2;
        int frame;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }

        public ThreadTask(System.Action<T, T1> _action, T _data1, T1 _data2)
        {
            action = _action;
            data1 = _data1;
            data2 = _data2;

            if (UNThreadManager.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action(data1, data2);
        }
    }
    /// <summary>
    /// A thread task that takes 3 parameters.
    /// <typeparam name="T">Type 1</typeparam>
    /// <typeparam name="T1">Type 2</typeparam>
    /// <typeparam name="T2">Type 3</typeparam>
    /// </summary>
    public class ThreadTask<T, T1, T2> : IThreadTask
    {
        System.Action<T, T1, T2> action;
        T data1;
        T1 data2;
        T2 data3;
        int frame;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }

        public ThreadTask(System.Action<T, T1, T2> _action, T _data1, T1 _data2, T2 _data3)
        {
            action = _action;
            data1 = _data1;
            data2 = _data2;
            data3 = _data3;

            if (UNThreadManager.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action(data1, data2, data3);
        }
    }
    /// <summary>
    /// A thread task that takes 4 parameters.
    /// <typeparam name="T">Type 1</typeparam>
    /// <typeparam name="T1">Type 2</typeparam>
    /// <typeparam name="T2">Type 3</typeparam>
    /// <typeparam name="T3">Type 4</typeparam>
    /// </summary>
    public class ThreadTask<T, T1, T2, T3> : IThreadTask
    {
        System.Action<T, T1, T2, T3> action;

        T data1;
        T1 data2;
        T2 data3;
        T3 data4;
        int frame;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }

        public ThreadTask(System.Action<T, T1, T2, T3> _action, T _data1, T1 _data2, T2 _data3, T3 _data4)
        {
            action = _action;
            data1 = _data1;
            data2 = _data2;
            data3 = _data3;
            data4 = _data4;

            if (UNThreadManager.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action(data1, data2, data3, data4);
        }
    }

    /// <summary>
    /// A thread task interface.
    /// Implement on any customely created thread task.
    /// </summary>
    public interface IThreadTask
    {
        void Invoke();
        int creationFrame { get; }
    }
    #endregion

    public enum uNature_Thread_Workers
    {
        One_Worker = 1,
        Two_Workers = 2,
        Three_Workers = 3,
        Four_Workers = 4,
        Five_Workers = 5
    }
}