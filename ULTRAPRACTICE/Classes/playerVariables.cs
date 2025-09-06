using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ULTRAPRACTICE.Classes;

//similar to weaponcharges
public sealed class NewMovementVars : MonoBehaviour
{
    public bool modNoDashSlide;

    [HideInInspector]
    public bool modNoJump;

    [HideInInspector]
    public float modForcedFrictionMultip = 1f;

    public float friction;

    public InputManager inman;

    [HideInInspector]
    public AssistController asscon;

    public float walkSpeed;

    public float jumpPower;

    public float airAcceleration;

    public float wallJumpPower;

    public bool jumpCooldown;

    public bool enemyStepping;

    [HideInInspector]
    public bool falling;

    [HideInInspector]
    public Rigidbody rb;

    public Vector3 movementDirection;

    public Vector3 movementDirection2;

    public Vector3 airDirection;

    public float timeBetweenSteps;

    public float stepTime;

    public int currentStep;

    public Quaternion tempRotation;

    public GameObject forwardPoint;

    public GroundCheck gc;

    public GroundCheck slopeCheck;

    public WallCheck wc;

    public Vector3 wallJumpPos;

    public int currentWallJumps;

    public AudioSource aud;

    public AudioSource aud2;

    public AudioSource aud3;

    public int currentSound;

    public AudioClip jumpSound;

    public AudioClip landingSound;

    public AudioClip finalWallJump;

    public bool walking;

    public int hp = 100;

    public float antiHp;

    public float antiHpCooldown;

    public bool cantInstaHeal;

    public Image hurtScreen;

    public AudioSource hurtAud;

    public Color hurtColor;

    public Color currentColor;

    public float hurtInvincibility;

    public bool dead;

    public bool endlessMode;

    public DeathSequence deathSequence;

    public FlashImage hpFlash;

    public FlashImage antiHpFlash;

    public AudioSource greenHpAud;

    public float currentAllVolume;

    public bool boost;

    public Vector3 dodgeDirection;

    public float boostLeft;

    public float dashStorage;

    public float boostCharge = 300f;

    public AudioClip dodgeSound;

    public CameraController cc;

    public GameObject staminaFailSound;

    public GameObject screenHud;

    public Vector3 hudOriginalPos;

    public GameObject dodgeParticle;

    public GameObject scrnBlood;

    public Canvas fullHud;

    public GameObject hudCam;

    public Vector3 camOriginalPos;

    public RigidbodyConstraints defaultRBConstraints;

    public GameObject revolver;

    public StyleHUD shud;

    public GameObject wallScrape;

    public SurfaceType currentScrapeSurfaceType;

    public StyleCalculator scalc;

    public bool activated;

    public int gamepadFreezeCount;

    public float fallSpeed;

    public bool jumping;

    public float fallTime;

    public GameObject impactDust;

    public GameObject fallParticle;

    public GameObject currentFallParticle;

    [HideInInspector]
    public CapsuleCollider playerCollider;

    public bool sliding;

    public float slideSafety;

    public GameObject slideParticle;

    public GameObject currentSlideParticle;

    public ParticleSystem.TrailModule slideTrail;

    public ParticleSystem.MinMaxGradient normalSlideGradient;

    public ParticleSystem.MinMaxGradient invincibleSlideGradient;

    public GameObject slideScrape;

    public SurfaceType currentSlideSurfaceType;

    public Vector3 slideMovDirection;

    public GameObject slideStopSound;

    public bool crouching;

    public bool standing;

    public bool slideEnding;

    public Vector3 groundCheckPos;

    public AudioSource oilSlideEffect;

    public bool onGasoline;

    public GameObject currentFrictionlessSlideParticle;

    public SurfaceType currentFricSlideSurfaceType;

    public AudioSource[] fricSlideAuds;

    public float[] fricSlideAudVols;

    public float[] fricSlideAudPitches;

    public LayerMask frictionlessSurfaceMask;

    public GunControl gunc;

    public float currentSpeed;

    public FistControl punch;

    public GameObject dashJumpSound;

    public bool slowMode;

    public Vector3 pushForce;

    public float slideLength;

    [HideInInspector]
    public float longestSlide;

    public float preSlideSpeed;

    public float preSlideDelay;

    public bool quakeJump;

    public GameObject quakeJumpSound;

    [HideInInspector]
    public bool exploded;

    [HideInInspector]
    public float safeExplosionLaunchCooldown;

    public float clingFade;

    public bool stillHolding;

    public float slamForce;

    public bool slamStorage;

    [HideInInspector]
    public float slamCooldown;

    public bool launched;

    public int difficulty;

    [HideInInspector]
    public int sameCheckpointRestarts;

    public CustomGroundProperties groundProperties;

    [HideInInspector]
    public int rocketJumps;

    [HideInInspector]
    public int hammerJumps;

    [HideInInspector]
    public Grenade ridingRocket;

    [HideInInspector]
    public int rocketRides;

    public float ssjMaxFrames = 4f;

    public Light pointLight;

    public TimeSince sinceSlideEnd;

    [HideInInspector]
    public bool levelOver;

    [HideInInspector]
    public HashSet<Water> touchingWaters = new();

    public Vector3Int? lastCheckedGasolineVoxel;

    public int framesSinceSlide;

    public Vector3 velocityAfterSlide;
}

public static class playerVariables
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

    public static void SaveVariables(NewMovement ply)
    {
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

    public static void SetVariables(NewMovement ply)
    {
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