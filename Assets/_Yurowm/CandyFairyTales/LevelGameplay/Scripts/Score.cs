using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm;
using Yurowm.Serialization;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class Score : GameEntity {

        public int value {
            get;
            private set;
        }

        public LevelStars stars;
        LevelScriptEvents events;

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            events = context.GetArgument<LevelScriptEvents>();
            events.onAddScore.Invoke(0);
        }

        public bool StarIsReached(int starNumber) {
            switch (starNumber) {
                case 1: return value >= stars.First;
                case 2: return value >= stars.Second;
                case 3: return value >= stars.Third;
                default: return false;
            }
        }

        public int StarCount() {
            if (value >= stars.Third) return 3;
            if (value >= stars.Second) return 2;
            if (value >= stars.First) return 1;
            return 0;
        }

        public void AddScore(int points) {
            if (points <= 0) return;
            
            int stars = StarCount();
            value += points;
            events.onAddScore(points);
            
            stars = StarCount() - stars;
            
            if (stars == 0) return;

            for (int s = 0; s < stars; s++)
                events.onReachedTheStar();
        }
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("score", value);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            value = reader.Read<int>("score");
        }
    }
}