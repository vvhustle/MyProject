using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    [SlotTag(ConsoleColor.Yellow)]
    public class ExtendedChipGenerator : ChipGenerator, ISlotRectModifier {
        
        public override ContentInfo GetNextChipInfo() {
            cases.ForEach(c => c.enabled = controller.limiters.All(l => l.IsAllowed(c.info.Reference)));
            
            var casesWeight = cases
                .Where(c => c.enabled) 
                .Sum(c => c.weight);
            
            if (casesWeight <= 0) return null;
            
            float probability = random.Range(0, casesWeight);
            
            foreach (var c in cases) {
                if (!c.enabled) continue;
                
                probability -= c.weight;
                if (probability > 0) continue;

                return c.info;
            }
            
            return null;
        }

        public override SpaceObject EmitBody() {
            var sprites = cases
                .OrderByDescending(c => c.weight)
                .Select(c => c.info.Reference)
                .CastIfPossible<Chip>()
                .Where(c => !c.IsDefault && !c.miniIcon.IsNullOrEmpty())
                .Select(c => AssetManager.GetAsset<Sprite>(c.miniIcon))
                .NotNull()
                .ToArray();
            
            if (sprites.IsEmpty())
                return null;
            
            var body = base.EmitBody();
            
            if (!body)
                return null;
            
            var iconsParent = body.transform
                .GetChildByName("Icons")
                .GetComponent<RectTransform>();
            
            var renderers = iconsParent
                .GetComponentsInChildren<SpriteRenderer>(true);
            
            
            
            for (var i = 0; i < renderers.Length; i++) {
                var renderer = renderers[i]; 
                renderer.gameObject.SetActive(i < sprites.Length); 

                if (!renderer.gameObject.activeSelf)
                    continue;

                renderer.sprite = sprites[i];
                
                var rect = renderer.GetOrAddComponent<RectTransform>();
                
                var pos = sprites.Length == 1 ?
                    .5f : 1f * i / (sprites.Length - 1);

                rect.anchorMin = new Vector2(pos, 0);
                rect.anchorMax = rect.anchorMin;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            
            return body;
        }

        Side orientationSide = Side.Null;
        
        public override void OnChangeSlot() {
            base.OnChangeSlot();
            slotModule.Slot().onCalculateFallingSlot += () => {
                orientationSide = slotModule.Slot()?.falling
                    .FirstOrDefault(p => p.Value != null)
                    .Key ?? Side.Null;

                if (orientationSide.IsNotNull())
                    direction = orientationSide.ToAngle() - 270f;
                else
                    direction = 0;
            };
        }

        #region Variables

        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(ECGCasesVariable);
        }

        #endregion

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case ECGCasesVariable ecgcv: {
                    cases = ecgcv.value; 
                    break;
                }
            }
        }

        #region Cases
                
        public List<ECGCase> cases = new List<ECGCase>();

        [SerializeShort]
        public class ECGCase : ISerializable {
            public float weight = 1;
            public ContentInfo info;
            public bool enabled = true;

            public void Serialize(Writer writer) {
                writer.Write("info", info);
                writer.Write("weight", weight);
            }

            public void Deserialize(Reader reader) {
                reader.Read("info", ref info);
                reader.Read("weight", ref weight);
            }
        }

        #endregion

        public Rect ModifySlotRect(Rect slotRect) {
            var size = Slot.Offset;
            var center = position;
            
            switch (orientationSide) {
                case Side.Bottom: slotRect.yMax = slotRect.yMax.ClampMin(center.y + size); break;
                case Side.Top: slotRect.yMin = slotRect.yMin.ClampMax(center.y - size); break;
                case Side.Left: slotRect.xMax = slotRect.xMax.ClampMin(center.x + size); break;
                case Side.Right: slotRect.xMin = slotRect.xMin.ClampMax(center.x - size); break;
            }
            
            return slotRect;
        }
    }
    
    public class ECGCasesVariable : ContentInfoVariable {
        public List<ExtendedChipGenerator.ECGCase> value = new List<ExtendedChipGenerator.ECGCase>();

        public override void Serialize(Writer writer) {
            writer.Write("ECGCases", value.ToArray());
        }

        public override void Deserialize(Reader reader) {
            value.Clear();
            value.AddRange(reader.ReadCollection<ExtendedChipGenerator.ECGCase>("ECGCases"));
        }
    }
}
