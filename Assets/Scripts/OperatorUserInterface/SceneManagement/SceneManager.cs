namespace OperatorUserInterface
{
    public enum Scene
    {
        Training, // scene with mirror to get used to the simulation
        HUD, // real world scene, casting camera, additional info on display
        Empty // main scene that contains only necessary control elements for both scenes
    }
    
    /// <summary>
    /// TODO:
    /// This class should combine <see cref="StateManager"/>, <see cref="OperatorUI.AdditiveSceneManager"/>,
    /// <see cref="TransitionManager"/>, and <see cref="ConstructFXManager"/>.
    /// One prefab object managing the state should be created.
    /// </summary>
    public class SceneManager : Singleton<SceneManager>
    {
        public Scene currentScene;
        
    }
}