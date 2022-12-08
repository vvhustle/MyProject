using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using YMatchThree.Meta;
using Yurowm;
using Yurowm.ComposedPages;
using Yurowm.ContentManager;
using Yurowm.Controls;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Features;
using Yurowm.Integrations;
using Yurowm.Jobs;
using Yurowm.Spaces;
using Yurowm.UI;
using Yurowm.Utilities;
using Behaviour = Yurowm.Behaviour;
using Page = Yurowm.UI.Page;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Seasons {
    public class LevelButton : SpacePhysicalItem, IClickable {
        
        public LevelScriptOrdered level;

        ContentAnimator animator;
        
        LevelButtonBody buttonBody;
        
        bool locked;

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            if (body is LevelButtonBody button) {
                buttonBody = button;
                
                button.SetText(level.order.ToString());
                SetStars(0);
            } 

            body.SetupComponent(out animator);
        }
        
        public void SetStars(int count) {
            buttonBody?.SetStars(count);
        }

        public void Lock() {
            locked = true;
            animator?.Rewind("Unlock");
        }
        
        public void UnlockFast() {
            locked = false;
            animator?.RewindEnd("Unlock");
        }
        
        public void UnlockCurrent() {
            locked = false;
            animator?.Play("Current");
        }

        Vector2 ICastable.position => body.transform.position; 
        
        public bool IsAvaliableForClick(int mode) {
            return !locked;
        }

        public float clickableRadius => 1;

        public void OnClick(int mode) {
            OnClickLogic().Run(space.coroutine);
        }
        
        IEnumerator OnClickLogic() {
            var stars = Behaviour.GetAll<LevelCompleteStar>().ToList();
                
            stars.ForEach(g => g.No());

            if (level is LevelScriptOrdered ordered) {
                ordered.OnSelect();
                
                var worldPropgress = PlayerData.levelProgress.GetWorldPropgress(ordered.worldName, true);
                var bestScore = worldPropgress.GetBestScore(level.ID);    
                
                stars.RemoveAll(s => level.stars.GetCount(bestScore) < s.number);
            }
            
            var startButton = Behaviour.GetByID<Button>("StartLevel");
            startButton.onClick.SetSingleListner(StartLevel);
            
            yield return Page.Get("LevelSelected").ShowAndWait();

            stars.ForEach(s => s.Award());
        }
        
        void StartLevel() {
            if (level != null)
                PuzzleSpace.TryToStart(level);
        }
    }
}