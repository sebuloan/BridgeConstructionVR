using Bosch.ESA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PPEProcedure : MonoBehaviour,IProcedure
{
    public int stepNumber;
    public string pickableObject;

    public event Action<IProcedure> ProcedureCompleted;

    public PPEManager ppeManager;
    public UIManager uiManager;
    public List<string> otherSafetyObjects = new List<string>();
    int totalCount = 0;
    int completedCount = 0;

    public void Execute()
    {
        completedCount = 0;
        totalCount = 0;
        uiManager.SwitchToPPEPanel(stepNumber);
        if (string.IsNullOrEmpty(pickableObject))
        {
            Debug.LogError($"PickProcedure (Step {stepNumber}): pickableObject is not assigned.");
            OnCompleted(); 
            return;
        }

        if (pickableObject != null && ppeManager != null)
        {
            totalCount++;
            ppeManager.HandleGlove(pickableObject, OnObjectPicked);
        }
        if(otherSafetyObjects.Count > 0)
        {
            totalCount++;
            ppeManager.HandleSafetyObjects(otherSafetyObjects, OnSafetyObject);
        }
        
        //uiManager.ShowText(null);
        if (totalCount == 0)
        {
            OnAllFunctionCompleted();
        }
    }

    private void OnObjectPicked()
    {        
        completedCount++;
        UnityEngine.Debug.Log("gloves picked complted");
        if (completedCount == totalCount)
        {
            OnAllFunctionCompleted();
        }
    }
    private void OnSafetyObject()
    {
        completedCount++;
        UnityEngine.Debug.Log("other safety object picked");
        if (completedCount == totalCount)
        {
            OnAllFunctionCompleted();
        }
    }
    public bool IsComplete()
    {
        throw new NotImplementedException();
    }
    public void OnAllFunctionCompleted()
    {
        OnCompleted();
    }
    public void OnCompleted()
    {   
        if (null != ProcedureCompleted)
        {
            ProcedureCompleted.Invoke(this);
        }
    }
 

    public void OnGameObjectStateSet()
    {
        //completedCount++;
        //if (completedCount == totalCount)
        {
            //OnAllFunctionCompleted();
            UnityEngine.Debug.Log("Object state is set");
        }
    }

    

    
    public void Stop()
    {
        
        StopAllCoroutines();
    }
}

