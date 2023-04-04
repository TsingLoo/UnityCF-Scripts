using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class PatrolPath : MonoBehaviour
    {
        public List<BotController> EnemiesToAssign = new List<BotController>();

        public List<Transform> PathNodes = new List<Transform>();

        void Start()
        {
            SignPatrolPath();

            EventManager.AddListener<BotAddEvent>(OnBotAdd);
        }

        private void OnBotAdd(BotAddEvent evt)
        {
            var botCon = evt.Bot.GetComponent<BotController>();
            botCon.PatrolPath = this;
        }

        private void SignPatrolPath()
        {
            // enemys
            if (!EnemiesToAssign.HasValue())
            {
                EnemiesToAssign = FindObjectsOfType<BotController>().ToList();
            }

            // path
            if (!PathNodes.HasValue())
            {
                var playerTeamStarts = GameFlowManager.Ins.BaseGameMode.playerTeamStarts;
                var enemyTeamStarts = GameFlowManager.Ins.BaseGameMode.enemyTeamStarts;

                if (playerTeamStarts.HasValue())
                {
                    PathNodes.Add(playerTeamStarts.FirstOrDefault().transform);
                }
                if(enemyTeamStarts.HasValue())
                {
                    PathNodes.Add(enemyTeamStarts.FirstOrDefault().transform);
                }
            }

            foreach (var enemy in EnemiesToAssign)
            {
                enemy.PatrolPath = this;
            }
        }

        public float GetDistanceToNode(Vector3 origin, int destinationNodeIndex)
        {
            if (destinationNodeIndex < 0 || destinationNodeIndex >= PathNodes.Count ||
                PathNodes[destinationNodeIndex] == null)
            {
                return -1f;
            }

            return (PathNodes[destinationNodeIndex].position - origin).magnitude;
        }

        public Vector3 GetPositionOfPathNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= PathNodes.Count || PathNodes[nodeIndex] == null)
            {
                return Vector3.zero;
            }

            return PathNodes[nodeIndex].position;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < PathNodes.Count; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex >= PathNodes.Count)
                {
                    nextIndex -= PathNodes.Count;
                }

                Gizmos.DrawLine(PathNodes[i].position, PathNodes[nextIndex].position);
                Gizmos.DrawSphere(PathNodes[i].position, 0.1f);
            }
        }
    }
}