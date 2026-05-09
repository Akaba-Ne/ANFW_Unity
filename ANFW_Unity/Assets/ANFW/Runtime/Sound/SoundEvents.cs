namespace ANFW.Sound
{
    public struct PlayBGMEvent
    {
        public string Address;
    }

    public struct StopBGMEvent { }

    public struct PlaySEEvent
    {
        public string Address;
    }

    public struct PlaySEAtPositionEvent
    {
        public string Address;
        public UnityEngine.Vector3 Position;
    }

    public struct SetBGMVolumeEvent
    {
        public float Volume;
    }

    public struct SetSEVolumeEvent
    {
        public float Volume;
    }

    public struct SetMasterVolumeEvent
    {
        public float Volume;
    }
}
