using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YMatchThree.Core;
using Yurowm.ContentManager;

namespace Yurowm.Effects {
    public class AnimationEvent : BaseBehaviour {
        public UnityEngine.UI.Button.ButtonClickedEvent[] emit;
        
        public void EmitAnimationEvent(int index) {
            if (index < 0 || index >= emit.Length) return;
            emit[index].Invoke();
        }

        public void Vibrate(float power) {
            LevelContent.VibrateWithPower(power);
        }
    }
}