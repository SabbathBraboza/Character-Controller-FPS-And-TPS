using PlayerController;
using UnityEngine;

[DefaultExecutionOrder(-3)]
public class PlayerInputManagers : MonoBehaviour
{
    public static PlayerInputManagers Instance;
    public PlayerControls PlayerControls { get; private set; }

    private void Awake()
    {
       if(Instance == null && Instance == this) 
       {
           Destroy(gameObject);
            return;
       }

       Instance = this;
      
    }

    private void OnEnable()
    {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();
    }

    private void OnDisable()
    {
        if (PlayerControls != null)
        {
            PlayerControls.Disable();
        }
    }

}
