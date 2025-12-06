using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;

namespace Bosch.ESA.Procedures
{
    public class SceneDataManager : MonoBehaviour
    {
        public enum TrainingMode
        {
            GuidedMode,
            AssessmentMode,
            None
        }
        public TrainingMode selectedMode = TrainingMode.None;
        public static SceneDataManager instance;

        private void OnEnable()
        {
            if (null != instance)
            {
                Destroy(this);
                return;
            }
            instance = this;
            DontDestroyOnLoad(this);
        }

        public void TrainingModeSelected(int mode)
        {
            selectedMode = (TrainingMode)mode;
            SceneManager.LoadScene(1);
        }
    }
}
