using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    
    [BaseContentOrder(5)]
    public abstract class ChipGenerator : SlotContent {
        
        LevelColorSettings colorSettings;
        protected ChipGeneratorController controller;

        public override Type GetContentBaseType() {
            return typeof(ChipGenerator);
        }

        public override Type BodyType => typeof(ChipGeneratorBody);

        public override bool IsUniqueContent() {
            return true;
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            colorSettings = context.GetArgument<LevelColorSettings>();
            if (context.SetupItem(out controller)) 
                controller.generators.Add(this);
        }

        public override void OnChangeSlot() {
            base.OnChangeSlot();
            random = field.random.NewRandom($"Generator_{slotModule.Center}");
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space); 
            controller?.generators.Remove(this);
        }

        public bool IsActual() {
            if (!enabled) return false;
            var slot = slotModule.Slot();
            return slot.enabled && slot.nearSlot.Values
                .NotNull()
                .All(s => !s.falling.ContainsValue(slot) 
                              || (s.IsBlocked(out var t) && t == Block.BlockingType.Obstacle));
        }

        public bool IsEmpty() {
            return !slotModule.HasContent<Chip>() && !slotModule.HasContent<Block>();
        }
        
        public abstract ContentInfo GetNextChipInfo();

        public Chip Generate(ContentInfo chipInfo) {
            if (!IsEmpty() || chipInfo?.Reference == null) 
                return null;
            
            chipInfo = chipInfo.Clone();
            
            #region Apply Mask
            {
                var colorVariable = chipInfo.GetVariable<ColoredVariable>();
                if (colorVariable != null && colorVariable.info.IsMatchableColor())
                    colorVariable.info = ItemColorInfo.ByID(colorSettings.Convert(colorVariable.info.ID));
            }
            #endregion
            
            var content = chipInfo.Reference.Clone();
            
            content.random = random.NewRandom();

            content.ApplyDesign(chipInfo);

            if (content is SlotContent sc)
                sc.emitType = EmitType.Generated;
            
            field.AddContent(content);
            
            content.position = position;
            
            var chip = content as Chip;
            
            slotModule.AddContent(chip);
            
            return chip;
        }

        public ContentInfo GetNext() {
            return slotModule.Slot().GetContent<ChipGeneratorCharge>()?.GetNext() ?? GetNextChipInfo();
        }
    }
    
    public class ChipGeneratorController : LevelContent {
        
        public List<ChipGenerator> generators = new List<ChipGenerator>();
        public List<ChipLimiter> limiters = new List<ChipLimiter>();
        
        public override Type GetContentBaseType() {
            return typeof(ChipGeneratorController);
        }
        
        public bool Generate() {
            
            var result = false;

            foreach (var generator in generators.Where(g => g.IsActual())) {
                if (!generator.IsEmpty()) 
                    continue;
                
                var chip = generator.Generate(generator.GetNext());
                if (chip) {
                    result = true;
                    events.onGenerate.Invoke(chip);                
                }
            }
            
            return result;
        }
    }
}