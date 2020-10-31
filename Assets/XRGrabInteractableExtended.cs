using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractableExtended : XRGrabInteractable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected override void OnSelectEnter(XRBaseInteractor interactor)
    {
        base.OnSelectEnter(interactor);

        var m_RigidBody = GetComponent<Rigidbody>();
        m_RigidBody.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }

    protected override void OnSelectExit(XRBaseInteractor interactor)
    {
        base.OnSelectExit(interactor);
        var m_RigidBody = GetComponent<Rigidbody>();
        m_RigidBody.constraints ^= RigidbodyConstraints.FreezeRotationZ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
