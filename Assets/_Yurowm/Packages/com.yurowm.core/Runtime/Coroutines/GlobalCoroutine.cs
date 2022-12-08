using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Yurowm.Extensions;
using Yurowm.Profiling;
using Yurowm.Utilities;

namespace Yurowm.Coroutines {
    public class GlobalCoroutine {
        static GlobalCoroutine _Instance;
        public static GlobalCoroutine Instance { 
            get {
                if (_Instance == null)
                    _Instance = new GlobalCoroutine();
                
                _Instance.Initialize();
                
                return _Instance;
            }
        }
        
        bool IsInitialized = false;
        
        void Initialize() {
            if (IsInitialized || !Utils.IsMainThread()) return;
            
            IsInitialized = true;
            
            _core = new CoroutineCore();
            
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            
            for (int i = 0; i < playerLoop.subSystemList.Length; i++) {
                var subSystem = playerLoop.subSystemList[i];
                
                if (subSystem.type == typeof(Update)) {
                    subSystem.updateDelegate += Update;
                } else if (subSystem.type == typeof(PostLateUpdate)) {
                    subSystem.updateDelegate += LateUpdate;
                } else if (subSystem.type == typeof(FixedUpdate)) {
                    subSystem.updateDelegate += FixedUpdate;
                } else continue;

                playerLoop.subSystemList[i] = subSystem;
            }
            
            PlayerLoop.SetPlayerLoop(playerLoop);
        }
        
        
        GlobalCoroutine() {}
        
        CoroutineCore _core;
        
        CoroutineCore core {
            get {
                #if UNITY_EDITOR
                if (_core == null && !Application.isPlaying)
                    return EditorCoroutine.GetCore();
                #endif
                return _core;
            }
        }
        
        public CoroutineCore GetCore() {
            return core;
        }

        void Update() {
            using (YProfiler.Area("Global Update"))
                core?.Update(CoroutineCore.Loop.Update);
        }
        
        void LateUpdate() {
            using (YProfiler.Area("Global LateUpdate"))
                core?.Update(CoroutineCore.Loop.LateUpdate);
        }
        
        void FixedUpdate() {
            using (YProfiler.Area("Global FixedUpdate"))
                core?.Update(CoroutineCore.Loop.FixedUpdate);
        }

        public static IEnumerator RunConsistently(params IEnumerator[] logics) {
            return Instance.core.RunConsistently(logics);
        }

        public static IEnumerator RunParallel(params IEnumerator[] logics) {
            return Instance.core.RunParallel(logics);
        }

        public static IEnumerator Run(IEnumerator logic, CoroutineOptions options = CoroutineOptions.Default, CoroutineCore.Loop loop = CoroutineCore.Loop.Update, int order = 0) {
            return Instance.core.Run(logic, options, loop, order);
        }

        public static void Stop(IEnumerator logic) {
            Instance.core.Stop(logic);
        }
    }

    public class CoroutineCore {
        List<Routine> sortedRoutines = new List<Routine>();
        List<Routine> routines = new List<Routine>();
        Queue<Action> singleCallEvents = new Queue<Action>();
        Action onUpdate = delegate { };
        bool inUpdate = false;

        public enum Loop {
            Update,
            FixedUpdate,
            LateUpdate
        }
        
        public bool playModeOnly = true;

