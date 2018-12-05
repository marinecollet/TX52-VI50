﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WandManager : MonoBehaviour {

    // Use this for initialization
    public Teleport teleport;

    public float range = 100f;
    public List<SpellDefinition> spellList;
    public float moveObjectSpeed;
    public Renderer mesh_teleport;

    [SerializeField]
    public Transform player;
    [SerializeField]
    public Transform camera;

    //private Dictionary<string,List<SpellColliderType>> colliderDictio;

    private SpellTree spellTree;

    [HideInInspector]
    public bool isReading;

    Ray shootRay;
    RaycastHit shootHit;
    ParticleSystem spellParticles;
    LineRenderer gunLine;

    int movableMask;
    int doorMask;

    Vector3 position = new Vector3();

    private Collider spellCollider;

    ChangeParent changedParentGameObject = null;

    private bool tryToMove;
    
    GameObject spellShot;
    

    [System.Serializable]
    public struct SpellDefinition
    {
        public string spellName;
        public SpellColliderType[] colliderOrder;
    }

    void Awake () {
        spellCollider = this.GetComponent<Collider>();
        gunLine = GetComponent<LineRenderer>();

        tryToMove = true;
        isReading = false;
        movableMask = LayerMask.GetMask("Movable");
        doorMask = LayerMask.GetMask("Door");

        spellShot = Resources.Load("Sphere") as GameObject;

        spellParticles = GetComponent<ParticleSystem>();

        spellTree = new SpellTree();

        foreach (SpellDefinition spell in spellList)
        {
            spellTree.addSpell(new List<SpellColliderType>(spell.colliderOrder) , spell.spellName);
        }

        spellTree.DebugTree();
    }

    public void AddSortCollider(SpellColliderType col)
    {
        if (this.spellTree.advance(col))
        {
            string spell = this.spellTree.isSpell();

            if (spell != null)
            {
                launchSpell(spell);
                spellTree.resetActualNode();
            }
        }
        else
        {
            spellTree.resetActualNode();
        }
    }

    void launchSpell(string spell)
    {
        spellParticles.Stop();
        spellParticles.Play();

        switch (spell)
        {

            case "shot":
                GameObject wand = GameObject.FindGameObjectWithTag("Wand");
                GameObject projectile = Instantiate(spellShot) as GameObject;
                projectile.transform.position = transform.position + wand.transform.forward;
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                rb.velocity = wand.transform.forward * 20;
                break;
            case "lave":
                //Set the shootRay so that it starts at the end of the wand and points forward.
                //Debug.Log("rayCast");
                //Debug.Log(transform.position);

                gunLine.enabled = true;
                gunLine.SetPosition(0, transform.position);

                shootRay.origin = transform.position;
                shootRay.direction = transform.forward;

                if (Physics.Raycast(shootRay, out shootHit, range, movableMask))
                {
                    //Debug.Log("rayCast success");
                    // Try and find an ChangeParent script on the gameobject hit.
                    changedParentGameObject = shootHit.collider.GetComponent<ChangeParent>();
                    //If the EnemyHealth component exist...
                    if (changedParentGameObject != null)
                    {
                        //Debug.Log("change success");
                        //... the enemy should take damage.
                        changedParentGameObject.changeParent(this.gameObject);
                    }

                    //Set the second position of the line renderer to the point the raycast hit.
                    gunLine.SetPosition(1, shootHit.point);
                }
                else
                {
                    //Debug.Log("rayCast fail");
                    //... set the second position of the line renderer to the fullest extent of the gun's range.
                    gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
                }
                break;
            case "holohomora":
                //Set the shootRay so that it starts at the end of the wand and points forward.
                //Debug.Log("holohomora racast");
                //Debug.Log(transform.position);

                gunLine.enabled = true;
                gunLine.SetPosition(0, transform.position);

                shootRay.origin = transform.position;
                shootRay.direction = transform.forward;

                if (Physics.Raycast(shootRay, out shootHit, range, doorMask))
                {
                    //Debug.Log("rayCast success");
                    // Try and find an ChangeParent script on the gameobject hit.
                    //Animator anim = shootHit.collider.GetComponent<Animator>();
                    ////If the EnemyHealth component exist...
                    //if (anim != null && anim.GetBool("isOpen") != true)
                    //{
                    //    Debug.Log("open success success");
                    //    //... the enemy should take damage.
                    //    anim.SetBool("isOpen",true);
                    //}

                    DoorManager doorManager = shootHit.collider.GetComponent<DoorManager>();
                    if(doorManager!= null)
                    {
                        doorManager.openTheDoor();
                    }

                    //Set the second position of the line renderer to the point the raycast hit.
                    gunLine.SetPosition(1, shootHit.point);
                }
                else
                {
                    //Debug.Log("holohomora rayCast fail");
                    //... set the second position of the line renderer to the fullest extent of the gun's range.
                    gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
                }
                break;
            default:
                break;
        }
        Debug.Log(spell);

        // Stop the particles from playing if they were, then start the particles.


        // Enable the line renderer and set it's first position to be the end of the gun.
    }

    public void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) && changedParentGameObject != null)
        {
            changedParentGameObject.reset();
            changedParentGameObject = null;
        }

        if ((OVRInput.Get(OVRInput.RawButton.RThumbstickUp) || OVRInput.Get(OVRInput.RawButton.RThumbstickDown)) && changedParentGameObject != null)
        {
            Vector2 vect = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);

            Vector3 dir = (changedParentGameObject.transform.localPosition - this.transform.position).normalized;

            Vector3 newPosition = changedParentGameObject.transform.localPosition + dir * vect.y * moveObjectSpeed;

            if((newPosition - this.transform.position).sqrMagnitude > 1)
            {
                changedParentGameObject.transform.localPosition = newPosition;
            }
        }

        /**
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            if(mesh_teleport.enabled == true)
            {
                mesh_teleport.enabled = false;
            }
            else
            {
                mesh_teleport.enabled = true;
            }
        }**/
        if (OVRInput.GetDown(OVRInput.RawButton.LThumbstick))
        {
            if (mesh_teleport.enabled == true)
            {
                tryToMove = false;
                mesh_teleport.enabled = false;
            }
            else
            {
                tryToMove = true;
                mesh_teleport.enabled = true;
            }
        }

        if (tryToMove && (OVRInput.Get(OVRInput.RawButton.LThumbstickUp) || OVRInput.Get(OVRInput.RawButton.LThumbstickDown)))
        {
            Vector2 vector_joystick = new Vector2();
            vector_joystick = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
            Debug.Log("aaaaa " + vector_joystick);
            teleport.SetVelocity(teleport.velocity + vector_joystick.y *0.1);
        }

        if (OVRInput.GetUp(OVRInput.RawButton.LThumbstick))
        {
            if (teleport.getRight())
            {
                position = teleport.getPosition();
                player.position = new Vector3(position.x, player.position.y, position.z);
                camera.position = new Vector3(position.x, camera.position.y, position.z);
            }
        }
    }


}

