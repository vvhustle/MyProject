using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Simulator
{
    public class UIContents
    {
        public Transform transform;
        public Battle battle;
        public GameData gameData;
        public Simulator simul;

        public GamePanel gamePanel;
        public FixedPanel fixedPanel;

        public bool isClickAutoBtn = false; // 자동 전투 토글
        public bool isClickShowAreaBtn = false; // 위험 지역 토글
        public bool isOpenPopup = false; // 팝업 토글

        public UIContents()
        {
        }

        public UIContents(Transform mainTransform, Battle mainBattle, GameData mainData)
        {
            transform = mainTransform;
            battle = mainBattle;
            gameData = mainData;

            fixedPanel = new FixedPanel();
            fixedPanel.Init(transform.Find("FixedPanel"), battle, gameData);

            gamePanel = new GamePanel();
            gamePanel.Init(transform.Find("GamePanel"), battle, gameData);
        }

    }
}
