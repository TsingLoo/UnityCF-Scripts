using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class ActorsManager : MonoBehaviour
    {
        public List<Actor> Actors { get; private set; }
        public GameObject Player { get; private set; }

        void Awake()
        {
            Actors = new List<Actor>();
        }

        public void SetPlayer(GameObject player)
        {
            Player = player;
        }

        #region getters
        public List<Actor> GetAllActors()
        {
            var newList = new List<Actor>();
            foreach (var actor in Actors)
            {
                newList.Add(actor);
            }

            return newList;
        }

        public List<Actor> GetBotActors()
        {
            var newList = new List<Actor>();
            foreach (var actor in Actors)
            {
                if (!actor.isPlayer)
                {
                    newList.Add(actor);
                }
            }

            return newList;
        }

        #endregion
        // End
    }
}
