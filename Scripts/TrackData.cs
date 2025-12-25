using System;
using System.Collections.Generic;

[Serializable]
public class TrackData
{
    public float bpm;
    public float secondsPerSample;
    public List<TrackSample> samples;
}

[Serializable]
public class TrackSample
{
    public float curve;   // -1..+1
    public float hill;    // -1..+1
    public float energy;  //  0..1
}
