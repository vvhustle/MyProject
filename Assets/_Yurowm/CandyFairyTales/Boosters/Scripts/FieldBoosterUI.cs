using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.UI;
using Behaviour = Yurowm.Behaviour;

namespace YMatchThree.Core {
	[RequireComponent(typeof(ContentAnimator))]
	public class FieldBoosterUI : Behaviour {
		public TMP_Text desctiption;

		ContentAnimator animator;
		
		const string hideClip = "Hide";
		const string showClip = "Show";
		
		FieldBooster booster;
		
		public override void Initialize() {
			base.Initialize();
			this.SetupComponent(out animator);
		}

		void OnEnable() {
			if (!animator) return;
			
			if (booster == null)
				animator.RewindEnd(hideClip);
			else
				animator.RewindEnd(showClip);
		}
		
		public void Setup(FieldBooster booster) {
			Showing(booster).Run();
		}
		
		bool busy = false;
		
		IEnumerator Showing(FieldBooster booster) {
			if (!animator || busy || !enabled)
				yield break;
			
			busy = true;

			using (Page.NewActiveAnimation()) {
					
				if (this.booster != null) {
					this.booster = null;
					yield return animator.PlayAndWait(hideClip);
				}

				if (booster != null) {
					desctiption.text = booster.descriptionText.GetText();
					
					this.booster = booster;
					
					yield return animator.PlayAndWait(showClip);
				}
				
			}
			
			busy = false;
		}
		
		void Cancel() {
			booster.Deactivate();
		}
	}
}