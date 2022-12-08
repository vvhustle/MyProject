using System.Collections;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class MatchThreeSwapHelpNode : ActionNode {

        public string layerID;
        public string handName;

        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            if (!field) yield break;

            var level = field.fieldContext.GetArgument<Level>();
            
            var layer = level.layers.FirstOrDefault(l => l.ID == layerID);
            
            field.interactionLayer = layer; 
            
            if (layer != null) {
                
                if (field.gameplay is MatchThreeGameplay g) {
                    var move = g.FindMoves().FirstOrDefault();
                    
                    if (move != null)
                        using (var task = field.gameplay.NewExternalTask()) {
                            yield return task.WaitAccess();
                            
                            var moveControl = g.FindMoves().FirstOrDefault();
                            
                            if (moveControl != null && move.A == moveControl.A && move.B == moveControl.B) {
                                field.gameplay.hintLockers.Add(this);
                                
                                move.solution.contents.ForEach(c => c.Flashing());
                                
                                var hand = AssetManager.Create<HelperHand>(handName);
                                if (hand) {
                                    hand.transform.SetParent(field.root);
                                    hand.transform.Reset();
                                    hand.Animate(move, field);
                                }
                            }
                        }
                    
                    if (move != null)
                        yield return field.gameplay.WaitForTask<MatchingTask>();
                }
                
                
                field.interactionLayer = null;
                
            }

            field.gameplay.hintLockers.Remove(this);
            
            Push(outputPort, field);
        }
        
        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("handName", handName);
            writer.Write("layerID", layerID);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("handName", ref handName);
            reader.Read("layerID", ref layerID);
        }

        #endregion
    }
}