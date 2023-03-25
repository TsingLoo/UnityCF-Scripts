namespace Unity.FPS.Game
{
    public class AnimationEventDto
    {
        public string AnimName { get; set; }
        public string FunctionName { get; set; }
        public float Frame { get; set; }
        public string SoundName { get; set; } // StringParameter
        public float SoundVolume { get; set; } // FloatParameter
        public float RealTime { get; set; }

        public float Time { get; set; }
        public float TotalFrame { get; set; }

        public string StringParameter;
        public float FloatParameter;
        public int IntParameter;
        public int MessageOptions;
    }
}