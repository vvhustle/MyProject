using System;
using UnityEngine;
using YMatchThree.Seasons;
using Yurowm.Core;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class LevelBackground : LevelContent {
        
        SpaceCamera camera;
        
        LevelMapLocationBody locationBody;
        float offset;
        
        public Info info;
        
        public override Type GetContentBaseType() {
            return typeof(LevelBackground);
        }

        public override Type BodyType => typeof(LevelMapLocationBody);

        public override void OnAddToSpace(Space space) {
            bodyName = info.name;
            base.OnAddToSpace(space);   
            
            locationBody = body as LevelMapLocationBody;
            
            if (!locationBody) return;
            
            body?
                .GetComponentsInChildren<ILevelMapLocationComponent>(true)
                .ForEach(s => s.Initialize(context));
            
            context.Catch<SpaceCamera>(c => {
                camera = c;
                camera.onMove += OnCameraMove;
                camera.onZoom += OnCameraZoom;
                App.onScreenResize += OnCameraZoom;
                OnCameraMove(camera.position);
                OnCameraZoom(camera.viewSizeVertical);
            });
        }

        public override void OnRemoveFromSpace(Space space) {
            App.onScreenResize -= OnCameraZoom;
            
            base.OnRemoveFromSpace(space);
            
            if (!camera) return;
            
            camera.onMove -= OnCameraMove;
            camera.onZoom -= OnCameraZoom;
        }

        void OnCameraMove(Vector2 position) {
            this.position = position - new Vector2(0, offset);
            OnCameraZoom();
        }

        void OnCameraZoom(float _) => OnCameraZoom();
        
        void OnCameraZoom() {
            size = 2f * camera.viewSizeHorizontal / LevelMapLocationBody.Width;
            Crop();
        }

        void Crop() {
            var bottom = position.y + locationBody.bottom * size;
            if (camera.Bottom < bottom)
                position += new Vector2(0, camera.Bottom - bottom);
            
            var top = position.y + locationBody.top * size;
            if (camera.Top > top)
                position += new Vector2(0, camera.Top - top);
            
            ApplyTransform();
        }
    
        public struct Info {
            public string name;
            public float offset;

            public Info(string name, float offset) {
                this.offset = offset;
                this.name = name;
            }
        }
    }
}