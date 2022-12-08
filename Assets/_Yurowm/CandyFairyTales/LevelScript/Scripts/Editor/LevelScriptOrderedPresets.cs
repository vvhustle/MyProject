using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Nodes;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Editor {

    public class LevelScriptOrderedPresetEmpty : ILevelScriptPreset {
        public virtual string GetName() => "Empty";

        public LevelScriptOrdered Emit(LevelEditorContext context) {
            var result = new LevelScriptOrdered();
            
            result.colorSettings = (LevelColorSettings) Activator.CreateInstance(context.colorSettingsTypes
                .FirstOrDefaultFiltered(
                    t => typeof(IDefault).IsAssignableFrom(t),
                    t => true));
            result.colorSettings.colorPalette = LevelContent.storage.GetDefault<ItemColorPalette>();
            
            AssetManager.Instance.Initialize();

            Fill(context, result);
            
            result.name = GetName();
            
            return result;           
        }

        protected virtual void Fill(LevelEditorContext context, LevelScriptOrdered script) {
            
        }
    }
    
    public class LevelScriptOrderedPresetDefault : LevelScriptOrderedPresetEmpty {
        public override string GetName() => "Default";

        protected override void Fill(LevelEditorContext context, LevelScriptOrdered script) {
            base.Fill(context, script);
            
            var startNode = new LevelStartNode {
                ID = 0
            };

            var buildNode = new BuildLevelNode {
                level = new Level(), 
                ID = 1,
                position = new Vector2(0, 100)
            };

            SetupLevel(buildNode.level);

            var completeNode = new LevelCompleteNode {
                ID = 2,
                position = new Vector2(0, 240)
            };
            
            script.extensions = LevelContent.storage
                .GetAllDefault<LevelScriptExtension>()
                .Select(l => new ContentInfo(l))
                .ToList();
            
            script.nodes.Add(startNode);
            script.nodes.Add(buildNode);
            script.nodes.Add(completeNode);
            
            script.nodes.ForEach(n => n.OnCreate());
            
            script.connections.Add( new Pair<Port>(
                startNode.outputPort,
                buildNode.triggerPort));
            
            script.connections.Add( new Pair<Port>(
                buildNode.outputPort,
                completeNode.triggerPort));
        }
        
        public static void SetupLevel(Level level) {
            if (level == null)
                return;

            level.gamePlay = LevelContent.storage.GetDefault<LevelGameplay>()?.ID;
            level.physic = LevelContent.storage.GetDefault<ChipPhysic>()?.ID;
            
            level.extensions = LevelContent.storage
                .GetAllDefault<LevelExtension>()
                .Select(l => new ContentInfo(l))
                .ToList();

            level.randomSeed = 0;
            
            level.layers = new List<SlotLayerBase> {
                new AllSlotsLayer {
                    ID = "Default",
                    isDefault = true
                }
            };
            
            level.slots.Clear();
            for (int x = 0; x < level.width; x++)
            for (int y = 0; y < level.height; y++)
                level.slots.Add(new SlotInfo(new int2(x,y)));
            
            foreach (var content in LevelContent.storage.GetAllDefault<SlotContent>())
                if (content is IDefaultSlotContent dsc)
                    level.slots    
                        .Where(slot => dsc.IsSuitableForNewSlot(level, slot))
                        .ForEach(s => s.AddContent(new ContentInfo(content)));
        }
    }

}