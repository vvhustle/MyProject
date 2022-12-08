using System;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Jobs;
using Yurowm.Serialization;

namespace Yurowm.Spaces {

    public abstract class SpacePhysicalItemBase : GameEntity, IBody {
        protected object locker = new object();
        
        float _size = 1f;
        public virtual float size {
            get {
                lock (locker)
                    return _size;
            }
            set {
                lock (locker)
                    _size = value;
                if (body)
                    body.transform.localScale = Vector3.one * _size;
            }
        }
        
        public string bodyName {get; set;}
        
        public virtual Type BodyType => typeof(SpaceObject);
        
        public void SetBody(SpaceObject body) {
            this.body = body;
            if (body) 
                onSetBody.Invoke(body);
            OnSetBody();
            SendOnSetBody();
        }
        
        public static SPI New<SPI>(string bodyName) where SPI : SpacePhysicalItemBase {
            var result = Activator.CreateInstance<SPI>();
            result.bodyName = bodyName;
            return result;
        }
        
        public override void OnEnable() {
            base.OnEnable();
            body?.gameObject.SetActive(true);
            if (this is IVisibilitySpecified ivs) {
                if (ivs.isVisible)
                    ivs.OnVisible();
                else
                    ivs.OnInvisible();
            }
        }

        public override void OnDisable() {
            base.OnDisable();
            body?.gameObject.SetActive(false);
        }

        #region ISpaceEntity
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            if (!bodyName.IsNullOrEmpty()) {
                SetBody(EmitBody());
            }
            space.Subscribe(this);
        }

        public override void OnRemoveFromSpace(Space space) {
            onRemoveFromSpace();
            base.OnRemoveFromSpace(space);
            if (body) body.Kill();
        }
        #endregion

        public SpaceObject body;
        
        protected virtual void OnSetBody() { }
        
        #if PHYSICS_2D
        
        Collider2D _collider2D;
        public Collider2D collider2D {
            get {
                if (_collider2D || (body && body.SetupComponent(out _collider2D)))
                    return _collider2D;
                return null;
            }
        }
        
        Rigidbody2D _rigidbody2D;
        public Rigidbody2D rigidbody2D {
            get {
                if (_rigidbody2D || (body && body.SetupComponent(out _rigidbody2D)))
                    return _rigidbody2D;
                return null;
            }
        }
        
        #endif

        public SingleCallEvent<SpaceObject> onSetBody = new SingleCallEvent<SpaceObject>();
        public virtual SpaceObject EmitBody() {
            var result = AssetManager.Emit<SpaceObject>(bodyName, context);
            result.transform.SetParent(space.root);            
            ApplyTransform(result.transform);
            result.item = this;
            return result;
        }
        
        protected virtual void ApplyTransform(Transform transform) {
            transform.localScale = Vector3.one * size;
        }

        public void ApplyTransform() {
            ApplyTransform(body.transform);
        } 

        public void SendOnSetBody() {
            body?.GetComponentsInChildren<IOnSetBodyHandler>()
                .ForEach(h => h.OnSetBody(this));
        }
        
        public override void OnKill() {
            if (body) body.Destroying();
            body = null;
            base.OnKill();
        }

        #region ISerializable
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            if (!bodyName.IsNullOrEmpty()) writer.Write("body", bodyName);
            
            if (size != 1) writer.Write("size", size);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            bodyName = reader.Read<string>("body");
            if (!reader.Read("size", ref _size))
                _size = 1;
        }
        #endregion

        #region IVisibilitySpecified
        public bool isVisible {get; private set;} = true;

        public virtual float GetVisibleSize() {
            return size;
        }

        public virtual void OnVisible() {
            isVisible = true;
        }

        public virtual void OnInvisible() {
            isVisible = false;
        }
        #endregion

    }
    
    public class SpacePhysicalItem : SpacePhysicalItemBase {

        #region Dimensions
        Vector2 _position = new Vector2();
        public Action<SpacePhysicalItem> onChangePosition = null;
        public virtual Vector2 position {
            get {
                lock (locker)
                    return _position;
            } set {
                lock (locker) {
                    if (_position == value) return;
                    _position = value;
                    onChangePosition?.Invoke(this);
                }
            }
        }

        Vector2 _velocity = new Vector2();
        public virtual Vector2 velocity {
            get {
                lock (locker)
                    return _velocity;
            }
            set {
                lock (locker) 
                    _velocity = value;
            }
        }

        float _direction = 0;
        public virtual float direction {
            get {
                lock (locker)
                    return _direction;
            }
            set {
                lock (locker)
                    _direction = value;
            }
        }
        #endregion

        protected override void ApplyTransform(Transform transform) {
            base.ApplyTransform(transform);
            transform.position = position;
            transform.rotation = Quaternion.Euler(0, 0, direction);
        } 
        
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            if (!position.IsEmpty()) writer.Write("position", position);
            
            if (direction != 0) writer.Write("direction", direction);
            
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("position", ref _position);
            reader.Read("direction", ref _direction);
        }
    }
    
    public class SpacePhysicalItem3D : SpacePhysicalItemBase {

        #region Dimensions
        Vector3 _position = new Vector3();
        public Action<SpacePhysicalItem3D> onChangePosition = null;
        public virtual Vector3 position {
            get {
                lock (locker)
                    return _position;
            } set {
                lock (locker) {
                    _position.x = value.x;
                    _position.y = value.y;
                    _position.z = value.z;
                    onChangePosition?.Invoke(this);
                }
            }
        }

        Vector3 _velocity = new Vector3();
        public virtual Vector3 velocity {
            get {
                lock (locker)
                    return _velocity;
            }
            set {
                lock (locker) 
                    _velocity = value;
            }
        }

        Quaternion _rotation = Quaternion.identity;
        public virtual Quaternion rotation {
            get {
                lock (locker)
                    return _rotation;
            }
            set {
                lock (locker)
                    _rotation = value;
            }
        }
        #endregion

        protected override void ApplyTransform(Transform transform) {
            base.ApplyTransform(transform);
            transform.position = position;
            transform.rotation = rotation;
        } 
        
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            if (!position.IsEmpty()) writer.Write("position", position);
            
            if (rotation != Quaternion.identity) writer.Write("rotation", rotation);
            
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("position", ref _position);
            reader.Read("rotation", ref _rotation);
        }
    }
    
    public interface IOnSetBodyHandler {
        void OnSetBody(GameEntity entity);
    }

    public class SpacePhysicalItemLinked : SpacePhysicalItem, ISelfUpdate {
        SpacePhysicalItem link = null;

        public void Link(SpacePhysicalItem link) {
            this.link = link;
        }

        public virtual void UpdateFrame(Updater updater) {
            if (link) {
                link.SureToUpdate(updater);
                position = link.position;
                velocity = link.velocity;
            }
        }
    }
}