        public void Update(Loop loop) {
            if (!Enable) return;
            
            if (playModeOnly && !Application.isPlaying) return;
            
            while (singleCallEvents.Count > 0) {
                try {
                    singleCallEvents.Dequeue().Invoke();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
            
            inUpdate = true;
            
            bool vacuum = false;
            foreach (var routine in Routines()) {
                if (routine.loop != loop) 
                    continue;
                routine.Update();
                if (!vacuum && routine.IsComplete())
                    vacuum = true;
            }
            
            if (vacuum)
                RemoveRoutines(VacuumPredicate);
            
            if (onUpdate != null) {
                onUpdate.Invoke();
                onUpdate = delegate { };
            }
            
            inUpdate = false;
        }
        
        IEnumerable<Routine> Routines() {
            foreach (var routine in sortedRoutines)
                if (routine.order < 0)
                    yield return routine;
            
            foreach (var routine in routines) 
                yield return routine;
            
            foreach (var routine in sortedRoutines) 
                if (routine.order > 0)
                    yield return routine;
        }
        
        public void Clear() {
            Routines().ForEach(r => r.Destroy());
            
            routines.Clear();
            sortedRoutines.Clear();
            
            singleCallEvents.Clear();
            onUpdate = null;
        }

        public bool Enable {get; set;} = true;
        
        static bool VacuumPredicate(Routine routine) {
            return routine.IsComplete();
        }

        public void SingleCall(Action action) {
            singleCallEvents.Enqueue(action);
        }
        
        public IEnumerator Run(IEnumerator logic, CoroutineOptions options = CoroutineOptions.Default, Loop loop = Loop.Update, int order = 0) {
            var result = new Routine(logic, options, loop);
            result.order = order;
            
            if (inUpdate)
                onUpdate += () => NewRoutine(result);
            else
                NewRoutine(result);
            
            return result;
        }
        
        public void Stop(IEnumerator logic) {
            if (inUpdate)
                onUpdate += () => StopAll(logic);
            else
                StopAll(logic);
        }

        void StopAll(IEnumerator logic) {
            RemoveRoutines(r => r == logic || r.logic == logic);
        }
        
        void NewRoutine(Routine routine) {
            if (routine.order == 0)
                routines.Add(routine);
            else {
                sortedRoutines.Add(routine);
                sortedRoutines.Sort(r => r.order);
            }
        }
        
        void RemoveRoutines(Predicate<Routine> match) {
            routines.RemoveAll(match);
            sortedRoutines.RemoveAll(match);
        }

        #region Multiple Flow
        public IEnumerator RunConsistently(params IEnumerator[] logics) {
            return Run(logics.CoroutineConsistently());
        }


        public IEnumerator RunParallel(params IEnumerator[] logics) {
            return Run(logics.Parallel());
        }
        #endregion

        internal class Routine : IEnumerator {
            public IEnumerator logic;
            Routine instruction = null;
            internal int order = 0;
            bool isComplete = false;
            CoroutineOptions options;
            public Loop loop = Loop.Update;

            static DelayedAccess skipFramesAccess = new DelayedAccess(1f / 20);
            
            public object Current => null;

            public Routine(IEnumerator logic, CoroutineOptions options, Loop loop) {
                this.options = options;
                this.logic = logic;
                this.loop = loop;
                
                if (options.HasFlag(CoroutineOptions.Complete))
                    this.logic = this.logic.Complete();
                if (options.HasFlag(CoroutineOptions.Immediate))
                    Update();
            }

            public void Update() {
                if (isComplete)
                    return;

                try {
                    if (instruction != null) {
                        if (!instruction.MoveNext())
                            instruction = null;
                        else
                            return;
                    }

                    if (logic.MoveNext()) {
                        if (logic.Current is IEnumerator enumerator)
                            instruction = new Routine(enumerator, options, loop);
                        
                        if (logic.Current is YieldInstruction yieldInstruction)
                            instruction = new Routine(YieldInstructionCover(yieldInstruction), options, loop);
                        
                    } else
                        isComplete = true;
                } catch (Exception e) {
                    Debug.LogException(e);
                    isComplete = true;
                }
            }
            
            IEnumerator YieldInstructionCover(YieldInstruction instruction) {
                switch (instruction) {
                    case AsyncOperation ao:
                        while (!ao.isDone)
                            yield return null;
                        break;
                    default: 
                        yield return instruction;
                        break;
                }
            }
            
            public void Destroy() {
                logic = null;
                instruction = null;
                isComplete = true;
            }

            public bool IsComplete() {
                return isComplete;
            }

            public void Complete() {
                while (MoveNext()) { }
            }

            public bool MoveNext() {
                Update();
                
                if (options.HasFlag(CoroutineOptions.SkipFrames))
                    while (!skipFramesAccess.GetAccess()) {
                        Update();   
                        if (IsComplete()) break;
                    }
                    
                return !IsComplete();
            }

            public void Reset() { }
        }
    }
    
    public static class CoroutineExtensions {
        
        public static IEnumerator ContinueWith(this IEnumerator current, IEnumerator next) {
            yield return current;
            yield return next;
        }
        
        public static IEnumerable<IEnumerator> With(this IEnumerator current, params IEnumerator[] next) {
            yield return current;
            foreach (var enumerator in next)
                yield return enumerator;
        }

        public static IEnumerator ContinueWith(this IEnumerator current, Action next) {
            yield return current;
            next?.Invoke();
        }

        public static IEnumerator Run(this IEnumerator logic, CoroutineCore core = null,
                    CoroutineOptions options = CoroutineOptions.Default,
                    CoroutineCore.Loop loop = CoroutineCore.Loop.Update,
                    int order = 0) {

            if (core == null)
                return GlobalCoroutine.Run(logic, options, loop, order);
            else
                return core.Run(logic, options, loop, order);
        }

        public static void Stop(this IEnumerator logic, CoroutineCore core = null) {
            if (core == null)
                GlobalCoroutine.Stop(logic);
            else
                core.Stop(logic);
        }
        
        public static IEnumerator CoroutineConsistently(this IEnumerable<IEnumerator> logics) {
            foreach (var logic in logics)
                yield return logic;
        }

        public static IEnumerator Parallel(this IEnumerable<IEnumerator> logics) {
            var routines = logics.Select(l => new CoroutineCore.Routine(l, CoroutineOptions.Default, CoroutineCore.Loop.Update)).ToList();
            while (routines.Count > 0) {
                routines.RemoveAll(ParallelPredicate);
                routines.ForEach(r => r.Update());
                yield return null;
            }
        }
        
        static bool ParallelPredicate(CoroutineCore.Routine routine) {
            return routine.IsComplete();
        }
        
        
        public static void RunInMainThread(this Action action, CoroutineCore core = null, bool wait = true) {
            if (Utils.IsMainThread()) {
                action();
            } else {
                bool complete = false;
                
                Action logic = () => {
                    try {
                        action();
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }
                    complete = true;
                };

                if (core == null)
                    core = GlobalCoroutine.Instance.GetCore();

                core.SingleCall(logic);
                
                if (wait)
                    while (!complete)
                        Thread.Sleep(3);
            }
        }
        
        public static void RunInMainThread(this IEnumerator logic, CoroutineCore core = null, bool skipFrames = false) {
            var options = CoroutineOptions.Complete;
            if (skipFrames) options |= CoroutineOptions.SkipFrames;
            
            core ??= GlobalCoroutine.Instance.GetCore();
            
            if (Utils.IsMainThread()) {
                UnityEngine.Debug.LogError("It is already the main thread");
                logic.Run(core, options);
                return;
            } 
            
            var complete = false;
            
            logic.ContinueWith(() => complete = true).Run(core, options);
            
            while (!complete)
                Thread.Sleep(3);
        }
        
    }
    
    [Flags]
    public enum CoroutineOptions {
        Run = 1 << 0,
        Complete = 1 << 1,
        SkipFrames = 1 << 2,
        Immediate = 1 << 3,
        Default = Run | Immediate
    }
}
