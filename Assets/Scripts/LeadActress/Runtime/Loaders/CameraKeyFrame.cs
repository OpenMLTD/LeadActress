namespace LeadActress.Runtime.Loaders {
    public sealed class CameraKeyFrame {

        public CameraKeyFrame(int frameIndex) {
            FrameIndex = frameIndex;
            Time = frameIndex / (float)FrameRate.Mltd;
        }

        public int FrameIndex { get; }

        public float Time { get; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float PositionZ { get; set; }

        public float TargetX { get; set; }

        public float TargetY { get; set; }

        public float TargetZ { get; set; }

        // X and Y are constant 0
        public float AngleX { get; set; }

        public float AngleY { get; set; }

        public float AngleZ { get; set; }

        public float FocalLength { get; set; }

        public int Cut { get; set; }

    }
}
