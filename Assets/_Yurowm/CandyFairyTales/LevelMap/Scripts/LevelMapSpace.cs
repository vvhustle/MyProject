using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Seasons;
using Yurowm.Controls;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.UI;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class LevelMapSpace : Space {
        SpaceCamera camera;
        
        public static string nextLevelMapID;
        
        public LevelMap levelMap;
        
        public override IEnumerator Complete() {
            camera = new SpaceCamera();
            AddItem(camera);
            
            yield return null;
            
            levelMap = LevelMap.storage
                .FirstOrDefaultFiltered(m => m.ID == nextLevelMapID, m => true)
                .Clone();
            
            AddItem(levelMap);
            
            var op = new CameraOperator {
                allowToMove = true,
                allowToRotate = false,
                allowToZoom = false
            };
            
            op.limiter = new LevelMap.CameraLimiter(levelMap);

            AddItem(op);
            
            levelMap.ShowCurrentLevel();
        }

        IEnumerator _CenterSeason = null;
        
        IEnumerator CenterSeason(float inertia) {
            _CenterSeason = null;

            yield break;
        }
    }
}