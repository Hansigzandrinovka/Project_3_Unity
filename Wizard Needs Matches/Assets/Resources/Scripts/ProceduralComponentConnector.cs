using UnityEngine;
using System.Collections;
using UnityEngine.UI;

///holds references to everything that cannot be easily connected together before runtime
///because one or both components are generated at runtime (procedural generation)
///IE: Player GUI and Player character
public class ProceduralComponentConnector {
    private static TestPlayerController playerControllerToLink;
    private static Slider HealthBar; ///the Slider to represent a character's health bar, needs player character in order to render correctly
    private static Slider TimeBar; /// ^^^^ time bar, needs player character
    private static Slider EnergyBar; /// ^^^^^^ energy bar, needs player character
    private static Canvas cameraCanvas; ///the canvas that needs player information to render correctly

    ///precondition: health,time,energy != null
    public static void AllocatePlayerGUILink(Slider health, Slider time, Slider energy)
    {
        isUsed = true;
        HealthBar = health;
        TimeBar = time;
        EnergyBar = energy;
        //if we already have player ready, connect it to Sliders
        if (playerControllerToLink != null)
        {
            playerControllerToLink.ConfigureGUIConnections(health,energy,time);
        }
    }

    public static void AllocateGUICameraListener(Canvas dungeonCanvas)
    {
        isUsed = true;
        cameraCanvas = dungeonCanvas;
        if (playerControllerToLink != null)
        {
            cameraCanvas.worldCamera = playerControllerToLink.gameObject.GetComponentInChildren<Camera>();
            if(cameraCanvas.worldCamera == null)
            {
                Debug.LogError("Alert! Unable to fetch Camera object for Procedurally-generated Character to display GUI");
            }
        }
    }

    public static void AllocateGUICameraListener(TestPlayerController player)
    {
        isUsed = true;
        playerControllerToLink = player;
        if(cameraCanvas != null)
        {
            AllocateGUICameraListener(cameraCanvas);
        }
    }

    ///precondition: player != null
    ///stores PlayerController to link with eventual Sliders, or links PlayerController with Sliders if they are available
    public static void AllocatePlayerGUILink(TestPlayerController player)
    {
        playerControllerToLink = player;
        isUsed = true;
        //if we already have health,time,energy bars ready, connect them to player
        if (HealthBar != null && TimeBar != null && EnergyBar != null)
        {
            playerControllerToLink = player;
            AllocatePlayerGUILink(HealthBar, TimeBar, EnergyBar);
        }
    }

    public static bool isUsed;

    ///sets every component in the Connector to null,
    ///to allow garbage collecting of referenced objects
    public static void DeAllocate()
    {
        playerControllerToLink = null;
        HealthBar = null;
        TimeBar = null;
        EnergyBar = null;
    }
}
