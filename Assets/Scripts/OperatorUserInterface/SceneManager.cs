namespace OperatorUserInterface
{
    public enum Scene
    {
        Training, // scene with mirror to get used to the simulation
        HUD, // real world scene, casting camera, additional info on display
        Empty // main scene that contains only necessary control elements for both scenes
    }
    
    public class SceneManager : Singleton<SceneManager>
    {
        public Scene currentScene;
        
    }
}