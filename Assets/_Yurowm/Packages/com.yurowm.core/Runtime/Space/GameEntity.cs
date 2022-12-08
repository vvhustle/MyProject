using System;
using System.Collections.Generic;
using Yurowm.ContentManager;
using Yurowm.Controls;
using Yurowm.Extensions;
using Yurowm.Jobs;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.UI;
using Yurowm.Utilities;
using IUIRefresh = Yurowm.UI.IUIRefresh;
using Space = Yurowm.Spaces.Space;

namespace Yurowm {
    public abstract class GameEntity : ILiveContexted, IStorageElementExtraData {
        public string name;
        YRandom _random;
        public YRandom random {
            get => _random ??= YRandom.main.NewRandom();
            set => _random = value;
        }

        public Space space = null;
        public Action<GameEntity> onAddChild = delegate { };
        public Action<GameEntity> onRemoveChild = delegate { };

        #region IStorageElementExtraData

        public StorageElementFlags storageElementFlags { get; set; }
        
        public bool IsDefault => storageElementFlags.HasFlag(StorageElementFlags.DefaultElement);
        
        
        #endregion
        
        
        public SpaceTime time => space?.time;
        
        public static GE New<GE>() where GE : GameEntity {
            return Activator.CreateInstance<GE>();
        }

        #region Modules

        public List<Module> modules = new List<Module>();
        
        public M AddModule<M>() where M : Module {
            var result = Module.Emit<M>(this); 
            modules.Add(result);
            return result;
        }
        
        public M GetModule<M>() where M : Module {
            return modules.CastOne<M>();
        }
        
        public bool SetupMobule<M>(out M result) where M : Module {
            result = modules.CastOne<M>(); 
            return result != null;
        }
        
        #endregion

        #region Enable / Disable
        bool _enabled;
        public bool enabled {
            get => _enabled;
            set {
                if (_enabled == value) return;
                _enabled = value;
                if (_enabled) OnEnable();
                else OnDisable();
            }
        }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        #endregion

        public LiveContext context { get; set; }

        #region Initialization
        public GameEntity() {}

        public virtual void Create() {
            Complete();
        }

        public virtual void Complete() {}

        public virtual void Initialize() {}
        
        #endregion

        #region ISerializable
        
        public virtual void Serialize(Writer writer) {
            if (_random != null)
                writer.Write("random", _random);
            if (!name.IsNullOrEmpty())
                writer.Write("name", name);
            if (this is ISerializableID sid)
                writer.Write("ID", sid.ID);
            writer.Write("storageElementFlags", storageElementFlags);
        }

        public virtual void Deserialize(Reader reader) {
            reader.Read("random", ref _random);
            reader.Read("name", ref name);
            if (this is ISerializableID sid)
                sid.ID = reader.Read<string>("ID");
            storageElementFlags = reader.Read<StorageElementFlags>("storageElementFlags");
        }
        
        #endregion

        #region ILiveContexted
        GameEntity _original = null;
        public GameEntity original => _original;

        bool isKilled = false;
        public Action onKill = delegate { };
        public Action onRemoveFromSpace = delegate { };
        public void Kill() {
            if (isKilled) return;
            if (!space) space = context?.GetArgument<Space>();
            if (space) OnRemoveFromSpace(space);
            context?.Remove(this);
            onKill();
            OnKill();
            isKilled = true;
        }

        public virtual void OnKill() {
        }

        public bool IsAlive() {
            return !isKilled;
        }
        
        public bool EqualContent(ILiveContexted obj) {
            if (obj is GameEntity entity) {
                if (!entity) return false;
                if (entity == this) return true;
                if (entity._original && _original) return entity._original == _original;
                return entity._original == this || _original == entity;
            }
            return false;
        }
        #endregion

        #region ISelfUpdate
        public int updateID { get; set; }

        public bool readyForUpdate {
            get => enabled;
            set => enabled = value;
        }

        public void SureToUpdate(Updater updater) {
            if (this is ISelfUpdate update && updateID != updater.frameID) {
                update.UpdateFrame(updater);
                update.updateID = updater.frameID;
            }
        }

        public void MakeUnupdated() {
            if (this is ISelfUpdate update)
                update.updateID = int.MinValue;
        }

        #endregion

        #region GameEntity
        public virtual void OnAddToSpace(Space space) {
            if (this is ISelfUpdate)
                space.Subscribe<SelfUpdateJob>(this);
            if (this is ICastable castable)
                space.clickables.Set(castable);
            if (this is IUIRefresh refresh)
                UIRefresh.Add(refresh);
        }

        public virtual void OnRemoveFromSpace(Space space) {
            enabled = false;
            space.Unsubscribe(this);
            if (this is ICastable castable)
                space.clickables.Remove(castable);
            if (this is IUIRefresh refresh)
                UIRefresh.Remove(refresh);
        }
        #endregion

        public static implicit operator bool(GameEntity entity) {
            return entity != null && !entity.isKilled;
        }

        public static bool operator ==(GameEntity a, GameEntity b) {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);
            return a.Equals(b);
        }

        public static bool operator !=(GameEntity a, GameEntity b) {
            return !(a == b);
        }

        public override string ToString() {
            string result = "";
            if (!name.IsNullOrEmpty())
                result += name + " ";
            
            result += $"({GetType().Name})";
            
            if (random != null)
                result += $" r{random.seed}";
            
            return result;
        }
    } 
}