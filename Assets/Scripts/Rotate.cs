using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using Photon.Realtime;


public class Rotate : MonoBehaviourPun
{
    [SerializeField]
    private GameObject Camera;

    [SerializeField]
    private GameObject player;

    public KeyCode but = KeyCode.P;
    private PhotonView view;

    [SerializeField]
    public Recorder VoiceRecorder;
    // Start is called before the first frame update
    void Start()
    {
        var renderr = player.GetComponent<Renderer>();
        Color newColor = new Color(Random.value, Random.value, Random.value, 1.0f);
        // apply it on current object's material
        renderr.material.SetColor("_Color", newColor);
        view = photonView;
        VoiceRecorder.TransmitEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
      var CharacterRotation = Camera.transform.rotation;
      CharacterRotation.x = 0;
      CharacterRotation.z = 0;
      player.transform.rotation = CharacterRotation;

      if (Input.GetKeyDown(but))
        {
            if (view.IsMine)
            {
                VoiceRecorder.TransmitEnabled = true;
            }

        }
        if (Input.GetKeyUp(but))
        {
            if (view.IsMine)
            {
                VoiceRecorder.TransmitEnabled = false;
            }

        }


    }
}
