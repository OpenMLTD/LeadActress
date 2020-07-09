using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

namespace Imas.Live {
    [Serializable]
    [DebuggerDisplay("EventScenarioData (time={absTime}, type={type})")]
    public sealed class EventScenarioData {

        public double absTime;

        public bool selected;

        public long tick;

        public int measure;

        public int beat;

        public int track;

        public int type;

        public int param;

        public int target;

        public long duration;

        public double absEndTime;

        [NotNull]
        public string str = string.Empty;

        [NotNull]
        public string info = string.Empty;

        public int on;

        public int on2;

        public ColorRGBA col;

        public ColorRGBA col2;

        public ColorRGBA col3;

        [NotNull]
        public float[] cols = Array.Empty<float>();

        public Texture tex;

        public int texInx = -1;

        public int trig;

        public float speed;

        public int idol;

        public int camNo;

        // It means "singing", not "mute"
        [NotNull]
        public bool[] mute = Array.Empty<bool>();

        public bool addf;

        public float eye_x;

        public float eye_y;

        [NotNull]
        public Vector4[] formation = Array.Empty<Vector4>();

        public bool appeal;

        public int layer;

        public int cheeklv;

        public bool eyeclose;

        public bool talking;

        public bool delay;

        [NotNull]
        public int[] clratio = Array.Empty<int>();

        [NotNull]
        public int[] clcols = Array.Empty<int>();

        public int camCut = -1;

        [NotNull]
        public VjParam vjparam = new VjParam();

        public int seekFrame;

        public float fvalue;

        public float fvalue2;

        public int idol2;

        public int param2;

        // public object[] vecs1;

        public bool bool1;

    }
}
