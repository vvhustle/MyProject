using System;
using System.Collections.Generic;
using System.Linq;
#if MXPARSER
using org.mariuszgromada.math.mxparser;
#endif
using UnityEngine;

namespace Yurowm.UI {
    public class ObjectMask : BaseBehaviour, IUIRefresh {

        public bool allChild = true;
        public List<GameObject> targets = new List<GameObject>();
    
        public ComparisonAction action = ComparisonAction.MakeActive;

        bool IUIRefresh.visible => gameObject.activeInHierarchy;
        
        public List<Arg> arguments;
        public string expression = "A == B";

        void Start () {
            
            Refresh();
            UIRefresh.Add(this);
        }
        
        void OnDestroy() {
            UIRefresh.Remove(this);
        }

        void OnEnable () {
            Refresh ();
        }

        public void Refresh () {
            if (!enabled) return;

            bool result = false;
            
            #if MXPARSER
            try {
                Expression exp = new Expression(expression, arguments.Select(i => i.Value).ToArray());
                result = exp.calculate() == 1;
            }
            catch (Exception e) {
                Debug.LogException(e);
                result = false;
            }
            #endif
            
            AllTargets(result);
        }

        void AllTargets (bool v) {
            if (allChild)
                foreach (Transform t in transform)
                    Action(t.gameObject, v);

            foreach (GameObject t in targets)
                Action(t, v);
        }

        void Action(GameObject go, bool v) {
            if (action == ComparisonAction.MakeActive) {
                go.SetActive(v);
                return;
            }
            if (action == ComparisonAction.UnlockButton) {
                go.GetComponent<UnityEngine.UI.Button>().interactable = v;
                return;
            }
        }

        [Serializable]
        public class Arg {
            public string name;
            public string reference;
            
            #if MXPARSER
            public Argument value;

            public Argument Value {
                get {
                    Calc();
                    return value;
                }
            }
            
            double argumentValue {
                get {
                    if (double.TryParse(ReferenceValues.Get(reference).ToString(), out var result))
                        return result;
                    return 0;
                }
            }

            public void Calc() {
                if (value == null)
                    value = new Argument(name, argumentValue);
                else {
                    value.setArgumentName(name);
                    value.setArgumentValue(argumentValue);
                }
            }
            #endif
        }
    }

    public enum ComparisonAction {
        MakeActive = 0, 
        UnlockButton = 1
    };
}
