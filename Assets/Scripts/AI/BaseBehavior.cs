﻿using UnityEngine;
using System.Collections;

public class BaseBehavior : MonoBehaviour {

    public bool isActive;

    protected NpcController _controller;
    protected Rigidbody _rigidbody;
    
    public void Start() {
        _controller = GetComponent<NpcController>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Update() {
        if (this.isActive) {
            _Update();
        }
    }

    /// <summary>
    /// Override in the child classes.
    /// </summary>
    protected virtual void _Update() { }
    
    public void FixedUpdate() {
        if (this.isActive) {
            _FixedUpdate();
        }
    }

    /// <summary>
    /// Override in the child classes.
    /// </summary>
    protected virtual void _FixedUpdate() { }

    /// <summary>
    /// Activates this behavior. An activated behavior will perform
    /// the Update and FixedUpdate functions.
    /// </summary>
    public void Activate() {
        foreach (BaseBehavior b in _controller.behaviors) {
            b.Deactivate();
        }

        _Activate();
        this.isActive = true;
    }

    /// <summary>
    /// Override in the child classes.
    /// </summary>
    protected virtual void _Activate() { }

    /// <summary>
    /// Deactivates this behavior. Use as a cleanup method.
    /// </summary>
    public void Deactivate() {
        if (this.isActive) {
            _Deactivate();
            this.isActive = false;
        }
    }

    /// <summary>
    /// Override in the child classes.
    /// </summary>
    protected virtual void _Deactivate() { }
}
