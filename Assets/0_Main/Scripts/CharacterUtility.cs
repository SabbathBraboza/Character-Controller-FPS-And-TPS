using UnityEngine;

public static class CharacterUtility 
{
   public static Vector3 GetNormalWithSphereCast(CharacterController controller, LayerMask layer = default)
    {
        Vector3 normal = Vector3.up;
        Vector3 Center = controller.transform.position + controller.center;
        float Distance = controller.height / 2f + controller.stepOffset + 0.01f;

        RaycastHit hit;
        if(Physics.SphereCast(Center,controller.radius, Vector3.down, out hit, Distance, layer))
        {
            normal = hit.normal;
        }

        return normal;
    }
}
