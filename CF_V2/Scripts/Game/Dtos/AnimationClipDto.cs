using MiniExcelLibs.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class AnimationClipDto
    {
        public string AnimName { get; set; }
        public string AnimNameAffix
        {
            get => AnimName.Split("_").LastOrDefault();
        }

        [ExcelIgnore]
        [JsonIgnore]
        public AnimationClip AnimClip { get; set; }
        public float RealTime { get; set; }
        public float FrameRate { get; set; }
        public float Length { get; set; }
        public float Speed { get; set; }

        #region From QC
        public int TotalFrame { get; set; }
        // frames per second
        public int FPS { get; set; }
        #endregion

        // add events in code
        // https://docs.unity3d.com/ScriptReference/AnimationClip.AddEvent.html
        [ExcelIgnore]
        public List<AnimationEventDto> AnimEventDtos { get; set; } = new();

        /// <summary>
        /// false: has 1P and 3P anims
        /// true: no 3P anims / 1P speed up
        /// </summary>
        public bool SyncPawnAnim { get; internal set; } = true;

        #region help funcs

        public float GetTime()
        {
            if (Speed == 0)
            {
                Speed = 1;
            }

            return AnimClip.length / Speed;
        }

        public int GetTotalFrame()
        {
            if (AnimClip != null)
            {
                return (int)(AnimClip.frameRate * AnimClip.length);
            }

            return 0;
        }
        #endregion
    }
}