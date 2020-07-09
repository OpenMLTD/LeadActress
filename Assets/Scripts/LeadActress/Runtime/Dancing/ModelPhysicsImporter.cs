using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LeadActress.Runtime.Loaders;
using LeadActress.Runtime.Sway;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Model Physics Importer")]
    public class ModelPhysicsImporter : MonoBehaviour {

        public PhysicMaterial defaultPhysicMaterial;

        private void Awake() {
            _replaceCache = new StringReplaceCache();
        }

        public void ImportPhysics([NotNull] ModelLoadResult loadResult) {
            SwayAsset.FixSwayReferences(loadResult.HeadSway, loadResult.BodySway);

            var objectMap = new Dictionary<string, GameObject>();

            AddSwayColliders(loadResult.Body, loadResult.BodySway, objectMap);
            AddSwayColliders(loadResult.Head, loadResult.HeadSway, objectMap);

            AddSwayBones(loadResult.Body, loadResult.BodySway, objectMap);
            AddSwayBones(loadResult.Head, loadResult.HeadSway, objectMap);

            ProcessCollisionIgnores(loadResult, objectMap);
        }

        private void AddSwayColliders([NotNull] GameObject root, [NotNull] SwayController controller, [NotNull] Dictionary<string, GameObject> cache) {
            foreach (var swayCollider in controller.Colliders) {
                switch (swayCollider.Type) {
                    case ColliderType.Sphere:
                    case ColliderType.Capsule:
                        break;
                    case ColliderType.Plane: {
                        // Not supported for now
                        continue;
                    }
                    case ColliderType.Bridge: {
                        // ???
                        continue;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var colliderName = BreakPath(swayCollider.Path);
                var gameObject = SearchObjectByName(root, colliderName, cache);
                Debug.Assert(gameObject != null);

                switch (swayCollider.Type) {
                    case ColliderType.Sphere: {
                        var collider = gameObject.AddComponent<SphereCollider>();
                        collider.radius = swayCollider.Radius;
                        collider.center = swayCollider.Offset;
                        collider.material = defaultPhysicMaterial;

                        break;
                    }
                    case ColliderType.Capsule: {
                        var collider = gameObject.AddComponent<CapsuleCollider>();
                        collider.radius = swayCollider.Radius;
                        collider.center = swayCollider.Offset;
                        collider.height = swayCollider.Distance;
                        // https://docs.unity3d.com/ScriptReference/CapsuleCollider-direction.html
                        collider.direction = (int)swayCollider.Axis - 1;
                        collider.material = defaultPhysicMaterial;

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void AddSwayBones([NotNull] GameObject root, [NotNull] SwayController controller, [NotNull] Dictionary<string, GameObject> cache) {
            foreach (var swayManager in controller.Managers) {
                foreach (var bone in swayManager.RootBones) {
                    AddSwayBone(root, bone, cache);
                }
            }

            foreach (var swayManager in controller.Managers) {
                foreach (var bone in swayManager.RootBones) {
                    AddBoneLinks(root, bone, cache);
                }
            }
        }

        private void AddSwayBone([NotNull] GameObject root, [NotNull] SwayBone bone, [NotNull] Dictionary<string, GameObject> cache) {
            if (IsOppai(bone)) {
                return;
            }

            var bonePart = BreakPath(bone.Path);
            var targetObject = SearchObjectByName(root, bonePart, cache);
            Debug.Assert(targetObject != null);
            var targetObjectTransform = targetObject.transform;
            Debug.Assert(targetObject != null);

            var parent = bone.Parent;
            Rigidbody body1;

            if (parent == null) {
                // Try to add rigid body to its game object parent
                body1 = GetOrAddKinematicRigidBody(targetObjectTransform.parent.gameObject);
            } else {
                var parentObject = SearchObjectByName(root, BreakPath(parent.Path), cache);
                Debug.Assert(parentObject != null);
                body1 = parentObject.GetComponent<Rigidbody>();
            }

            Debug.Assert(targetObjectTransform.childCount == 1);
            var childTransform = targetObjectTransform.GetChild(0);
            var sphereCenter = childTransform.localPosition;
            float mass;
            if (bone.IsSkirt) {
                mass = 0.2f;
            } else {
                // Only layer 0 bones are marked IsSkirt=true
                if (IsOnSkirtChain(bone)) {
                    mass = 0.1f;
                } else {
                    mass = 1;
                }
            }

            var body2 = AddRigidBody(targetObject, sphereCenter, mass);

            if (bonePart.Contains("hair_")) {
                body2.drag = 1;
            }

            Debug.Assert(bone.Type == ColliderType.Sphere);
            if (bonePart.Contains("hair_")) {
                var collider = targetObject.AddComponent<CapsuleCollider>();
                collider.radius = bone.Radius;
                collider.direction = 0; // X-axis
                collider.height = (childTransform.position - targetObjectTransform.position).magnitude;
                collider.center = sphereCenter;
                collider.material = defaultPhysicMaterial;
            } else {
                var collider = targetObject.AddComponent<SphereCollider>();
                collider.radius = bone.Radius;
                collider.center = sphereCenter;

                // skirts should be slippery so don't use default phys mat
                if (!bone.IsSkirt) {
                    collider.material = defaultPhysicMaterial;
                }
            }

            // Add a joint
            var joint = targetObject.AddComponent<ConfigurableJoint>();
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            // Don't twist
            joint.angularXMotion = ConfigurableJointMotion.Limited;

            joint.connectedBody = body1;

            foreach (var childBone in bone.Children) {
                AddSwayBone(root, childBone, cache);
            }
        }

        private static void ProcessCollisionIgnores([NotNull] ModelLoadResult loadResult, [NotNull] Dictionary<string, GameObject> cache) {
            var objectName = loadResult.Body.name;

            {
                // Hair nodes
                // Having hair colliding with each other part is more natural.
                // But hair collision avoiding is necessary for 045kar. It does not make much difference when applying to the others.
                var headT = loadResult.Head.transform;
                var hairT = headT.Find("KUBI/ATAMA/HAIR");

                if (hairT == null) {
                    Debug.LogWarning($"OMG there is no hair! ({objectName})");
                }

                var hair = hairT.gameObject;

                // We generate capsules instead of spheres on hair nodes
                var colliders = hair.GetComponentsInChildren<CapsuleCollider>();

                Debug.Assert(colliders.Length > 0, $"OMG there is no collider on the hair! ({objectName})");

                IgnoreColliderMatrix(colliders);
            }

            {
                // Skirt nodes
                var bodyT = loadResult.Body.transform;
                var skirtGpT = bodyT.Find("MODEL_00/BODY_SCALE/BASE/KOSHI/skrt_GP");

                if (skirtGpT == null) {
                    skirtGpT = bodyT.Find("MODEL_00/BODY_SCALE/BASE/KOSHI/skirt_GP");
                }

                if (skirtGpT == null) {
                    Debug.Log($"OMG there is no skirt! ({objectName})");
                }

                var skirtGp = skirtGpT.gameObject;

                var colliders = skirtGp.GetComponentsInChildren<SphereCollider>();

                // For now we simply ignore all collisions in all skirt bodies
                IgnoreColliderMatrix(colliders);
            }
        }

        private static void IgnoreColliderMatrix([NotNull, ItemNotNull] Collider[] colliders) {
            for (var i = 0; i < colliders.Length; i += 1) {
                var c1 = colliders[i];
                for (var j = 1; j < colliders.Length; j += 1) {
                    var c2 = colliders[j];
                    Physics.IgnoreCollision(c1, c2);
                }
            }
        }

        private static bool IsOnSkirtChain([NotNull] SwayBone bone) {
            Debug.Assert(!bone.IsSkirt);

            var b = bone;
            while (b.Parent != null) {
                b = b.Parent;

                if (b.IsSkirt) {
                    return true;
                }
            }

            return false;
        }

        private static void AddBoneLinks([NotNull] GameObject root, [NotNull] SwayBone bone, [NotNull] Dictionary<string, GameObject> cache) {
            if (IsOppai(bone)) {
                return;
            }

            var sideLink = bone.SideLink;

            if (sideLink != null) {
                var thisObject = SearchObjectByName(root, BreakPath(bone.Path), cache);
                Debug.Assert(thisObject != null);
                var thatObject = SearchObjectByName(root, BreakPath(sideLink.Path), cache);
                Debug.Assert(thatObject != null);
                var thisBody = thisObject.GetComponent<Rigidbody>();
                var thatBody = thatObject.GetComponent<Rigidbody>();
                // Don't use SpringJoint on this
                var joint = thisObject.AddComponent<HingeJoint>();
                // joint.tolerance = bone.SideSpringTolerance;
                joint.connectedBody = thatBody;

                var thisTransform = thisBody.transform;
                var thatTransform = thatBody.transform;
                var midPoint = (thisTransform.position + thatTransform.position) / 2;
                var toLocal = thisTransform.worldToLocalMatrix;
                joint.anchor = toLocal * midPoint;

                var limit = new JointLimits();
                limit.max = 180;
                limit.min = 144;
                joint.limits = limit;
            }

            foreach (var childBone in bone.Children) {
                AddBoneLinks(root, childBone, cache);
            }
        }

        private static bool IsOppai([NotNull] SwayBone bone) {
            return bone.Path.EndsWith("OPAI_L0") || bone.Path.EndsWith("OPAI_R0");
        }

        private static Rigidbody GetOrAddKinematicRigidBody([NotNull] GameObject obj) {
            if (!obj.TryGetComponent<Rigidbody>(out var rigidBody)) {
                rigidBody = obj.AddComponent<Rigidbody>();
                rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidBody.isKinematic = true;
            }

            return rigidBody;
        }

        private static Rigidbody AddRigidBody([NotNull] GameObject obj, Vector3 position, float mass) {
            var rigidBody = obj.AddComponent<Rigidbody>();

            rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidBody.mass = mass;
            rigidBody.position = position;

            return rigidBody;
        }

        [CanBeNull]
        private static GameObject SearchObjectByName([NotNull] GameObject root, [NotNull] string name, [NotNull] Dictionary<string, GameObject> cache) {
            if (cache.ContainsKey(name)) {
                return cache[name];
            }

            var queue = new Queue<GameObject>();
            queue.Enqueue(root);

            while (queue.Count > 0) {
                var o = queue.Dequeue();

                if (o.name == name) {
                    cache.Add(name, o);
                    return o;
                }

                var t = o.transform;
                var count = t.childCount;

                for (var i = 0; i < count; i += 1) {
                    var c = t.GetChild(i);
                    queue.Enqueue(c.gameObject);
                }
            }

            return null;
        }

        [NotNull]
        private static string BreakLast([NotNull] string str, char ch) {
            var index = str.LastIndexOf(ch);

            if (index >= 0) {
                return str.Substring(index + 1);
            } else {
                return str;
            }
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string BreakPath([NotNull] string str) {
            return BreakLast(str, '/');
        }

        private static bool IsPathInHead([NotNull] string path) {
            return path.Contains("KUBI/ATAMA/");
        }

        private StringReplaceCache _replaceCache;

        private sealed class StringReplaceCache {

            public StringReplaceCache() {
                _map = new Dictionary<string, string>();
            }

            [NotNull]
            public string Get([NotNull] string str) {
                if (_map.ContainsKey(str)) {
                    return _map[str];
                }

                if (IsPathInHead(str)) {
                    var s = str.Replace("KUBI/ATAMA/", "KUBI/ATAMA/" + ModelLoader.CharaHeadObjectName + "/");
                    _map.Add(str, s);
                    return s;
                } else {
                    return str;
                }
            }

            [NotNull]
            private readonly Dictionary<string, string> _map;

        }

    }
}
