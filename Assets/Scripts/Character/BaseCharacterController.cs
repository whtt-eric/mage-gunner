using UnityEngine;
using System.Collections;

/// <summary>
/// The base class for character handlers.
/// Associated GameObject should have a CharacterState-derived script
/// and a kindred-sprite Sprite-derived script.
///
/// You cannot assign this to a game object directly.  You must
/// inherit from this class.
///
/// Sub-classes should modify character state in the 'Act' function.
/// </summary>
public abstract class BaseCharacterController : MonoBehaviour {

    /* *** Member Variables *** */

    protected BaseCharacterState _character;
    protected Inventory _inventory;
    protected ReticleController _reticle;
    protected Rigidbody _rigidbody;

    public BaseCharacterState character {
        get { return _character; }
    }

    public Inventory inventory {
        get { return _inventory; }
    }

    public ReticleController reticle {
        get { return _reticle; }
    }

    /* *** Constructors *** */
    public virtual void Awake() {
        _character = GetComponent<BaseCharacterState>();
        _inventory = GetComponentInChildren<Inventory>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;

        // Create a reticle for this character.
        GameObject reticlePrefab = (GameObject)Resources.Load("Prefabs/Reticle");
        Vector3 spawnPosition = _character.transform.position + _character.lookDirection;
        GameObject reticleInstance = (GameObject)Instantiate(reticlePrefab, spawnPosition, reticlePrefab.transform.rotation);
        reticleInstance.transform.parent = _character.transform;
        _reticle = reticleInstance.GetComponent<ReticleController>();
        _character.LookAt(_reticle.transform.position);
    }


    /* *** MonoBehaviour Methods *** */

    public virtual void Update() {
        _character.LookAt(_reticle.transform.position);
    }

    public virtual void FixedUpdate() {
    }

    /* *** Member Methods *** */

    public void Kneel() {
        if (!_character.kneeling) {
            Vector3 scale = this.transform.localScale;
            Vector3 halfHeight = new Vector3(scale.x, scale.y / 2, scale.z);
            this.transform.localScale = halfHeight;

            Vector3 position = this.transform.position;
            Vector3 newPosition = new Vector3(position.x, position.y - halfHeight.y / 2, position.z);
            this.transform.position = newPosition;

            _character.kneeling = true;
        }
    }

    public void Stand() {
        if (_character.kneeling) {
            Vector3 scale = this.transform.localScale;
            Vector3 fullHeight = new Vector3(scale.x, scale.y * 2, scale.z);
            this.transform.localScale = fullHeight;

            Vector3 position = this.transform.position;
            Vector3 newPosition = new Vector3(position.x, position.y + fullHeight.y / 4, position.z);
            this.transform.position = newPosition;

            _character.kneeling = false;
        }
    }

    /// <summary>
    /// Equip the passed in firearm
    /// </summary>
    /// <param name='firearm'>
    /// The firearm to equip.
    /// </param>
    public void EquipWeapon(Firearm firearm) {
        _character.equippedFirearm = firearm;
        _reticle.ResetRecoil();
        _reticle.willUpdateReticlePosition = firearm.recoilMovesReticle;
    }

    /// <summary>
    /// Fires the currently equipped firearm, and applies the recoil
    /// to the reticle.
    /// </summary>
    public void Fire() {
        _Fire();
    }

    protected void _Fire() {
        Vector3 aimPoint = _character.aimPoint;

        if (_character.kneeling) {
            aimPoint += new Vector3(0, 0, 0.3f); // Move the bullet spawn point to above the cover
        }

        // TODO: Ensure that we aren't firing into the ground when the reticle is close to the player
        Vector3 bulletVector = _reticle.ActualTargetPosition - aimPoint;

        if (_character.kneeling) {
            RaycastHit hitInfo;
            if (Physics.Raycast(this.transform.position, bulletVector, out hitInfo)) {
                if (hitInfo.collider.CompareTag("Cover")) {
                    // TODO: Don't hardcode this range. Make it an attribute on the firearm?
                    float scatterAmount = UnityEngine.Random.Range(-20, 20);
                    Quaternion quato = Quaternion.LookRotation(bulletVector, Vector3.forward);
                    bulletVector = quato * Quaternion.Euler(0, scatterAmount, 0) * Vector3.forward;
                }
            }
        }

        Vector3 recoil = _character.equippedFirearm.Fire(bulletVector);
        _reticle.AddRecoil(recoil);
    }

    /// <summary>
    /// Returns true if the character will pop up from cover.
    /// </summary>
    public abstract bool WillPopUp(Vector3 fromDirection);
}
