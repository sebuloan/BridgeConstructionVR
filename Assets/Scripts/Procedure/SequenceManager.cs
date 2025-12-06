using Bosch.ESA.Procedures;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Bosch.ESA.Managers
{
    public interface IProcedure
    {
        void Execute();
        void OnCompleted();
        bool IsComplete();
        void Stop();
        event Action<IProcedure> ProcedureCompleted;
    }

    [System.Serializable]
    public class Sequence
    {
        public List<StepData> procedures;
    }

    [System.Serializable]
    public class GenericObjectData
    {
        public string genericObjectParent;
        public string gameObjectName;
        public bool isSelected;
    }

    [System.Serializable]
    public class ClimbInteractorObject 
    {
        public string climbInteractorgameObject;
        public bool isEnabled;
    }

    [System.Serializable]
    public class StepData
    {
        public int stepNumber;
        public string procedureType;
        public string gameObject;
        public string dropLocation;
        public string collidingObject;
        public string impactedObject;
        public string particleSystemObject;
        public AnimationData animations;
        public string teleportParent;
        public List<GenericObjectData> genericObjects;
        public List<string> highlightGameObject;
        public List<string> stepDescriptionAudioClips;
        public string stepInteractionAudioClip;
        public List<string> texts;
        public List<string> videoClips;
        public string glovesObject;
        public List<string> otherSafetyObjects;//= new List<string>();
        public bool retainInHands;
        public bool assembly;
        public int delay;
        public bool userConfirmation;
        public string confirmationText;
        public bool triggerMode;
        public bool isHandCollision;
        public string pickObject;
        public string hookAttachTarget;
        public List<ClimbInteractorObject> climbInteractors; 
    }

    [System.Serializable]
    public class AnimationData
    {
        public string gameObjectName;
        public string animationClipName;
        public float startFrame;
        public float endFrame;
    }

    public class SequenceManager : MonoBehaviour
    {
        public TextAsset sequenceJson;
      public TMP_Text debugText;
        public List<IProcedure> procedures = new List<IProcedure>();

        [Header("Generic Managers")]
        public AnimationManager animationManager;
        public AudioManager audioManager;
        public HighlightingManager highlightingManager;
        public UIManager uiManager;
        public TeleportManager teleportManager;
        public CollisionManager collisionManager;
        public VideoManager videoManager;
        public GenericObjectManager genericObjectManager;
        public ParticleSystemManager particleSystemManager;
        public PPEManager pPEManager;
        [Header("Procedure Managers")]
        public PickManager pickManager;
        public PickAndDropManager pickAndDropManager;
        public DropManager dropManager;
        public HookInteractionManager hookInteractionManager;

        [Space]
        private int currentProcedureIndex = 0;
        public AudioSource completionAudio;
        public AudioClip completionAudioClip;
      //  public UserConfirmationManager userConfirmationManager;
        public DelayManager delayManager;

        private float sequenceStartTime;
        private float sequenceEndTime;
        private float totalSequenceTime;

        void Start()
        {
            if (null != SceneDataManager.instance && SceneDataManager.instance.selectedMode == SceneDataManager.TrainingMode.AssessmentMode)
            {
              //  uiManager.ToggleSequenceInfoPanel();
                highlightingManager.EnableOutline = false; // Disable highlighting in assessment mode
                teleportManager.EnableAllTeleportPoints();
            }
            LoadSequence(sequenceJson.text);
            StartSequence();
        }

        public void LoadSequence(string json)
        {
            Sequence sequenceData = JsonUtility.FromJson<Sequence>(json);

            if (sequenceData != null && sequenceData.procedures != null)
            {
                debugText.text = "Loaded " + sequenceData.procedures.Count + " procedures.";
                Debug.Log("Loaded " + sequenceData.procedures.Count + " procedures.");

                foreach (var stepData in sequenceData.procedures)
                {
                    string procedureType = (string)stepData.procedureType;

                    IProcedure procedure = null;

                    switch (procedureType)
                    {
                        case "PickAndDrop":
                            procedure = CreatePickAndDropProcedure(stepData);
                            break;
                        case "Pick":
                            procedure = CreatePickProcedure(stepData);
                            break;
                        case "Animation":
                            procedure = CreateAnimationProcedure(stepData);
                            break;
                        case "Drop":
                            procedure = CreateDropProcedure(stepData);
                            break;
                        case "Teleport":
                            procedure = CreateTeleportProcedure(stepData);
                            break;
                        case "General":
                            procedure = CreateGeneralProcedure(stepData);
                            break;
                        case "CollisionType":
                            procedure = CreateCollisionProcedure(stepData);
                            break; 
                        case "SafetyMeasurement":
                            procedure = CreateSafetyProcedure(stepData);
                            break;
                        case "HookInteraction":
                            procedure = CreateHookInteractionProcedure(stepData);
                            break;
                        default:
                            Debug.LogError($"Unknown procedure type: {procedureType}");
                            break;
                    }

                    if (procedure != null)
                    {
                        procedures.Add(procedure);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Failed to deserialize JSON.");
            }
        }

        private IProcedure CreatePickAndDropProcedure(StepData stepData)
        {
            PickAndDropProcedure pickAndDropProcedure = new PickAndDropProcedure();
            pickAndDropProcedure.stepNumber = stepData.stepNumber;
            pickAndDropProcedure.pickObject = stepData.gameObject;
            pickAndDropProcedure.dropLocation = stepData.dropLocation;
            pickAndDropProcedure.animations = stepData.animations;
            pickAndDropProcedure.highlightGameObject = stepData.highlightGameObject;
            pickAndDropProcedure.stepDescriptionAudioClips = stepData.stepDescriptionAudioClips;
            pickAndDropProcedure.stepInteractionAudioClip = stepData.stepInteractionAudioClip;
            pickAndDropProcedure.genericObjects = stepData.genericObjects;
            pickAndDropProcedure.texts = stepData.texts;
            pickAndDropProcedure.videoClips = stepData.videoClips;
            pickAndDropProcedure.retainInHands = stepData.retainInHands;
            pickAndDropProcedure.assembly = stepData.assembly;
            pickAndDropProcedure.delay = stepData.delay;
            pickAndDropProcedure.userConfirmation = stepData.userConfirmation;
            pickAndDropProcedure.confirmationText = stepData.confirmationText;
            pickAndDropProcedure.pickAndDropManager = pickAndDropManager;
            pickAndDropProcedure.animationManager = animationManager;
            pickAndDropProcedure.highlightingManager = highlightingManager;
            pickAndDropProcedure.audioManager = audioManager;
            pickAndDropProcedure.uiManager = uiManager;
            pickAndDropProcedure.delayManager = delayManager;
            pickAndDropProcedure.videoManager = videoManager;
            //pickAndDropProcedure.userConfirmationManager = userConfirmationManager;
            pickAndDropProcedure.genericObjectManager = genericObjectManager;
            return pickAndDropProcedure;
        }

        private IProcedure CreatePickProcedure(StepData stepData)
        {
            PickProcedure pickProcedure = new PickProcedure();
            pickProcedure.stepNumber = stepData.stepNumber;
            pickProcedure.pickableObject = stepData.gameObject;
            pickProcedure.animations = stepData.animations;
            pickProcedure.highlightGameObjects = stepData.highlightGameObject;
            pickProcedure.genericObjects = stepData.genericObjects;
            pickProcedure.stepDescriptionAudioClips = stepData.stepDescriptionAudioClips;
            pickProcedure.stepInteractionAudioClip = stepData.stepInteractionAudioClip;
            pickProcedure.texts = stepData.texts;
            pickProcedure.videoClips = stepData.videoClips;
            pickProcedure.retainInHands = stepData.retainInHands;
            pickProcedure.delay = stepData.delay;
            pickProcedure.userConfirmation = stepData.userConfirmation;
            pickProcedure.confirmationText = stepData.confirmationText;
            pickProcedure.pickManager = pickManager;
            pickProcedure.animationManager = animationManager;
            pickProcedure.highlightingManager = highlightingManager;
            pickProcedure.audioManager = audioManager;
            pickProcedure.uiManager = uiManager;
            pickProcedure.delayManager = delayManager;
            pickProcedure.videoManager = videoManager;
          //  pickProcedure.userConfirmationManager = userConfirmationManager;
            pickProcedure.genericObjectManager = genericObjectManager;
            return pickProcedure;
        }

        private IProcedure CreateDropProcedure(StepData stepData)
        {
            DropProcedure dropProcedure = new DropProcedure();
            dropProcedure.stepNumber = stepData.stepNumber;
            dropProcedure.objectToDrop = stepData.gameObject;
            dropProcedure.dropLocation = stepData.dropLocation;
            dropProcedure.animations = stepData.animations;
            dropProcedure.highlightGameObject = stepData.highlightGameObject;
            dropProcedure.stepDescriptionAudioClips = stepData.stepDescriptionAudioClips;
            dropProcedure.stepInteractionAudioClip = stepData.stepInteractionAudioClip;
            dropProcedure.texts = stepData.texts;
            dropProcedure.videoClips = stepData.videoClips;
            dropProcedure.retainInHands = stepData.retainInHands;
            dropProcedure.delay = stepData.delay;
            dropProcedure.genericObjects = stepData.genericObjects;
            dropProcedure.userConfirmation = stepData.userConfirmation;
            dropProcedure.confirmationText = stepData.confirmationText;
            dropProcedure.animationManager = animationManager;
            dropProcedure.highlightingManager = highlightingManager;
            dropProcedure.audioManager = audioManager;
            dropProcedure.uiManager = uiManager;
            dropProcedure.delayManager = delayManager;
            dropProcedure.videoManager = videoManager;
           // dropProcedure.userConfirmationManager = userConfirmationManager;
            dropProcedure.dropManager = dropManager;
            dropProcedure.genericObjectManager = genericObjectManager;
            return dropProcedure;
        }

        private IProcedure CreateCollisionProcedure(StepData stepData)
        {
            CollisionProcedure collisionProcedure = new CollisionProcedure();
            collisionProcedure.collisionManager = collisionManager;
            collisionProcedure.highlightingManager = highlightingManager;
            collisionProcedure.videoManager = videoManager;
            collisionProcedure.audioManager = audioManager;
            collisionProcedure.highlightGameObject = stepData.highlightGameObject;
            collisionProcedure.uiManager = uiManager;
            collisionProcedure.genericObjects = stepData.genericObjects;
            collisionProcedure.stepNumber = stepData.stepNumber;
            collisionProcedure.collidingGameObject = stepData.collidingObject;
            collisionProcedure.impactedGameObject = stepData.impactedObject;
            collisionProcedure.stepDescriptionAudioClips = stepData.stepDescriptionAudioClips;
            collisionProcedure.stepInteractionAudioClip = stepData.stepInteractionAudioClip;
            collisionProcedure.texts = stepData.texts;
            collisionProcedure.delay = stepData.delay;
            collisionProcedure.triggerMode = stepData.triggerMode;
            collisionProcedure.userConfirmation = stepData.userConfirmation;
            collisionProcedure.confirmationText = stepData.confirmationText;
            collisionProcedure.videoClips = stepData.videoClips;
            collisionProcedure.delayManager = delayManager;
            collisionProcedure.isHandCollision = stepData.isHandCollision;
          //  collisionProcedure.userConfirmationManager = userConfirmationManager;
            collisionProcedure.genericObjectManager = genericObjectManager;
            return collisionProcedure;
        }

        private IProcedure CreateAnimationProcedure(StepData stepData)
        {
            AnimationProcedure animationProcedure = new AnimationProcedure();
            animationProcedure.stepNumber = stepData.stepNumber;
            animationProcedure.animations = stepData.animations;
            animationProcedure.highlightGameObject = stepData.highlightGameObject;
            animationProcedure.genericObjects = stepData.genericObjects;
            animationProcedure.stepDescriptionAudioClips = stepData.stepDescriptionAudioClips;
            animationProcedure.stepInteractionAudioClip = stepData.stepInteractionAudioClip;
            animationProcedure.texts = stepData.texts;
            animationProcedure.delay = stepData.delay;
            animationProcedure.userConfirmation = stepData.userConfirmation;
            animationProcedure.confirmationText = stepData.confirmationText;
            animationProcedure.animationManager = animationManager;
            animationProcedure.highlightingManager = highlightingManager;
            animationProcedure.audioManager = audioManager;
            animationProcedure.uiManager = uiManager;
            animationProcedure.delayManager = delayManager;
            animationProcedure.videoManager = videoManager;
            animationProcedure.genericObjectManager = genericObjectManager;
           // animationProcedure.userConfirmationManager = userConfirmationManager;
            return animationProcedure;
        }

        private IProcedure CreateTeleportProcedure(StepData stepData)
        {
            TeleportProcedure teleportProcedure = new TeleportProcedure();
            teleportProcedure.stepNumber = stepData.stepNumber;
            teleportProcedure.gameObjectName = stepData.gameObject;
            teleportProcedure.highlightGameObject = stepData.highlightGameObject;
            teleportProcedure.teleportParent = stepData.teleportParent;
            teleportProcedure.genericObjects = stepData.genericObjects;
            teleportProcedure.stepDescriptionAudioClips = stepData.stepDescriptionAudioClips;
            teleportProcedure.stepInteractionAudioClip = stepData.stepInteractionAudioClip;
            teleportProcedure.texts = stepData.texts;
            teleportProcedure.delay = stepData.delay;
            teleportProcedure.userConfirmation = stepData.userConfirmation;
            teleportProcedure.confirmationText = stepData.confirmationText;
            teleportProcedure.teleportManager = teleportManager;
            teleportProcedure.highlightingManager = highlightingManager;
            teleportProcedure.audioManager = audioManager;
            teleportProcedure.uiManager = uiManager;
            teleportProcedure.delayManager = delayManager;
            teleportProcedure.genericObjectManager = genericObjectManager;
          //  teleportProcedure.userConfirmationManager = userConfirmationManager;
            return teleportProcedure;
        }

        private IProcedure CreateGeneralProcedure(StepData stepData)
        {
            GeneralProcedure generalProcedure = new GeneralProcedure();
            generalProcedure.audioManager = audioManager;
            generalProcedure.stepNumber = stepData.stepNumber;
            generalProcedure.stepDescriptionAudioClips = stepData.stepDescriptionAudioClips;
            generalProcedure.stepInteractionAudioClip = stepData.stepInteractionAudioClip;
            generalProcedure.texts = stepData.texts;
            generalProcedure.videoClips = stepData.videoClips;
            generalProcedure.delay = stepData.delay;
            generalProcedure.userConfirmation = stepData.userConfirmation;
            generalProcedure.confirmationText = stepData.confirmationText;
            generalProcedure.uiManager = uiManager;
            generalProcedure.genericObjects = stepData.genericObjects;
            generalProcedure.particleSystemManager = particleSystemManager;
            generalProcedure.genericObjectManager = genericObjectManager;
            generalProcedure.delayManager = delayManager;
            generalProcedure.videoManager = videoManager;
           // generalProcedure.userConfirmationManager = userConfirmationManager;
            return generalProcedure;
        }
        private IProcedure CreateHookInteractionProcedure(StepData stepData)
        {
            HookProcedure hookInteractionProcedure = new HookProcedure();
            hookInteractionProcedure.stepNumber = stepData.stepNumber;
            hookInteractionProcedure.pickObject = stepData.pickObject;
            hookInteractionProcedure.hookTargetLocation = stepData.hookAttachTarget;
            hookInteractionProcedure.climbInteractors = stepData.climbInteractors;
            hookInteractionProcedure.animations = stepData.animations;
            hookInteractionProcedure.highlightGameObject = stepData.highlightGameObject;
            hookInteractionProcedure.stepDescriptionAudioClips = stepData.stepDescriptionAudioClips;
            hookInteractionProcedure.stepInteractionAudioClip = stepData.stepInteractionAudioClip;
            hookInteractionProcedure.genericObjects = stepData.genericObjects;
            hookInteractionProcedure.texts = stepData.texts;
            hookInteractionProcedure.videoClips = stepData.videoClips;
            hookInteractionProcedure.retainInHands = stepData.retainInHands;
            hookInteractionProcedure.delay = stepData.delay;
            hookInteractionProcedure.userConfirmation = stepData.userConfirmation;
            hookInteractionProcedure.confirmationText = stepData.confirmationText;
            hookInteractionProcedure.hookInteractionManager = hookInteractionManager;
            hookInteractionProcedure.animationManager = animationManager;
            hookInteractionProcedure.highlightingManager = highlightingManager;
            hookInteractionProcedure.audioManager = audioManager;
            hookInteractionProcedure.uiManager = uiManager;
            hookInteractionProcedure.delayManager = delayManager;
            hookInteractionProcedure.videoManager = videoManager;
            //pickAndDropProcedure.userConfirmationManager = userConfirmationManager;
            hookInteractionProcedure.genericObjectManager = genericObjectManager;
            return hookInteractionProcedure;
        }

        private IProcedure CreateSafetyProcedure(StepData stepData)
        {
            PPEProcedure ppeProcedure = new PPEProcedure();
            ppeProcedure.stepNumber = stepData.stepNumber;
            ppeProcedure.ppeManager = pPEManager;
            ppeProcedure.pickableObject = stepData.glovesObject;
            ppeProcedure.otherSafetyObjects = stepData.otherSafetyObjects;
            ppeProcedure.uiManager = uiManager;
            return ppeProcedure;
        }

        // Modify StartSequence to start the timer
        public void StartSequence()
        {
            sequenceStartTime = Time.time;
            currentProcedureIndex = 0;
            ExecuteCurrentProcedure();
        }

        // At the end of the sequence, calculate and display the total time
        private void ExecuteCurrentProcedure()
        {
            if (currentProcedureIndex < procedures.Count)
            {
                var currentProcedure = procedures[currentProcedureIndex];
                currentProcedure.ProcedureCompleted += OnProcedureCompletedHandler;
                Debug.Log("Starting the procedure : " + procedures[currentProcedureIndex].GetType());
                debugText.text += Environment.NewLine;
                debugText.text += Environment.NewLine;
                debugText.text += "Starting the procedure : " + procedures[currentProcedureIndex].GetType();
                currentProcedure.Execute();
            }
            else
            {
                sequenceEndTime = Time.time;
                totalSequenceTime = sequenceEndTime - sequenceStartTime;
                debugText.text += Environment.NewLine;
                debugText.text += Environment.NewLine;
                debugText.text += "Sequence Completed!";
                debugText.text += Environment.NewLine;
                debugText.text += $"Total Time Spent: {totalSequenceTime:F2} seconds";
                Debug.Log("Sequence Completed!");
                Debug.Log($"Total Time Spent: {totalSequenceTime:F2} seconds");
            }
        }

        private void OnProcedureCompletedHandler(IProcedure completedProcedure)
        {
            debugText.text += Environment.NewLine;
            debugText.text += "Completed procedure : " + completedProcedure.GetType();
            completionAudio.clip = completionAudioClip;
            
            uiManager.ShowText(null);
            uiManager.SetSequenceInfoPanel();
            completionAudio.Play();
            Debug.Log("Completed procedure : " + completedProcedure.GetType());
            completedProcedure.ProcedureCompleted -= OnProcedureCompletedHandler; // Unsubscribe from the event
            currentProcedureIndex++;

            StartCoroutine(ExecuteNext());
        }
        private IEnumerator ExecuteNext()
        {
            yield return new WaitForSeconds(1.5f);
            ExecuteCurrentProcedure();
        }
    }
}