using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyFirstARGame
{
    public class Scoreboard : MonoBehaviour
    {
        public int playerInitialBullet = 10;
        private Dictionary<string, int> scores;
        private Dictionary<string, int> bulletNo; 
         
         
        // Start is called before the first frame update
        void Start()
        {
            scores = new Dictionary<string, int>();
            scores = new Dictionary<string, int>();
        }
        
        public void SetScore(string playerName, int score)
        {
            if (scores.ContainsKey(playerName))
            {
                scores[playerName] = score;
            }
            else
            {
                scores.Add(playerName, score);
            }
        }

        public int GetScore(string playerName)
        {
            if(scores.ContainsKey(playerName))
            {
                return scores[playerName];
            }
            return 0;
        }
        
        public void SetBulletNo(string playerName, int score)
        {
            if (bulletNo.ContainsKey(playerName))
            { 
                bulletNo[playerName] = score;
            }
            else
            {
                bulletNo.Add(playerName, score);
            }
        }
        public int GetBulletNo(string playerName)
        {
            if (bulletNo.ContainsKey(playerName))
            {
                return bulletNo[playerName];
            }

            return 0;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            foreach(var score in scores)
            {
                GUILayout.Label($"{score.Key}: {score.Value}", new GUIStyle { normal = new GUIStyleState { textColor = Color.green }, fontSize = 22 });
            }
            
            
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
