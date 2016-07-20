using UnityEngine;
using System.Collections;

public class TestingMovementScript : MonoBehaviour {

    public Transform MainHead;
    public Transform MainNeck;

    public Transform RefHead;
    public Transform RefNeck;

    public Transform HeadObject;


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        MainHead.position = HeadObject.position;
  /*      Debug.Log("MainHeadPos : " + MainHead.localPosition);
        Debug.Log("RefHeadPos  : " + RefHead.localPosition);
        Debug.Log("RefNeckPos : " + RefNeck.localPosition);*/
        MainNeck.localPosition = RefHead.localPosition - (RefHead.localPosition - RefNeck.localPosition);
    }
}

/*
 *         Vector3 scaleOffset  = new Vector3 (-0.0994701f, 0.0994701f, 0.0994701f);
        if (!ScaleAdjustingSkeleton)
        {
            MainHeadLocation.position = ViveHMD.position;
        }
        foreach (int joint in Joints.GetValues(typeof(Joints))){
            if (ScaleAdjustingSkeleton)
            {
                newJointList[joint].localPosition = new Vector3(inputJointList[joint].localPosition.x * scaleOffset.x, inputJointList[joint].localPosition.y * scaleOffset.y, inputJointList[joint].localPosition.z * scaleOffset.z);                             // Update our new skeleon with the position of the old skeleton each frame
                newJointList[joint].localRotation = new Quaternion(inputJointList[joint].localRotation.x, -inputJointList[joint].localRotation.y, -inputJointList[joint].localRotation.z, inputJointList[joint].localRotation.w);
            }
            else
            {
                if(joint.ToString() != "Head")
                {
                    newJointList[joint].localPosition = MainHeadLocation.localPosition - (ReferenceHeadLocation.localPosition - inputJointList[joint].localPosition);
                    //newJointList[joint].localPosition = inputJointList[joint].localPosition;                             // Update our new skeleon with the position of the old skeleton each frame
                }
                newJointList[joint].localRotation = inputJointList[joint].localRotation;
            }
        }*
*/
