using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy character controller.
/// </summary>
public class EnemyCharacterController : BaseCharacterController {

    /* *** Member Variables *** */

    protected bool _canSeePlayer;
    protected Vector3 _estimatedPlayerVelocity;
    protected PathfinderAI _pathfinderAI;
    protected int _layerMask;
    protected EnemyState _myState;
    protected PlayerState _playerState;
    protected Vector3 _previousPlayerPosition = Vector3.zero;

    /* *** Constructors *** */

    void Start() {
        _myState = (EnemyState)_character;
        _pathfinderAI = GetComponent<PathfinderAI>();
        _playerState = GameObject.Find("Player").GetComponentInChildren<PlayerState>();

        // When checking to see if we can see the player, we want the ray to ignore projectiles.
        _layerMask = ((1 << LayerMask.NameToLayer("Players")) |
                      (1 << LayerMask.NameToLayer("Obstacles")) |
                      (1 << LayerMask.NameToLayer("Enemies")));
    }

    /* *** MonoBehaviour Methods *** */

    /// <summary>
    /// Simulated player input for aiming and firing.
    /// </summary>
    public override void Update() {
        base.Update();

        _Aim();

        // Fire the equipped weapon
        var gun = this.equippedFirearm;
        if (gun != null) {
            if (gun.IsEmpty) {
                gun.Reload();
            } else if (_canSeePlayer) {
                // If we can't see the player, then there's nothing to fire at.
                //_Fire();
            }
        }
    }

    /// <summary>
    /// Move the NPC.
    /// Perform additional functionality that should happen at fixed
    /// intervals in FixedUpdate().
    /// </summary>
    public override void FixedUpdate() {
        _FindPlayer();
        _EstimatePlayerVelocity();

        _pathfinderAI.MoveAlongPath(_myState.maxWalkSpeed);

        base.FixedUpdate();
    }

    /* *** Member Methods *** */

    /// <summary>
    /// Try to find the player in the NPC's field of vision.
    /// </summary>
    protected void _FindPlayer() {
        _canSeePlayer = false;
        //_pathfinderAI.targetPosition = _playerState.transform.position;

        RaycastHit hitInfo;
        Quaternion quato = Quaternion.LookRotation(_myState.lookDirection, Vector3.forward);

        for (int i = _myState.fieldOfVision / 2 * -1; i < _myState.fieldOfVision / 2; i++) {
            Vector3 direction = quato * Quaternion.Euler(0, i, 0) * Vector3.forward;
            Debug.DrawRay(this.transform.position, direction * _myState.sightDistance);

            if (Physics.Raycast(this.transform.position, direction, out hitInfo, _myState.sightDistance, _layerMask)) {
                if (hitInfo.collider == _playerState.gameObject.collider) {
                    _canSeePlayer = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Estimates the player's movement velocity.
    /// Used in anticipating the player's movement when aiming.
    /// </summary>
    protected void _EstimatePlayerVelocity() {
        if (_myState.anticipatePlayerMovement && _canSeePlayer) {
            Vector3 currentPlayerPosition = _playerState.transform.position;

            if (_previousPlayerPosition != Vector3.zero) {
                Vector3 deltaPosition = currentPlayerPosition - _previousPlayerPosition;
                _estimatedPlayerVelocity = Vector3.ClampMagnitude(deltaPosition / Time.deltaTime, 1);
            }

            _previousPlayerPosition = currentPlayerPosition;
        } else {
            _previousPlayerPosition = Vector3.zero;
            _estimatedPlayerVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Aim at the player if we can see them or if not, in the direction we're moving.
    /// </summary>
    protected override void _Aim() {
        if (_canSeePlayer) {
            // If we can see the player, then aim at him.
            Vector3 playerPosition = _playerState.transform.position + _estimatedPlayerVelocity;
            _reticle.LerpTo(playerPosition, _myState.lookSpeed);
        } else {
            // If we can't see the player, aim in the direction we're moving.
            if (_moveable.velocity != Vector3.zero) {
                // We want our two vectors to have the same starting position, so we translate the
                // reticle position by the transform's position in world space. This, in effect,
                // puts the reticle position vector at the origin - the same as our velocity vector.
                Vector3 reticleRelativeToOrigin = _reticle.transform.position - this.transform.position;

                // With both vectors describing vectors relative to the origin, we can determine the
                // angle of rotation between them.
                float angle = Vector3.Angle(_moveable.velocity, reticleRelativeToOrigin);

                // We find the new reticle position by rotating our current reticle position by the angle.
                Quaternion reticleRotation = Quaternion.LookRotation(reticleRelativeToOrigin, Vector3.forward);
                Quaternion newReticleRotation = reticleRotation * Quaternion.Euler(0, angle, 0);

                // Since we subtracted the enemy position from the reticle position earlier, we have
                // to add the enemy position back into the reticle position.
                // NOTE: The 2 is an arbitrary scalar. Should that value be _myState.sightDistance?
                // Or can we keep it as a unit vector? How does the distance of the reticle from the
                // enemy affect the speed with which they aim at the player? My hunch is that by using
                // Vector3.Lerp, it makes no difference. If we were to move the the reticle by setting
                // a velocity though, the distance would make a difference.
                _reticle.LerpTo(this.transform.position + newReticleRotation * Vector3.forward * 2, _myState.lookSpeed);
            }
        }
    }
}
