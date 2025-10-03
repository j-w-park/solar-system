using UnityEngine;

public class PlayerInspector : MonoBehaviour
{
    public Player player;

    public void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200), "Player Inspector", GUI.skin.window);
        GUILayout.Label("Velocity: " + player.Motor.Velocity);
        GUILayout.Label("Grounded: " + player.Motor.GroundingStatus.IsStableOnGround);
        GUILayout.EndArea();
    }
}
