using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public class ReactionTask : LevelGameplay.InternalTask {
        
        public override IEnumerator Logic() {
            var reactions = Reactions.Get(field);
            
            while (reactions.reactors.Count > 0) {
                var result = Reactor.Result.CompleteToContinue;
                var reactor = reactions.reactors.First();
                
                while (reactor.logic.MoveNext()) {
                    var current = reactor.logic.Current;
                        
                    if (current is Reactor.Result r)
                        result = r;
                        
                    yield return current;
                }
                
                switch (result) {
                    case Reactor.Result.CompleteToContinue: 
                        reactions.reactors.RemoveAt(0); continue;
                    case Reactor.Result.CompleteToGravity: 
                        reactions.reactors.RemoveAt(0); break;
                    case Reactor.Result.GravityToRepeate: 
                        reactor.Reset(); break;
                }
                
                gameplay.NextTask<GravityTask>();
                yield break;
            }
            
            gameplay.NewTask<WaitTask>();
        }
    }
    
    public class Reactions {
        
        public List<Reactor> reactors = new List<Reactor>();
        
        Field field;
        
        Reactions() {}
        
        public static Reactions Get(Field field) {
            var result = field.fieldContext.GetArgument<Reactions>();
            
            if (result == null) {
                result = new Reactions();
                result.field = field;
                field.fieldContext.SetArgument(result);
            }
            
            return result;
        }

        public void Fill(Reaction.Type type) {
            foreach (var reaction in field.fieldContext.GetAll<Reaction>()) {
                if (!reaction.GetReactionType().OverlapFlag(type)) continue;
                if (reactors.Any(r => r.reaction == reaction)) continue;
                reactors.Add(new Reactor(reaction.React, reaction));
            }
            reactors.Sort(r => r.reaction.GetPriority());
        }

        public T Emit<T>() where T : Reaction {
            T reaction = field.fieldContext.Get<T>();
            
            if (field.fieldContext.Get<T>() == null) {
                try {
                    reaction = Activator.CreateInstance<T>(); 
                    field.AddContent(reaction);
                } catch (Exception e) {
                    Debug.LogException(e); 
                    return null;
                }
            } 
            
            return reaction;
        }
    }
}