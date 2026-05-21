using UnityEditor;
using System.Linq;

public class BuildScript
{
    public static void BuildWebGL()
    {
        string[] scenes = new[] { 
            "Assets/Scenes/Start.unity", 
            "Assets/Scenes/Game.unity" 
        };
        BuildPipeline.BuildPlayer(scenes, "Builds/WebGL", BuildTarget.WebGL, BuildOptions.None);
    }
}
