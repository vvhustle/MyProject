using System;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.UI;
using Yurowm.Utilities;

namespace Yurowm.Features {
    public class LifeSystemBar : Behaviour, IUIRefresh {
        
        public AnimationSampler originalLifeIcon;
        public GameObject infiniteLifeIcon;
        public LabelFormat progressLabel;
        public LabelFormat infiniteLabel;
        public LabelFormat fullLabel;
        
        public int maxIconCount = 8;
        public LabelFormat iconExtra;
        
        List<AnimationSampler> icons = new List<AnimationSampler>();

        Transform lifeContainer;
        
        LifeSystem lifeSystem;
        
        public override void Initialize() {
            base.Initialize();
            
            lifeSystem = Integration.Get<LifeSystem>();
            
            if (lifeSystem == null) {
                gameObject.SetActive(false);
                return;
            }
            
            if (originalLifeIcon) {
                icons.Add(originalLifeIcon);
                lifeContainer = originalLifeIcon.transform.parent;

                while (icons.Count < lifeSystem.lifeCount.ClampMax(maxIconCount)) {
                    var newIcon = Instantiate(originalLifeIcon.gameObject).GetComponent<AnimationSampler>();
                    newIcon.transform.SetParent(lifeContainer);
                    newIcon.transform.Reset();
                    icons.Add(newIcon);
                }
            }
        }

        DelayedAccess updateAccess = new DelayedAccess(1f);
        
        void Update() {
            if (updateAccess.GetAccess())
                Refresh();
        }

        public void Refresh() {
            if (lifeSystem == null) return;
            
            var infiniteLife = lifeSystem.HasInfiniteLife();
            
            infiniteLifeIcon?.SetActive(infiniteLife);
            
            for (int i = 0; i < lifeSystem.lifeCount.ClampMax(maxIconCount); i++) {
                var icon = icons[i]; 
                icon.gameObject.SetActive(!infiniteLife);
                if (!infiniteLife) {
                    if (i < lifeSystem.data.count) icon.Time = 1;
                    else if (i > lifeSystem.data.count) icon.Time = 0;
                    else icon.Time = lifeSystem.GetRecoveringProgress();
                }
            }
            
            if (iconExtra) {
                if (!infiniteLife && maxIconCount < lifeSystem.data.count) {
                    iconExtra.gameObject.SetActive(true);
                    iconExtra.transform.SetAsLastSibling();
                    iconExtra["value"] = (lifeSystem.data.count - maxIconCount).ToString();
                } else 
                    iconExtra.gameObject.SetActive(false);
            }
            
            infiniteLabel?.gameObject.SetActive(false);
            progressLabel?.gameObject.SetActive(false);
            fullLabel?.gameObject.SetActive(false);

            if (infiniteLife) {
                if (infiniteLabel) {
                    infiniteLabel.gameObject.SetActive(true);
                    infiniteLabel["time"] = LifeSystem.TimeSpanToString(lifeSystem.GetInfiniteLifeTime());
                }
            } else if (lifeSystem.IsRecovering) {
                if (progressLabel) {
                    progressLabel.gameObject.SetActive(true);
                    progressLabel["time"] = LifeSystem.TimeSpanToString(lifeSystem.GetRecoveringTime());
                }
            } else {
                fullLabel?.gameObject.SetActive(true);
            }
        }
    }
}