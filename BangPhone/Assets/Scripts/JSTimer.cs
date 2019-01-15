using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using UnityEngine.AI;

public class JSTimer : MonoBehaviour
{
    //--------------------单例---------------------------
    private static JSTimer mInstance;
    public static JSTimer Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject("JSTimer");
                mInstance = go.AddComponent<JSTimer>();
                DontDestroyOnLoad(go);
            }
            return mInstance;
        }
    }
    //---------------------------------------------------


    public abstract class Task
    {
        public string taskName;
        public float updateFrequence;
        public bool isPause;
        public float currentAccumulate;
        public bool isVaild;
        public bool timeScale;
    }

    public class TimerTask : Task
    {
        public delegate void OnTimeUpdate();
        public OnTimeUpdate onUpdate;

        public TimerTask(string taskName, OnTimeUpdate onUpdate, float updateFrequence, bool timeScale)
        {
            this.taskName = taskName;
            Reset(onUpdate, updateFrequence, timeScale);
        }

        public void Reset(OnTimeUpdate onUpdate, float updateFrequence, bool timeScale)
        {
            this.currentAccumulate = 0f;
            this.updateFrequence = updateFrequence;
            this.onUpdate = onUpdate;
            this.isPause = false;
            this.timeScale = timeScale;
            this.isVaild = true;
        }

        public void Cancel()
        {
            this.isVaild = false;
        }

        public void DoUpdate()
        {
            if (onUpdate != null)
                onUpdate();
        }

        public void OnDispose()
        {
            onUpdate = null;
        }
    }

    public class CDTask : Task
    {
        public delegate void OnUpdate(float remainTime);
        public delegate void OnFinish();

        public OnUpdate onUpdate;
        public OnFinish OnFinished;

        public float totalTime;
        public float remainTime;

        public CDTask(string taskName, float totalTime, OnUpdate onUpdate, OnFinish onFinished, float updateFrequence = 0.1f, bool timeScale = false)
        {
            this.taskName = taskName;
            Reset(totalTime, onUpdate, onFinished, updateFrequence, timeScale);
        }

        public void Reset(float totalTime, OnUpdate onUpdate, OnFinish onFinished, float updateFrequence, bool timeScale = false)
        {
            this.totalTime = totalTime;
            this.remainTime = totalTime;
            this.onUpdate = onUpdate;
            this.OnFinished = onFinished;

            this.updateFrequence = updateFrequence;
            this.currentAccumulate = 0f;
            this.isPause = false;
            this.timeScale = timeScale;
            this.isVaild = true;

            JSTimer.Instance.AddCdIsNotExist(this);
        }

        public void DoUpdate()
        {
            if (onUpdate != null)
                onUpdate(remainTime);
        }

        public void DoFinish()
        {
            if (OnFinished != null)
                OnFinished();
        }

        public void Cancel()
        {
            this.isVaild = false;
        }

        public void OnDispose()
        {
            onUpdate = null;
            OnFinished = null;
        }
    }

    private List<CDTask> mCdTasks = new List<CDTask>(32);
    public List<CDTask> CdTasks
    {
        get { return mCdTasks; }
    }

    private List<TimerTask> mTimeTasks = new List<TimerTask>(32);
    public List<TimerTask> TimeTasks
    {
        get { return mTimeTasks; }
    }





    /// <summary>
    /// 倒计时
    /// </summary>
    /// <param name="taskName"></param>
    /// <param name="totalTime"></param>
    /// <param name="onUpdate"></param>
    /// <param name="onFinished"></param>
    /// <param name="updateFrequence"></param>
    /// <param name="timeScale"></param>
    /// <returns></returns>
    public CDTask SetupCoolDown(string taskName, float totalTime, CDTask.OnUpdate onUpdate, CDTask.OnFinish onFinished, float updateFrequence = 0.1f, bool timeScale = false)
    {
        if (string.IsNullOrEmpty(taskName))
            return null;

        if (totalTime <= 0)
        {
            if (onFinished != null)
            {
                onFinished();
            }
            return null;
        }

        CDTask cdTask = GetCdTask(taskName);
        if (cdTask != null)
        {
            cdTask.Reset(totalTime, onUpdate, onFinished, updateFrequence, timeScale);
        }
        else
        {
            cdTask = new CDTask(taskName, totalTime, onUpdate, onFinished, updateFrequence, timeScale);
        }
        return cdTask;
    }

    public CDTask GetCdTask(string taskName)
    {
        return mCdTasks.Find((task) =>
        {
            return task.taskName.Equals(taskName);
        });
    }

    public bool IsCdExist(string taskName)
    {
        return GetCdTask(taskName) != null;
    }

    public bool AddCdIsNotExist(CDTask pCdTask)
    {
        if (null == pCdTask)
            return false;

        if (IsCdExist(pCdTask.taskName))
            return false;

        mCdTasks.Add(pCdTask);
        return true;
    }

    public bool PauseCd(string taskName)
    {
        CDTask task = GetCdTask(taskName);
        if (task != null)
        {
            task.isPause = true;
            return true;
        }
        return false;
    }

    public bool ResumeCd(string taskName)
    {
        CDTask task = GetCdTask(taskName);
        if (task != null)
        {
            task.isPause = false;
            return true;
        }
        return false;
    }

    public void CancelCd(string taskName)
    {
        CDTask task = GetCdTask(taskName);
        if (task != null)
        {
            task.isVaild = false;
        }
    }

    public float GetRemainTime(string taskName)
    {
        CDTask task = GetCdTask(taskName);
        if (task != null)
        {
            return task.remainTime;
        }
        return 0;
    }

    public void AddCdUpdateHandler(string taskName, CDTask.OnUpdate onUpdateHandler)
    {
        CDTask task = GetCdTask(taskName);
        if (task != null)
        {
            task.onUpdate -= onUpdateHandler;
            task.onUpdate += onUpdateHandler;
        }
    }
    public void RemoveCdUpdateHandler(string taskName, CDTask.OnUpdate onUpdateHandler)
    {
        CDTask task = GetCdTask(taskName);
        if (task != null)
        {
            task.onUpdate -= onUpdateHandler;
        }
    }

    public void AddCdFinishedandler(string taskName, CDTask.OnFinish onFinishedHandler)
    {
        CDTask task = GetCdTask(taskName);
        if (task != null)
        {
            task.OnFinished -= onFinishedHandler;
            task.OnFinished += onFinishedHandler;
        }
    }
    public void RemoveCdFinishedandler(string taskName, CDTask.OnFinish onFinishedHandler)
    {
        CDTask task = GetCdTask(taskName);
        if (task != null)
        {
            task.OnFinished -= onFinishedHandler;
        }
    }

    /// <summary>
    /// 计时器
    /// </summary>
    /// <param name="taskName"></param>
    /// <param name="onUpdate"></param>
    /// <param name="updateFrequence"></param>
    /// <param name="timeScale"></param>
    /// <returns></returns>
    public TimerTask SetupTimer(string taskName, TimerTask.OnTimeUpdate onUpdate, float updateFrequence = 0.1f, bool timeScale = false)
    {
        if (string.IsNullOrEmpty(taskName))
            return null;

        TimerTask timeTask = GetTimerTask(taskName);
        if (timeTask != null)
        {
            timeTask.Reset(onUpdate, updateFrequence, timeScale);
        }
        else
        {
            timeTask = new TimerTask(taskName, onUpdate, updateFrequence, timeScale);
            mTimeTasks.Add(timeTask);
        }
        return timeTask;
    }

    public TimerTask GetTimerTask(string taskName)
    {
        return mTimeTasks.Find((task) =>
        {
            return task.taskName.Equals(taskName);
        });
    }

    public bool IsTimerExist(string taskName)
    {
        return GetTimerTask(taskName) != null;
    }

    public bool PauseTimer(string taskName)
    {
        TimerTask timerTask = GetTimerTask(taskName);
        if (timerTask != null)
        {
            timerTask.isPause = true;
            return true;
        }
        return false;
    }

    public bool ResumeTimer(string taskName)
    {
        TimerTask timerTask = GetTimerTask(taskName);
        if (timerTask != null)
        {
            timerTask.isPause = false;
            return true;
        }
        return false;
    }

    public void CancelTimer(string taskName)
    {
        TimerTask timerTask = GetTimerTask(taskName);
        if (timerTask != null)
        {
            timerTask.isVaild = false;
        }
    }

    public void AddTimerUpdateHandler(string taskName, TimerTask.OnTimeUpdate onUpdate)
    {
        TimerTask timerTask = GetTimerTask(taskName);
        if (timerTask != null)
        {
            timerTask.onUpdate -= onUpdate;
            timerTask.onUpdate += onUpdate;
        }
    }

    public void RemoveTimerUpdateHandler(string taskName, TimerTask.OnTimeUpdate onUpdate)
    {
        TimerTask timerTask = GetTimerTask(taskName);
        if (timerTask != null)
        {
            timerTask.onUpdate -= onUpdate;
        }
    }

    private List<TimerTask> mTimerToRemove = new List<TimerTask>();
    private List<CDTask> mCoolDownToRemove = new List<CDTask>();

    void Update()
    {
        //计时器任务
        for (int i = 0, imax = mTimeTasks.Count; i < imax; ++i)
        {
            TimerTask timerTask = mTimeTasks[i];
            if (timerTask.isVaild)
            {
                if (timerTask.isPause)
                    continue;

                float deltaTime = timerTask.timeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                timerTask.currentAccumulate += deltaTime;
                if (timerTask.currentAccumulate >= timerTask.updateFrequence)
                {
                    timerTask.currentAccumulate = 0f;
                    timerTask.DoUpdate();
                }
            }
            else
            {
                mTimerToRemove.Add(timerTask);
            }
        }

        if (mTimerToRemove.Count > 0)
        {
            for (int i = 0; i < mTimerToRemove.Count; ++i)
            {
                TimerTask timerTask = mTimerToRemove[i];
                timerTask.OnDispose();
                mTimerToRemove.Remove(timerTask);
            }

            mTimerToRemove.Clear();
        }

        //倒计器任务
        for (int i = 0, imax = mCdTasks.Count; i < imax; ++i)
        {
            CDTask cdTask = CdTasks[i];
            if (cdTask.isVaild)
            {
                if (cdTask.isPause)
                    continue;

                float deltaTime = cdTask.timeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                cdTask.remainTime -= deltaTime;
                if (cdTask.remainTime <= 0)
                {
                    cdTask.remainTime = 0f;
                    cdTask.isVaild = false;
                    cdTask.DoUpdate();
                    cdTask.DoFinish();
                }
                else
                {
                    cdTask.currentAccumulate += deltaTime;
                    if (cdTask.currentAccumulate >= cdTask.updateFrequence)
                    {
                        cdTask.currentAccumulate = 0f;
                        cdTask.DoUpdate();
                    }
                }
            }
            else
            {
                mCoolDownToRemove.Add(cdTask);
            }
        }

        if (mCoolDownToRemove.Count > 0)
        {
            for (int i = 0; i < mCoolDownToRemove.Count; ++i)
            {
                CDTask cdTask = mCoolDownToRemove[i];
                cdTask.OnDispose();
                mCdTasks.Remove(cdTask);
            }
            mCoolDownToRemove.Clear();
        }
    }

    public void Dispose()
    {
        mCdTasks.Clear();
        mTimeTasks.Clear();
    }

}
