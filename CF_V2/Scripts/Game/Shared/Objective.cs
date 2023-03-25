using System;
using UnityEngine;

namespace Unity.FPS.Game
{
    public abstract class Objective : MonoBehaviour
    {
        public string Title;
        public string Description;

        public bool IsOptional;

        [Tooltip("Delay before the objective becomes visible")]
        public float DelayVisible;

        public bool IsCompleted { get; private set; }
        public bool IsBlocking()
        {
            return !(IsOptional || IsCompleted);
        }

        public static event Action<Objective> OnObjectiveCreated;
        public static event Action<Objective> OnObjectiveCompleted;

        protected virtual void Start()
        {
            OnObjectiveCreated?.Invoke(this);

            DisplayMessageEvent displayMessage = Events.DisplayMessageEvent;
            displayMessage.Message = Title;
            displayMessage.DelayBeforeDisplay = 0.0f;
            EventManager.Broadcast(displayMessage);
        }

        public void UpdateObjective(string descriptionText, 
            string counterText, 
            string notificationText)
        {
            ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            EventManager.Broadcast(evt);
        }

        public void CompleteObjective(string descriptionText,
            string counterText, 
            string notificationText)
        {
            IsCompleted = true;

            ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            EventManager.Broadcast(evt);

            OnObjectiveCompleted?.Invoke(this);
        }
    }
}