using System;
using BepInEx;
using UnityEngine;
// Object çakışmasını engellemek için takma ad tanımlıyoruz:
using Object = UnityEngine.Object;

namespace OneManCoOp
{
    [BepInPlugin("com.onemancoop.peakmod", "OneManCoOp", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K) && !this.isCloneSpawned)
            {
                this.SpawnDummyClone();
            }
            if (this.isCloneSpawned && this.cloneDummy != null && this.mainPlayer != null)
            {
                Vector3 position = this.mainPlayer.transform.position + this.mainPlayer.transform.right * 1.5f;
                this.cloneDummy.transform.position = position;
                this.cloneDummy.transform.rotation = this.mainPlayer.transform.rotation;
            }
        }

        private void LateUpdate()
        {
            if (this.isCloneSpawned && this.cloneDummy != null && this.mainPlayer != null)
            {
                this.SyncBones(this.mainPlayer.transform, this.cloneDummy.transform);
                if (this.mainAnimator != null && this.cloneAnimator != null)
                {
                    for (int i = 0; i < this.mainAnimator.parameterCount; i++)
                    {
                        AnimatorControllerParameter parameter = this.mainAnimator.GetParameter(i);
                        if (parameter.type == AnimatorControllerParameterType.Float)
                        {
                            this.cloneAnimator.SetFloat(parameter.nameHash, this.mainAnimator.GetFloat(parameter.nameHash));
                        }
                        else if (parameter.type == AnimatorControllerParameterType.Bool)
                        {
                            this.cloneAnimator.SetBool(parameter.nameHash, this.mainAnimator.GetBool(parameter.nameHash));
                        }
                        else if (parameter.type == AnimatorControllerParameterType.Int)
                        {
                            this.cloneAnimator.SetInteger(parameter.nameHash, this.mainAnimator.GetInteger(parameter.nameHash));
                        }
                    }
                }
            }
        }

        private void SpawnDummyClone()
        {
            this.mainPlayer = ((Character.localCharacter != null) ? Character.localCharacter : Object.FindObjectOfType<Character>());
            if (this.mainPlayer == null)
            {
                this.statusMessage = "HATA: Ana oyuncu bulunamadi!";
                return;
            }
            this.mainAnimator = this.mainPlayer.GetComponentInChildren<Animator>();
            Vector3 position = this.mainPlayer.transform.position + this.mainPlayer.transform.right * 1.5f;
            this.cloneDummy = Object.Instantiate<GameObject>(this.mainPlayer.gameObject, position, this.mainPlayer.transform.rotation);
            this.cloneAnimator = this.cloneDummy.GetComponentInChildren<Animator>();
            foreach (MonoBehaviour monoBehaviour in this.cloneDummy.GetComponentsInChildren<MonoBehaviour>())
            {
                if (monoBehaviour != null && !(monoBehaviour is Plugin))
                {
                    Object.Destroy(monoBehaviour);
                }
            }
            AudioListener componentInChildren = this.cloneDummy.GetComponentInChildren<AudioListener>();
            if (componentInChildren != null)
            {
                Object.Destroy(componentInChildren);
            }
            Camera componentInChildren2 = this.cloneDummy.GetComponentInChildren<Camera>();
            if (componentInChildren2 != null)
            {
                Object.Destroy(componentInChildren2);
            }
            foreach (Rigidbody rigidbody in this.cloneDummy.GetComponentsInChildren<Rigidbody>(true))
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }
            foreach (Collider collider in this.cloneDummy.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = true;
                collider.isTrigger = false;
            }
            this.isCloneSpawned = true;
            this.statusMessage = "KUKLA SENKRONIZE EDILDI!";
            base.Logger.LogInfo(this.statusMessage);
        }

        private void SyncBones(Transform source, Transform target)
        {
            for (int i = 0; i < source.childCount; i++)
            {
                if (i < target.childCount)
                {
                    Transform child = source.GetChild(i);
                    Transform child2 = target.GetChild(i);
                    child2.localPosition = child.localPosition;
                    child2.localRotation = child.localRotation;
                    if (child.childCount > 0)
                    {
                        this.SyncBones(child, child2);
                    }
                }
            }
        }

        private void OnGUI()
        {
        }

        private Character mainPlayer;
        private GameObject cloneDummy;
        private Animator mainAnimator;
        private Animator cloneAnimator;
        private bool isCloneSpawned;
        private string statusMessage = "Mod Hazir - Kukla Spawn icin 'K' tusuna basin.";
    }
}