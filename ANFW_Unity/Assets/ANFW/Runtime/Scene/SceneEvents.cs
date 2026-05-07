namespace ANFW.Scene
{
    public struct LoadSceneEvent
    {
        public string SceneName;
        public bool Additive;
    }

    public struct UnloadSceneEvent
    {
        public string SceneName;
    }

    public struct SceneLoadedEvent
    {
        public string SceneName;
    }

    public struct SceneUnloadedEvent
    {
        public string SceneName;
    }
}
