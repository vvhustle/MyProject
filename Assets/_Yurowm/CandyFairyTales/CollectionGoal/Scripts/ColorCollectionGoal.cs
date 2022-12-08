using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm;
using Yurowm.Colors;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class ColorCollectionGoal : CollectionGoal, IColored {
        
        public ItemColorInfo colorInfo { get; set; }

        public override bool IsUnique => false;

        Sprite icon;
        
        public override void OnAddToSpace(Space space) {
            var colorSettings = context.GetArgument<LevelColorSettings>();
            colorInfo = ItemColorInfo.ByID(colorSettings.Convert(colorInfo.ID)); 
            
            var chipBodyName = context?.GetArgument<LevelScriptBase>().chipBody;
            
            if (chipBodyName.IsNullOrEmpty())
                chipBodyName = storage.GetDefault<SimpleChip>().bodyName;
            
            if (!chipBodyName.IsNullOrEmpty()) {
                var chipBody = AssetManager.GetPrefab<SlotContentBody>(chipBodyName);
                icon = chipBody?.GetComponentInChildren<SetSpriteRepaint>()?.GetSprite(colorInfo.ID);
            }
            
            base.OnAddToSpace(space);
        }

        protected override bool ContentFilter(SlotContent content) {
            return content is IColored colored 
                   && colored.colorInfo.type == ItemColor.Known 
                   && colored.colorInfo.ID == colorInfo.ID;
        }

        public override Sprite GetIcon() {
            return icon;
        }

        public override void OnCounterCreated(LevelGoalCounter counter) {
            base.OnCounterCreated(counter);
            
            var counterGO = counter.gameObject;
            
            counterGO.Repaint(context.Get<ItemColorPalette>(), colorInfo);

            SetSpriteRepaint.Repaint(counterGO, colorInfo.ID);
        }

        #region Variables
        
        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes(); 
            yield return typeof(ColoredVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case ColoredVariable c: colorInfo = c.info; break;
            }
        }

        #endregion

    }
}