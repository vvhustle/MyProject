using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YMatchThree.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;

namespace Yurowm.Dialogues {
    public abstract class Instruction : ISerializable {
            
        public abstract IEnumerator Logic(Dialogue dialogue);

        public override string ToString() {
            return GetShortDetails();
        }

        public abstract string GetShortDetails();
        
        public virtual IEnumerable<string> GetLocalizationKeys() {
            yield break;
        }

        public abstract void Serialize(Writer writer);

        public abstract void Deserialize(Reader reader);
    }
        
    public class TapToContinueInstruction : Instruction {
        public override IEnumerator Logic(Dialogue dialogue) {
            return dialogue.TapToContinue();
        }

        public override string GetShortDetails() {
            return "Tap To Continue";
        }
        
        public override void Serialize(Writer writer) { }

        public override void Deserialize(Reader reader) { }
    }
 
    public abstract class CharacterInstruction : Instruction {
        public Dialogue.Side side = Dialogue.Side.Left;
        
        public override string GetShortDetails() {
            return side.ToString();
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("side", side);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("side", ref side);
        }
    }
        
    public class CharacterShowInstruction : CharacterInstruction {
        public string characterName;

        public override IEnumerator Logic(Dialogue dialogue) {
            return dialogue.Show(side, characterName);
        }

        public override string GetShortDetails() {
            return $"Show {characterName} ({base.GetShortDetails()})";
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer); 
            writer.Write("characterName", characterName);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("characterName", ref characterName);
        }
    }
        
    public class CharacterHideInstruction : CharacterInstruction {
        public override IEnumerator Logic(Dialogue dialogue) {
            return dialogue.Hide(side);
        }
        
        public override string GetShortDetails() {
            return $"Hide ({base.GetShortDetails()})";
        }
    }
        
    public class HideBubbleInstruction : Instruction {
        public override IEnumerator Logic(Dialogue dialogue) {
            return dialogue.HideBubble();
        }
        
        public override string GetShortDetails() {
            return "Hide Bubble";
        }

        public override void Serialize(Writer writer) { }

        public override void Deserialize(Reader reader) { }
    }
        
    public class ClearInstruction : Instruction {
        public override IEnumerator Logic(Dialogue dialogue) {
            yield return dialogue.HideBubble();
            yield return Enum.GetValues(typeof(Dialogue.Side))
                .Cast<Dialogue.Side>()
                .Select(dialogue.Hide)
                .Parallel();
        }
        
        public override string GetShortDetails() {
            return "Clear";
        }

        public override void Serialize(Writer writer) { }

        public override void Deserialize(Reader reader) { }
    }
    
    public class SayInstruction : CharacterInstruction {
        public string text = "";
        
        public bool localized = false;
        
        public string localizationKey = "";
        
        public override IEnumerator Logic(Dialogue dialogue) {
            string text;
            
            if (localized) {
                var level = dialogue.context.GetArgument<LevelScriptBase>();
                text = Localization.content[level.GetFullLocalizationKey(localizationKey)];
            } else
                text = this.text;
            
            yield return dialogue.Say(side, text);
        }

        public override string GetShortDetails() {
            var text = localized ? localizationKey.Brackets(Bracket.Curly) : this.text;
            return $"Say {base.GetShortDetails()}: {text.Ellipsis(12)}";
        }

        public override IEnumerable<string> GetLocalizationKeys() {
            if (localized) 
                yield return localizationKey;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);

            writer.Write("localized", localized);
            
            if (localized)
                writer.Write("localizationKey", localizationKey);
            else
                writer.Write("text", text);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);

            reader.Read("localized", ref localized);
            if (localized)
                reader.Read("localizationKey", ref localizationKey);
            else
                reader.Read("text", ref text);
        }
        
        
    }
}