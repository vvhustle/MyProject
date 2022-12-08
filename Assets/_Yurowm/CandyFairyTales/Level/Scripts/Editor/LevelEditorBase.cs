using System;
using System.Collections.Generic;
using System.Linq;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public abstract class LevelEditorBase {
        public static readonly List<Type> allTypes = Utils.FindInheritorTypes<LevelEditorBase>(false, false)
            .Where(t => !t.IsAbstract).ToList();
        
        public LevelEditorContext context {get; private set;}
        
        public void SetContext(LevelEditorContext context) { 
            this.context = context;
        }
        
        public LevelEditorBase() {}        
        public abstract void OnGUI();
        
        public abstract void OnSelectAnotherLevel(LevelScriptOrderedFile file);

        public virtual void OnActionToolbarGUI() {}

        public virtual void OnChange() {}

        public virtual string GetName() {
            return GetType().Name.NameFormat();
        }
    }
}
