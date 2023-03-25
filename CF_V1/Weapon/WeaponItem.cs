using System.Collections;
using System.Linq;
using UnityEngine;

public class WeaponItem : MonoBehaviour
{
    [Header("Controller")]
    public WeaponController Weapon1P;
    public Transform Muzzle;

    [field: SerializeField]
    private bool pickedUp { get; set; }

    BasePawnController _ownerPawn;

    [Header("Drop")]
    // can be picked up again after this time
    public float dropTime = 1.0f;
    public float dropForwardForce = 10;
    [HideInInspector] public float dropUpwardForce = 0;//3
    // dist to switch to on ground
    public float dropDist = 1.5f;
    public bool isThrowing = false;

    public float autoPickUpRange = 1f;

    [Header("Physics")]
    public Rigidbody rigidBody;

    /// <summary>
    /// player collider
    /// </summary>
    public Collider pickupCollider;

    PlayerController player;
    Transform _playerTransform;

    private void Awake()
    {
        pickedUp = false;
    }

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
        player = _playerTransform.GetComponent<PlayerController>();
        if(player == null)
        {
            Debug.LogError("player not found");
        }

        if (_ownerPawn != null && pickedUp)
        {
            SetPickUpProperty(_ownerPawn.weapon3PLayer);
        }
        else // on ground
        {
            SetOnGroundProperty();
        }
    }

    private void Update()
    {
        // switch state when leave player far enough, to avoid fall in ground
        if (isThrowing)
        {
            // Check if player is in range
            Vector3 distanceToPlayer =
                _playerTransform.position - transform.position;
            if (distanceToPlayer.magnitude >= dropDist)
            {
                SetOnGroundProperty();
            }
        }
        else if(!pickedUp) // not throwing, not picked up
        {
            // Check if player is in range
            Vector3 distanceToPlayer =
                _playerTransform.position - transform.position;
            var dist2D = new Vector3(distanceToPlayer.x, 0, distanceToPlayer.z);
            if (dist2D.magnitude <= autoPickUpRange)
            {
                if (player.PickupWeapon(this))
                {
                    this.SelfDestroy();
                }
            }
        }
    }

    /// <summary>
    /// set layer, physics ...
    /// before destroy
    /// </summary>
    public void OnPickUp(BasePawnController ownerPawn)
    {
        _ownerPawn = ownerPawn;

        SetPickUpProperty(ownerPawn.weapon3PLayer);
    }


    public void Throw(Transform cameraTransform)//Vector3 playerVelocity
    {
        //Set parent to null
        transform.SetParent(null);
        isThrowing = true;

        #region physics
        // in order to set speed
        //EnablePhysics();
        SetDroppingProperty();

        // velocity
        //rigidBody.velocity = playerVelocity;

        // force, forward
        rigidBody.AddForce(cameraTransform.forward * dropForwardForce,
            ForceMode.Impulse);

        // todo change to horizontal?
        // up
        //rigidBody.AddForce(cameraTransform.up * dropUpwardForce,
        //    ForceMode.Impulse);

        // random rotation
        //float random = Random.Range(-1f, 1f);
        //rigidBody.AddTorque(new Vector3(random, random, random)
        //    * 5);
        #endregion

        StartCoroutine(FinishDrop());


        //Disable script
    }

    private IEnumerator FinishDrop()
    {
        yield return new WaitForSeconds(dropTime);

        SetOnGroundProperty();
    }

    private void SetPickUpProperty(EditorLayer weapon3PLayer)
    {
        pickedUp = true;

        this.Setlayer(weapon3PLayer);

        DisablePhysics();
        // disable world collider
        GetComponentsInChildren<BoxCollider>()
            .ToList()
            .ForEach(it=> it.enabled = false);

        // disable trigger collider
        if(pickupCollider != null)
            pickupCollider.enabled = false;
    }


    private void SetOnGroundProperty()
    {
        pickedUp = false;
        isThrowing = false;

        this.Setlayer(EditorLayer.Pickup);

        EnablePhysics();
        if (pickupCollider != null)
            pickupCollider.enabled = true;
    }

    private void SetDroppingProperty()
    {
        pickedUp = false;

        this.Setlayer(EditorLayer.Pickup);

        EnablePhysics();
        if (pickupCollider != null)
            pickupCollider.enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        var playerCon = other.GetComponent<PlayerController>();
        if (playerCon != null)
        {
            if (playerCon.PickupWeapon(this))
            {
                this.SelfDestroy();
            }
        }
    }

    private void EnablePhysics()
    {
        rigidBody.isKinematic = false;
    }

    private void DisablePhysics()
    {
        rigidBody.isKinematic = true;
    }

    
}
