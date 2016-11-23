using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//holds references to everything that cannot be easily connected together before runtime
//because one or both components are generated at runtime (procedural generation)
//IE: Player GUI and Player character
public class ProceduralComponentConnector {
    private static TestPlayerController playerControllerToLink;
    private static Slider HealthBar;
    private static Slider TimeBar;
    private static Slider EnergyBar;

    //precondition: health,time,energy != null
    public static void AllocatePlayerGUILink(Slider health, Slider time, Slider energy)
    {
        //if we already have player ready, connect it to Sliders
        if(playerControllerToLink != null)
        {
            playerControllerToLink.ConfigureGUIConnections(health,energy,time);
        }
        else //else store sliders and wait
        {
            HealthBar = health;
            TimeBar = time;
            EnergyBar = energy;
        }
    }

    //precondition: player != null
    //stores PlayerController to link with eventual Sliders, or links PlayerController with Sliders if they are available
    public static void AllocatePlayerGUILink(TestPlayerController player)
    {
        //if we already have health,time,energy bars ready, connect them to player
        if(HealthBar != null && TimeBar != null && EnergyBar != null)
        {
            playerControllerToLink = player;
            AllocatePlayerGUILink(HealthBar, TimeBar, EnergyBar);
        }
        else //else store player and wait to connect
        {
            playerControllerToLink = player;
        }
    }

    public static bool isUsed;
    public static void DeAllocate()
    {
        playerControllerToLink = null;
        HealthBar = null;
        TimeBar = null;
        EnergyBar = null;
    }
}
