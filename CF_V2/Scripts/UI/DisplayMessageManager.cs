using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
    // DisplayNotifyManager
    public class DisplayMessageManager : MonoBehaviour
    {
        public UITable DisplayMessageRect;
        public Notify MessagePrefab;

        List<(float timestamp, float delay, string message, Notify notification)> m_PendingMessages;

        void Awake()
        {
            EventManager.AddListener<DisplayMessageEvent>(OnDisplayMessageEvent);
            m_PendingMessages = new List<(float, float, string, Notify)>();
        }

        void OnDisplayMessageEvent(DisplayMessageEvent evt)
        {
            Notify notification = Instantiate(MessagePrefab, DisplayMessageRect.transform).GetComponent<Notify>();
            m_PendingMessages.Add((Time.time, evt.DelayBeforeDisplay, evt.Message, notification));
        }

        void Update()
        {
            foreach (var message in m_PendingMessages)
            {
                if (Time.time - message.timestamp > message.delay)
                {
                    message.Item4.Initialize(message.message);
                    DisplayMessage(message.notification);
                }
            }

            // Clear deprecated messages
            m_PendingMessages.RemoveAll(x => x.notification.Initialized);
        }

        void DisplayMessage(Notify notification)
        {
            DisplayMessageRect.UpdateTable(notification.gameObject);
            //StartCoroutine(MessagePrefab.ReturnWithDelay(notification.gameObject, notification.TotalRunTime));
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<DisplayMessageEvent>(OnDisplayMessageEvent);
        }
    }
}