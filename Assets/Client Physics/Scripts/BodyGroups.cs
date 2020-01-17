using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine
{
    /// <summary>
    /// A read-only collection of dictionaries to access different GameObject groups of the human body (e.g. all bones of the left arm).
    /// </summary> 
    public class BodyGroups
    {
        /// <summary>
        /// A group name to identify and group parts of the human body, these will not include HumanBodyBones that might not be mapped to an object in the scene (e.g. LeftEye).
        /// </summary>
        public enum BODYGROUP
        {
            ALL_COMBINED,
            TRUNK,
            TRUNK_HEAD,
            LEFT_LEG,
            LEFT_FOOT,
            RIGHT_LEG,
            RIGHT_FOOT,
            LEFT_ARM,
            LEFT_FINGERS,
            RIGHT_ARM,
            RIGHT_FINGERS,
            HEAD
        }

        Animator animator;

        Dictionary<HumanBodyBones, GameObject> allCombined = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> trunk = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> leftLeg = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> leftFoot = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> rightLeg = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> rightFoot = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> leftArm = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> leftFingers = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> rightArm = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> rightFingers = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> head = new Dictionary<HumanBodyBones, GameObject>();
        Dictionary<HumanBodyBones, GameObject> trunkHead = new Dictionary<HumanBodyBones, GameObject>();

        public BodyGroups(Animator animator)
        {
            this.animator = animator;
            //allCombined has to be assigned first!
            allCombined = GetGroupDictionary(BODYGROUP.ALL_COMBINED);
            SplitAllCombinedIntoGroups();
        }

        public BodyGroups(Dictionary<HumanBodyBones, GameObject> dictionary)
        {
            //allCombined has to be assigned first!
            allCombined = dictionary;
            SplitAllCombinedIntoGroups();
        }

        void SplitAllCombinedIntoGroups()
        {
            trunk = GetGroupDictionary(BODYGROUP.TRUNK);            
            leftFoot = GetGroupDictionary(BODYGROUP.LEFT_FOOT);
            rightFoot = GetGroupDictionary(BODYGROUP.RIGHT_FOOT);
            leftLeg = GetGroupDictionary(BODYGROUP.LEFT_LEG);
            rightLeg = GetGroupDictionary(BODYGROUP.RIGHT_LEG);
            leftArm = GetGroupDictionary(BODYGROUP.LEFT_ARM);
            rightArm = GetGroupDictionary(BODYGROUP.RIGHT_ARM);
            leftFingers = GetGroupDictionary(BODYGROUP.LEFT_FINGERS);
            rightFingers = GetGroupDictionary(BODYGROUP.RIGHT_FINGERS);
            head = GetGroupDictionary(BODYGROUP.HEAD);
            trunkHead = GetGroupDictionary(BODYGROUP.TRUNK_HEAD);
        }

        /// <summary>
        /// Get Dictionary that includes Hips, Spine, Chest, UpperChest and Shoulders
        /// </summary>   
        public Dictionary<HumanBodyBones, GameObject> Trunk()
        {
            return trunk;
        }
        /// <summary>
        /// Get Dictionary that includes Neck and Head
        /// </summary>
        public Dictionary<HumanBodyBones, GameObject> Head()
        {
            return head;
        }
        /// <summary>
        ///  Get Dictionary that includes Hips, Spine, Chest, UpperChest, Shoulders, Neck and Head
        /// </summary>  
        public Dictionary<HumanBodyBones, GameObject> TrunkHead()
        {
            return trunkHead;
        }
        /// <summary>
        /// Get Dictionary that includes LeftUpperLeg, LeftLowerLeg, LeftFoot and LeftToes
        /// </summary>
        public Dictionary<HumanBodyBones, GameObject> LeftLeg()
        {
            return leftLeg;
        }
        /// <summary>
        /// Get Dictionary that includes LeftFoot, LeftToes
        /// </summary>
        public Dictionary<HumanBodyBones, GameObject> LeftFoot()
        {
            return leftFoot;
        }
        /// <summary>    
        /// Get Dictionary that includes RightUpperLeg, RightLowerLeg, RightFoot and RightToes
        /// </summary>
        public Dictionary<HumanBodyBones, GameObject> RightLeg()
        {
            return rightLeg;
        }
        /// <summary>
        /// Get Dictionary that includes RightFoot and RightToes
        /// </summary>   
        public Dictionary<HumanBodyBones, GameObject> RightFoot()
        {
            return rightFoot;
        }
        /// <summary>
        /// Get Dictionary that includes LeftUpperArm, LeftLowerArm, LeftHand and fingers
        /// </summary>
        public Dictionary<HumanBodyBones, GameObject> LeftArm()
        {
            return leftArm;
        }
        /// <summary>
        ///  Get Dictionary that includes left fingers
        /// </summary>
        public Dictionary<HumanBodyBones, GameObject> LeftFingers()
        {
            return leftFingers;
        }
        /// <summary>
        /// Get Dictionary that includes RightUpperArm, RightLowerArm, RightHand and fingers
        /// </summary>
        public Dictionary<HumanBodyBones, GameObject> RightArm()
        {
            return rightArm;
        }
        /// <summary>
        /// Get Dictionary that includes left fingers
        /// </summary>  
        public Dictionary<HumanBodyBones, GameObject> RightFingers()
        {
            return rightFingers;
        }
        /// <summary>
        /// Get Dictionary that includes all assigned bones, returns parameter if BodyGroups(Dictionary<HumanBodyBones, GameObject> dictionary) has been used.
        /// </summary>
        public Dictionary<HumanBodyBones, GameObject> AllCombined()
        {
            return allCombined;
        }

        private Dictionary<HumanBodyBones, GameObject> GetGroupDictionary(BODYGROUP group)
        {
            Dictionary<HumanBodyBones, GameObject> dict = new Dictionary<HumanBodyBones, GameObject>();

            switch (group)
            {
                #region ALL_COMBINED
                case BODYGROUP.ALL_COMBINED:
                    #region TrunkHead
                    AddBoneToDictionary(HumanBodyBones.Hips, dict);
                    AddBoneToDictionary(HumanBodyBones.Spine, dict);
                    AddBoneToDictionary(HumanBodyBones.Chest, dict);
                    AddBoneToDictionary(HumanBodyBones.UpperChest, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftShoulder, dict);
                    AddBoneToDictionary(HumanBodyBones.RightShoulder, dict);

                    AddBoneToDictionary(HumanBodyBones.Neck, dict);
                    AddBoneToDictionary(HumanBodyBones.Head, dict);

                    //not needed for standard model, but might be assigned when using a different model
                    AddBoneToDictionary(HumanBodyBones.LeftEye, dict);
                    AddBoneToDictionary(HumanBodyBones.RightEye, dict);
                    AddBoneToDictionary(HumanBodyBones.Jaw, dict);
                    #endregion
                    #region LeftArm
                    AddBoneToDictionary(HumanBodyBones.LeftUpperArm, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftLowerArm, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftHand, dict);

                    AddBoneToDictionary(HumanBodyBones.LeftIndexDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftIndexIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftIndexProximal, dict);

                    AddBoneToDictionary(HumanBodyBones.LeftMiddleDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftMiddleIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftMiddleProximal, dict);

                    AddBoneToDictionary(HumanBodyBones.LeftRingDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftRingIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftRingProximal, dict);

                    AddBoneToDictionary(HumanBodyBones.LeftLittleDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftLittleIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftLittleProximal, dict);

                    AddBoneToDictionary(HumanBodyBones.LeftThumbDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftThumbIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftThumbProximal, dict);

                    #endregion
                    #region RightArm
                    AddBoneToDictionary(HumanBodyBones.RightUpperArm, dict);
                    AddBoneToDictionary(HumanBodyBones.RightLowerArm, dict);
                    AddBoneToDictionary(HumanBodyBones.RightHand, dict);

                    AddBoneToDictionary(HumanBodyBones.RightIndexDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.RightIndexIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.RightIndexProximal, dict);

                    AddBoneToDictionary(HumanBodyBones.RightMiddleDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.RightMiddleIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.RightMiddleProximal, dict);

                    AddBoneToDictionary(HumanBodyBones.RightRingDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.RightRingIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.RightRingProximal, dict);

                    AddBoneToDictionary(HumanBodyBones.RightLittleDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.RightLittleIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.RightLittleProximal, dict);

                    AddBoneToDictionary(HumanBodyBones.RightThumbDistal, dict);
                    AddBoneToDictionary(HumanBodyBones.RightThumbIntermediate, dict);
                    AddBoneToDictionary(HumanBodyBones.RightThumbProximal, dict);
                    #endregion
                    #region LeftLeg
                    AddBoneToDictionary(HumanBodyBones.LeftUpperLeg, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftLowerLeg, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftFoot, dict);
                    AddBoneToDictionary(HumanBodyBones.LeftToes, dict);
                    #endregion
                    #region RightLeg
                    AddBoneToDictionary(HumanBodyBones.RightUpperLeg, dict);
                    AddBoneToDictionary(HumanBodyBones.RightLowerLeg, dict);
                    AddBoneToDictionary(HumanBodyBones.RightFoot, dict);
                    AddBoneToDictionary(HumanBodyBones.RightToes, dict);
                    
                    break;
                #endregion
                #endregion
                #region TRUNK
                case BODYGROUP.TRUNK:
                    AddBoneFromAllCombined(HumanBodyBones.Hips, dict);
                    AddBoneFromAllCombined(HumanBodyBones.Spine, dict); 
                    AddBoneFromAllCombined(HumanBodyBones.Chest, dict); 
                    AddBoneFromAllCombined(HumanBodyBones.UpperChest, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftShoulder, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightShoulder, dict);
                    break;
                #endregion
                #region HEAD
                case BODYGROUP.HEAD:
                    AddBoneFromAllCombined(HumanBodyBones.Neck, dict);
                    AddBoneFromAllCombined(HumanBodyBones.Head, dict);

                    //not needed for standard model, but might be assigned when using a different model
                    AddBoneFromAllCombined(HumanBodyBones.LeftEye, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightEye, dict);
                    AddBoneFromAllCombined(HumanBodyBones.Jaw, dict);
                    break;
                #endregion
                #region LEFT_ARM
                case BODYGROUP.LEFT_ARM:
                    AddBoneFromAllCombined(HumanBodyBones.LeftUpperArm, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftLowerArm, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftHand, dict);
                    AddBodyGroupFromDictionary(BODYGROUP.LEFT_FINGERS, dict);
                    break;
                #endregion
                #region LEFT_FINGERS
                case BODYGROUP.LEFT_FINGERS:
                    AddBoneFromAllCombined(HumanBodyBones.LeftIndexDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftIndexIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftIndexProximal, dict);

                    AddBoneFromAllCombined(HumanBodyBones.LeftMiddleDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftMiddleIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftMiddleProximal, dict);

                    AddBoneFromAllCombined(HumanBodyBones.LeftRingDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftRingIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftRingProximal, dict);

                    AddBoneFromAllCombined(HumanBodyBones.LeftLittleDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftLittleIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftLittleProximal, dict);

                    AddBoneFromAllCombined(HumanBodyBones.LeftThumbDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftThumbIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftThumbProximal, dict);
                    break;
                #endregion
                #region RIGHT_ARM
                case BODYGROUP.RIGHT_ARM:
                    AddBoneFromAllCombined(HumanBodyBones.RightUpperArm, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightLowerArm, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightHand, dict);
                    AddBodyGroupFromDictionary(BODYGROUP.RIGHT_FINGERS, dict);
                    break;
                #endregion
                #region RIGHT_FINGERS
                case BODYGROUP.RIGHT_FINGERS:
                    AddBoneFromAllCombined(HumanBodyBones.RightIndexDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightIndexIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightIndexProximal, dict);

                    AddBoneFromAllCombined(HumanBodyBones.RightMiddleDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightMiddleIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightMiddleProximal, dict);

                    AddBoneFromAllCombined(HumanBodyBones.RightRingDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightRingIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightRingProximal, dict);

                    AddBoneFromAllCombined(HumanBodyBones.RightLittleDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightLittleIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightLittleProximal, dict);

                    AddBoneFromAllCombined(HumanBodyBones.RightThumbDistal, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightThumbIntermediate, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightThumbProximal, dict);
                    break;
                #endregion
                #region LEFT_LEG
                case BODYGROUP.LEFT_LEG:
                    AddBoneFromAllCombined(HumanBodyBones.LeftUpperLeg, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftLowerLeg, dict);
                    AddBodyGroupFromDictionary(BODYGROUP.LEFT_FOOT, dict);
                    break;
                #endregion
                #region LEFT_FOOT
                case BODYGROUP.LEFT_FOOT:
                    AddBoneFromAllCombined(HumanBodyBones.LeftFoot, dict);
                    AddBoneFromAllCombined(HumanBodyBones.LeftToes, dict);

                    break;
                #endregion
                #region RIGHT_LEG
                case BODYGROUP.RIGHT_LEG:
                    AddBoneFromAllCombined(HumanBodyBones.RightUpperLeg, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightLowerLeg, dict);
                    AddBodyGroupFromDictionary(BODYGROUP.RIGHT_FOOT, dict);

                    break;
                #endregion
                #region RIGHT_FOOT
                case BODYGROUP.RIGHT_FOOT:
                    AddBoneFromAllCombined(HumanBodyBones.RightFoot, dict);
                    AddBoneFromAllCombined(HumanBodyBones.RightToes, dict);

                    break;
                #endregion
                #region TRUNK_HEAD
                case BODYGROUP.TRUNK_HEAD:
                    AddBodyGroupFromDictionary(BODYGROUP.TRUNK, dict);
                    AddBodyGroupFromDictionary(BODYGROUP.HEAD, dict);
                    break;
                #endregion
                default: break;
            }
            return dict;
        }

        void AddBoneToDictionary(HumanBodyBones bone, Dictionary<HumanBodyBones, GameObject> dictionary)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            if(boneTransform != null && !dictionary.ContainsKey(bone))
            {
                dictionary.Add(bone, boneTransform.gameObject);
            }
        }

        void AddBoneFromAllCombined(HumanBodyBones bone, Dictionary<HumanBodyBones, GameObject> dictionary)
        {
            if (allCombined.ContainsKey(bone))
            {
                if (!dictionary.ContainsKey(bone))
                {
                    dictionary.Add(bone, allCombined[bone]);
                }
            }
        }

        void AddBodyGroupFromDictionary(BODYGROUP bodyGroup, Dictionary<HumanBodyBones, GameObject> dictionary)
        {
            Dictionary<HumanBodyBones, GameObject> tmp = GetGroupDictionary(bodyGroup);

            foreach (HumanBodyBones bone in tmp.Keys)
            {
                if (!dictionary.ContainsKey(bone))
                {
                    dictionary.Add(bone, tmp[bone]);
                }
            }
        }

        void PrintoutBone(Dictionary<HumanBodyBones, GameObject> dictionary)
        {
            string print = "";
            foreach (HumanBodyBones bone in dictionary.Keys)
            {
                print += bone.ToString() +", ";
            }
            Debug.Log(print);
        }
    }
}
