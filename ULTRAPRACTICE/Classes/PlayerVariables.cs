using System.Collections;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

public sealed class PlayerVariables : IVariableSaver
{
    public static Vector3 savedPos;
    public static Vector3 savedVel;
    public static Quaternion savedRot;
    public static float rotationX;
    public static float rotationY;
    public static bool heavyFall;
    public static NewMovementVars savedVars;

    public static float timeUntilNotJumping;
    public static float timeUntilJumpReady;
    public static float timeUntilJumpReadyMax;
    public static float timeUntilNotJumpingMax;
    public static float savedTimeUntilNotJumping;
    public static float savedTimeUntilJumpReady;

    public void SaveVariables()
    {
        var ply = Plugin.Instance.player;
        GameObject plyObj = ply.gameObject;
        Rigidbody rb = ply.rb;

        if (savedVars != null) Object.Destroy(savedVars.gameObject);

        savedVars = new GameObject().AddComponent<NewMovementVars>();

        UpdateBehaviour.CopyValues(savedVars, ply);

        savedPos = plyObj.transform.position;
        savedVel = rb.velocity;
        savedRot = ply.cc.gameObject.transform.rotation;
        heavyFall = ply.gc.heavyFall;
        rotationX = ply.cc.rotationX;
        rotationY = ply.cc.rotationY;
    }

    public void SetVariables()
    {
        var ply = Plugin.Instance.player;
        GameObject plyObj = ply.gameObject;
        Rigidbody rb = plyObj.GetComponent<Rigidbody>();

        ply.gc.ForceOff();
        ply.gc.onGround = false;
        plyObj.transform.position = savedPos;
        ply.cc.gameObject.transform.rotation = savedRot;
        ply.cc.rotationX = rotationX;
        ply.cc.rotationY = rotationY;
        ply.gc.heavyFall = heavyFall;

        MonoSingleton<UpdateBehaviour>.Instance.StartCoroutine(SetVelocityAfter(ply));
    }

    //as with v2 i delay the next instructions a frame later so that speed doesnt bug out in weird scenarios and we don't stomp mid air

    public static IEnumerator SetVelocityAfter(NewMovement ply)
    {
        yield return new WaitForFixedUpdate();
        Rigidbody rb = ply.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = savedVel;
        }

        UpdateBehaviour.CopyValues(ply, savedVars);

        if (ply.jumping)
        {
            ply.Invoke(nameof(NewMovement.JumpReady), timeUntilJumpReadyMax - timeUntilJumpReady);
            ply.Invoke(nameof(NewMovement.NotJumping), timeUntilNotJumpingMax - timeUntilNotJumping);
        }
        ply.gc.StopForceOff();
    }
}