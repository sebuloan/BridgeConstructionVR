using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabHandDetector : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public string handName;
 //   public TextMeshProUGUI grabhandstatus;
    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        //grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
         handName = GetInteractorHandName(args.interactorObject.transform);
      //  grabhandstatus.text = handName;
        Debug.Log("Grabbed by: " + handName);
    }

    //private void OnSelectExited(SelectExitEventArgs args)
    //{
    //    string handName = GetInteractorHandName(args.interactorObject.transform);
    //    Debug.Log("Released by: " + handName);
    //  //  grabhandstatus.text = handName;

    //}
    private string GetInteractorHandName(Transform interactorTransform)
    {
        if (interactorTransform.parent != null)
            return interactorTransform.parent.name;

        return interactorTransform.name;
    }

    public string GetGrabHandName()
    {
        return handName;
    }
}
