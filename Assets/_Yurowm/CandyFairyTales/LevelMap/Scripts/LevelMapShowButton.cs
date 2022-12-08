using System.Collections;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Yurowm.UI;

namespace YMatchThree.Core {
    public class LevelMapShowButton : BaseBehaviour {
        
        public string levelMapID;
        

        void OnEnable() {
            if (this.SetupComponent(out Button button))
                button.SetAction(Show);
        }

        public void Show() {
            Show(levelMapID);
        }

        public static void Show(string ID) {
            Showing(ID).Run();
        }
        
        static IEnumerator Showing(string ID) {
            LevelMapSpace.nextLevelMapID = ID;
            
            yield return Page.Get("Loading").ShowAndWait();
                
            Space space = null;
            Space.Show<LevelMapSpace>(s => space = s);

            while (space == null)
                yield return null;
            
            Page.Get("Map").Show();
        }
    }
}