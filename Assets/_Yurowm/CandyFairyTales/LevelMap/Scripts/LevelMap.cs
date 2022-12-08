using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using YMatchThree.Meta;
using Yurowm;
using Yurowm.Colors;
using Yurowm.ContentManager;
using Yurowm.Core;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Shapes;
using Yurowm.Spaces;
using Yurowm.UI;
using Yurowm.Utilities;
using Behaviour = UnityEngine.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Seasons {
    public class LevelMap : GameEntity, ISerializableID, IUIRefresh {
        [PreloadStorage]
        public static Storage<LevelMap> storage = new Storage<LevelMap>("LevelMaps", TextCatalog.StreamingAssets);

        public string ID { get; set; }
        
        public string worldName;
        
        public string levelButtonName;
        public string levelLineName;
        
        public Color fieldColor = Color.white;
        
        LevelWorld _world;
        public LevelWorld world {
            get {
                if (_world == null)
                    _world = LevelWorld.all.FirstOrDefault(w => w.name == worldName);
                
                return _world;
            }
        }
        
        public List<string> locations = new List<string>();
        List<LevelMapLocationBody> locationPrefabs;
        LocationInfo[] locationInfos;

        LevelMapPointsProvider pointsProvider;
        
        SpaceCamera camera;
        CameraOperator cameraOperator;
        
        YLine2D line;
        
        float currentLevelPosition = 0;
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            context.Catch<SpaceCamera>(c => {
                camera = c;
                camera.onMove += OnMove;
            });
            
            context.Catch<CameraOperator>(c => 
                cameraOperator = c);
            
            var worldProgress = PlayerData.levelProgress.GetWorldPropgress(worldName, true);

            var currentLevel = world.levels
                .Where(l => !worldProgress.Complete(l.ID))
                .GetMin(l => l.order)?.order ?? 1;
            
            #region Build Location Infos

            {
                var topPosition = 0f;
                var locationIndex = 0;
                var levelsCount = 0;
                
                locationInfos = locations
                    .Select(locationName => {
                        var info = new LocationInfo(this);
                        info.prefab = AssetManager.GetPrefab<LevelMapLocationBody>(locationName);
                        info.index = locationIndex ++;
                        info.firstLevel = levelsCount;
                        info.bottom = topPosition;
                        info.top = info.bottom + info.prefab.Length;
                        info.bottomVisible = info.bottom - info.prefab.visibilityOffsetBottom.ClampMin(0);
                        info.topVisible = info.top + info.prefab.visibilityOffsetTop.ClampMin(0);
                        
                        levelsCount += info.prefab.pointsProvider.points.Length;
                        
                        if (currentLevel >= info.firstLevel && currentLevel <= levelsCount)
                            currentLevelPosition = info.prefab.pointsProvider.points[currentLevel - info.firstLevel].y +
                                topPosition - info.bottom;

                        topPosition = info.top;
                        
                        return info;
                    })
                    .ToArray();
            }
            
            #endregion
            
            locationPrefabs = locations
                .Select(AssetManager.GetPrefab<LevelMapLocationBody>)
                .NotNull()
                .ToList();
            
            line = AssetManager.Create<YLine2D>(levelLineName);
            line?.transform.SetParent(space.root);
            
            
            App.onScreenResize += OnScreenResize;
            
            OnMove();
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            if (line)
                Object.Destroy(line.gameObject);
            App.onScreenResize -= OnScreenResize;
        }

        void OnScreenResize() {
            if (!enabled || !cameraOperator) return;
            
            cameraOperator.Crop();
            cameraOperator.limiter.SetupCamera(camera);
            
            OnMove();
        }
        
        public void ShowCurrentLevel() {
            cameraOperator.position = new Vector2(0, currentLevelPosition);
            cameraOperator.Crop();
            
            OnMove();
        }
        
        #region Locations
        
        void OnMove(Vector2 _ = default) {
            locationInfos.ForEach(i => i.OnCameraMove(camera));
        }
        
        void FillButtons(LocationInfo info) {
            if (world == null || !info.location) return;
            
            var location = info.location;
            
            if (!location.pointsProvider) return;
            
            var firstIndex = info.firstLevel;
            var lastIndex = firstIndex + location.pointsProvider.points.Length;
                
            lastIndex = YMath.Min(world.levels.Count, lastIndex);
            
            for (int i = firstIndex; i < lastIndex; i++) {
                var button = SpacePhysicalItemBase.New<LevelButton>(levelButtonName);
                if (!button) break;
                
                var point = location.pointsProvider.points[i - firstIndex];
                
                button.level = world.levels[i];
                button.level.background = new LevelBackground.Info(info.location.body.name, point.y);
                location.buttons.Add(button);
                space.AddItem(button);
                button.position = location.position + point;
            }

            var worldProgress = PlayerData.levelProgress.GetWorldPropgress(worldName, true);
            
            var currentLevelOrder = 1;
            
            if (worldProgress != null) {
                var currentLevel = GetCurrentLevel();
                
                if (currentLevel != null)
                    currentLevelOrder = currentLevel.order;
                else
                    currentLevelOrder = world.levels.Max(l => l.order) + 1;
            }
            
            location.buttons
                .ForEach(lb => {
                    if (lb.level.order == currentLevelOrder)
                        lb.UnlockCurrent();
                    else if (lb.level.order <= currentLevelOrder) {
                        lb.UnlockFast();
                        var level = lb.level;
                        var score = worldProgress.GetBestScore(level.ID);
                        var stars = level.stars.GetCount(score);
                        lb.SetStars(stars);  
                    } else
                        lb.Lock();
                });
        }

        void BuildLine() {
            line.Clear();
            locationInfos
                .Where(l => l.visible)
                .SelectMany(l => l.location.buttons)
                .ForEach(b => line.AddPoint(b.position));
        }
        
        #endregion
        
        LevelScriptOrdered GetCurrentLevel() {
            var worldProgress = PlayerData.levelProgress.GetWorldPropgress(worldName, true);
                
            return world.levels
                .Where(l => !worldProgress.Complete(l.ID))
                .GetMin(l => l.order);
        }

        public override void OnEnable() {
            base.OnEnable();
            
            var worldProgress = PlayerData.levelProgress.GetWorldPropgress(worldName, true);
            
            var currentLevelOrder = 1;
            
            if (worldProgress != null) {
                var currentLevel = GetCurrentLevel();
                
                if (currentLevel != null)
                    currentLevelOrder = currentLevel.order;
                else
                    currentLevelOrder = world.levels.Max(l => l.order) + 1;
            }

            locationInfos
                .Where(i => i.visible)
                .SelectMany(i => i.location.buttons)
                .ForEach(lb => {
                    if (lb.level.order == currentLevelOrder)
                        lb.UnlockCurrent();
                    else if (lb.level.order <= currentLevelOrder) {
                        lb.UnlockFast();
                        var level = lb.level;
                        var score = worldProgress.GetBestScore(level.ID);
                        var stars = level.stars.GetCount(score);
                        lb.SetStars(stars);  
                    } else
                        lb.Lock();
                });
            
            OnScreenResize();
        }

        public void SetupField(Space _) {
            ApplyAccent();
        }

        public void ApplyAccent() {
            FillColorScheme(UIColorScheme.current);
            UIRefresh.Invoke();
        }

        public void FillColorScheme(UIColorScheme scheme) {
            scheme.SetColor("Main.Field", fieldColor);
        }

        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("worldName", worldName);
            writer.Write("levelButton", levelButtonName);
            writer.Write("levelLine", levelLineName);
            writer.Write("locations", locations.ToArray());
            writer.Write("fieldColor", fieldColor);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("worldName", ref worldName);
            reader.Read("levelButton", ref levelButtonName);
            reader.Read("levelLine", ref levelLineName);
            locations = reader.ReadCollection<string>("locations").ToList();
            reader.Read("fieldColor", ref fieldColor);
        }

        #endregion
        
        public class CameraLimiter : CameraOperatorLimiter {
            Rect edges;

            public CameraLimiter(LevelMap map) {
                edges = new Rect(
                    0, map.locationPrefabs[0].Edges.Min,
                    0, map.locationPrefabs.Sum(p => p.Length));
            }
            
            public override bool CropPosition(Vector2 position, out Vector2 cropped, Vector2 viewSize, float allowedOffset = 0) {
                cropped = position;
                
                var e = edges.GrowSize(0, -camera.viewSizeVertical * 2);
                
                if (e.Contains(cropped))
                    return false;
                
                cropped.x = cropped.x.Clamp(e.xMin, e.xMax);
                cropped.y = cropped.y.Clamp(e.yMin, e.yMax);
                return true;
            }

            public override Vector2 GetOffset(Vector2 position, Vector2 viewSize) {
                return default;
            }

            public override float CropZoom(float zoom) {
                return zoom;
            }

            public override float GetZoomTarget(float zoom) {
                return zoom;
            }

            public override void SetupCamera(SpaceCamera camera) {
                camera.viewSizeHorizontal = LevelMapLocationBody.Width / 2;
            }
        }
        
        class LocationInfo {
            LevelMap map;
            
            public LevelMapLocationBody prefab;
            public LevelMapLocation location;
            
            public int index;
            public int firstLevel;
            
            public float bottom;
            public float top;
            
            public float bottomVisible;
            public float topVisible;
            
            public bool visible {get; private set;}

            public LocationInfo(LevelMap map) {
                this.map = map;
            }

            bool IsVisible(SpaceCamera camera) {
                visible = topVisible >= camera.Bottom && bottomVisible <= camera.Top;
                return visible;
            }
            
            public void OnCameraMove(SpaceCamera camera) {
                var visible = IsVisible(camera);
                
                if (!visible && location) Remove();

                if (visible && !location) Create();
            }

            void Remove() {
                location.Kill();
                location = null;
            }

            void Create() {
                location = SpacePhysicalItemBase.New<LevelMapLocation>(prefab.name);
            
                if (!location) return;
            
                map.space.AddItem(location);
                
                location.position = new Vector2(0, 
                    bottom - location.BottomEdge);
                
                map.FillButtons(this);
                map.BuildLine();
            }
        }

        public bool visible => true;
        
        public void Refresh() {
            OnEnable();    
        }
    }
}