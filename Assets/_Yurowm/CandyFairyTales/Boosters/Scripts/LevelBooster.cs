using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YMatchThree.Meta;
using Yurowm;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Store;
using Yurowm.UI;
using Behaviour = Yurowm.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class LevelBooster : Booster {
        
        bool selected = false;
        
        protected PuzzleSpace puzzleSpace;

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            puzzleSpace = space as PuzzleSpace;
            
            Logic().Run(space.coroutine);
        }

        protected abstract IEnumerator Logic();

        public override void OnClick() {
            if (!selected) {
                if (PlayerData.inventory.GetItemCount(ID) > 0)
                    selected = true;
                else 
                    ShowInStore().Run();
            } else
                selected = false;

            UIRefresh.Invoke();
        }

        public override void SetupBody(VirtualizedScrollItemBody body) {
            base.SetupBody(body);
            
            if (!(body is BoosterButton bb)) return;
        
            if (bb.title)
                bb.title.text = titleText.GetText();
        
            if (bb.description)
                bb.description.text = descriptionText.GetText();
            
            bb.selected?.SetActive(selected);
        }

        #region Localizion

        public LocalizedText titleText = new LocalizedText();
        public LocalizedText descriptionText = new LocalizedText();
        
        public override IEnumerable GetLocalizationKeys() {
            yield return base.GetLocalizationKeys(); 
            yield return titleText; 
            yield return descriptionText; 
        }
        
        #endregion

        #region Lists
        
        const string boosterListID = "LevelBoosterList";
        const string waitButtonID = "LevelBoosterContinue";

        public static IEnumerator ShowLevelBoosters(PuzzleSpace space) {
            var boostersPage = Page.Get("Boosters");
            
            if (boostersPage == null)
                yield break;
            
            var boosterSet = space.context.Get<LevelBoosterSetBase>();
            
            if (boosterSet == null)
                yield break;
            
            var boosters = storage
                .Items<LevelBooster>()
                .Where(boosterSet.Filter)
                .Select(b => b.Clone())
                .ToArray();
            
            if (boosters.IsEmpty()) 
                yield break;

            Yurowm.Behaviour
                .GetAllByID<VirtualizedScroll>(boosterListID)
                .ToArray()
                .ForEach(l => l.SetList(boosters));
            
            Yurowm.Behaviour
                .GetAllByID<Button>(waitButtonID)
                .ForEach(l => 
                    l.onClick.SetSingleListner(Skip));
            
            bool wait = true;
            
            void Skip() {
                wait = false;
            }

            yield return boostersPage.ShowAndWait();

            while (wait)
                yield return null;
            
            boosters
                .Where(b => b.selected)
                .ForEach(space.AddItem);
        }
        
        #endregion

        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("titleText", titleText);
            writer.Write("descriptionText", descriptionText);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("titleText", ref titleText);
            reader.Read("descriptionText", ref descriptionText);
        }
        
        #endregion
    }
}