using UnityEngine;
using UnityEngine.Events;


namespace Bosch.ESA.Managers
{
    public class DropLocationManager : MonoBehaviour
    {
        public UnityEvent<GameObject> ObjectDroppedAtLocation;
        public bool IsAssembly;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable selectInteractable;
        private float _distance;
        private float _angle;
        private Color _ObjectColor;
        private bool _hasObjectEntered = false; // Track if an object has entered

        public DropLocationManager()
        {
            ObjectDroppedAtLocation = new UnityEvent<GameObject>();
            _ObjectColor = Color.white; // SwitchToConfimationPanel to white as a default
        }

        private void OnTriggerStay(Collider other)
        {
            if (selectInteractable != null)
            {
                if (IsAssembly)
                {
                    other.GetComponent<MeshRenderer>().material.color = Color.yellow;
                    CheckProximity(other.gameObject);
                    if (_distance <= 0.2f) // && _angle <= 5f)
                    {
                        other.GetComponent<MeshRenderer>().material.color = Color.green;

                        if (selectInteractable.isSelected == false) // Object has released in collider region (Drop Location)
                        {
                            other.GetComponent<MeshRenderer>().material.color = _ObjectColor;
                            other.transform.position = transform.position;
                            other.transform.rotation = transform.rotation;
                            ObjectDroppedAtLocation?.Invoke(other.gameObject);
                        }
                    }
                }
                else if (selectInteractable.isSelected == false) // Object has released in collider region (Drop Location)
                {
                    other.GetComponent<Rigidbody>().isKinematic = true; // Make sure the object is not affected by physics
                    other.transform.position = transform.position;
                    other.transform.rotation = transform.rotation;
                    ObjectDroppedAtLocation?.Invoke(other.gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            selectInteractable = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable>();
            MeshRenderer renderer = other.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                _ObjectColor = renderer.material.color;
                _hasObjectEntered = true;
            }
            else
            {
                _ObjectColor = Color.white; // Default if no renderer
                _hasObjectEntered = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            selectInteractable = null;
            if (_hasObjectEntered && other.GetComponent<MeshRenderer>() != null)
            {
                other.GetComponent<MeshRenderer>().material.color = _ObjectColor;
            }
            _ObjectColor = Color.white; // Reset for the next object
            _hasObjectEntered = false;
        }

        /// <summary>
        /// Check Distancr and angle between grabbedObject and snapPosition
        /// </summary>
        /// <param name="grabbedObject">The object being checked </param>
        public void CheckProximity(GameObject grabbedObject)
        {
            _distance = Vector3.Distance(transform.position, grabbedObject.transform.position);
            _angle = Quaternion.Angle(transform.rotation, grabbedObject.transform.rotation);
        }
    }
}