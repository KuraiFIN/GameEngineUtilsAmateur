//Another cringe file, derp

using System.Collections;
using System.Collections.Generic;

namespace CSL_Engine.Theta.World
{
    public class Action
    {
        public string type; //simple, dynamic
        public SimpleAction Simple;
        public DynamicAction Dynamic;

        public void PlayBlind()
        {
            if (isSimple)
            {
                Simple.frame += 0.1f;//Time.deltaTime;
                if (Simple.frame >= Simple.keyframes[Simple.keyframes.Length - 1])
                {
                    Simple.frame = 0.0f;
                    Simple.cycles += 1;
                }
            }
            else if (isDynamic)
            {
                Dynamic.frame += 0.1f;//Time.deltaTime;
                if (Dynamic.frame >= Dynamic.keyframes[Dynamic.keyframes.Length - 1])
                {
                    Dynamic.frame = 0.0f;
                    Dynamic.cycles += 1;
                }
                if (Dynamic.blendFrom != null)
                {
                    Dynamic.blendFrom.frame += 0.1f;//Time.deltaTime;
                    Dynamic.blendFrame += 0.1f;//Time.deltaTime;
                }
            }

        }
        public void PlayAction(Armature rig)
        {
            if (isSimple)
                PlaySimpleAction(rig);
            else if (isDynamic)
                PlayDynamicAction(rig);
        }

        public void PlayDynamicAction(Armature rig)
        {
            ArmaturePose pose = Dynamic.currentPose;
            Dynamic.frame += Dynamic.speed * 0.1f;//Time.deltaTime * Dynamic.speed;
            if (frame >= keyframes[keyframes.Length - 1])
            {
                Dynamic.cycles += 1;
                Dynamic.frame = 0.0f;
            }
            if (Dynamic.blendFrom != null)
            {
                Dynamic.blendFrame += Dynamic.speed * 0.1f;//Time.deltaTime * Dynamic.speed; //TODO: automatically updates regardless of "playing"
                Dynamic.blendFrom.frame += Dynamic.blendFrom.speed * 0.1f;//Time.deltaTime * Dynamic.blendFrom.speed;
                if (Dynamic.blendFrom.frame >= Dynamic.blendFrom.keyframes[Dynamic.blendFrom.keyframes.Length - 1])
                {
                    Dynamic.blendFrom.frame = 0.0f;
                    Dynamic.blendFrom.cycles += 1;
                }
            }
            for (int i = 0; i < pose.bones.Length; i++)
            {
                string bone = pose.bones[i];
                BonePose p = pose[bone];
                transform t = p.hasShortcut ? p.boneShortcut : rig[bone];
                if (t == null)
                    continue;
                if (p.hasLocationData)
                    t.location_local = p.location;
                if (p.hasRotationData)
                    t.rotation_local = p.rotationQuat;
                //if (p.hasScaleData) t.localScale = p.scale;
            }
        }

        public void PlaySimpleAction(Armature rig)
        {
            BonePose pose = Simple.currentPose;
            Simple.frame += speed * 0.1f;//Time.deltaTime * speed;
            if (frame >= keyframes[keyframes.Length - 1])
            {
                Simple.cycles += 1;
                Simple.frame = 0.0f;
            }
            if (pose.hasLocationData)
                rig.armatureObj.location_local = pose.location;
            if (pose.hasRotationData)
                rig.armatureObj.rotation_local = pose.rotationQuat;
            //if (pose.hasScaleData) rig.armatureObj.localScale = pose.scale;
        }

        public static Action CreateSimpleEmptyAction(transform obj)
        {
            Action action = new Action();
            action.type = "simple";
            action.Simple = new SimpleAction();
            action.Simple.speed = 1.0f;
            //CreateNewMethodToAddToAction
            return action;
        }
        public static Action CreateDynamicEmptyAction(transform obj)
        {
            Action action = new Action();
            action.type = "dynamic";
            action.Dynamic = new DynamicAction();
            action.Dynamic.speed = 1.0f;
            return action;
        }
        public string name
        {
            get { return isSimple ? Simple.name : Dynamic.name; }
        }
        public float frame
        {
            get
            {
                if (isSimple)
                    return Simple.frame;
                else if (isDynamic)
                    return Dynamic.frame;
                return 0.0f;
            }
        }
        public float speed
        {
            get { return isSimple ? Simple.speed : Dynamic.speed; }
        }
        public int cycles
        {
            get { return isSimple ? Simple.cycles : Dynamic.cycles; }
        }
        public string loop
        {
            get { return isSimple ? Simple.loop : Dynamic.loop; }
        }
        public float[] keyframes
        {
            get { return isSimple ? Simple.keyframes : Dynamic.keyframes; }
        }
        public bool isSimple
        {
            get { return Simple != null && type.ToLower() == "simple"; }
        }
        public bool isDynamic
        {
            get { return Dynamic != null && type.ToLower() == "dynamic"; }
        }
    }

    public class DynamicAction
    {
        public string name;
        public float frame;
        public float speed; //default 1.0
        public int cycles;
        public string loop;
        public float[] keyframes;
        public ArmaturePose[] poses;

        public DynamicAction blendFrom;
        //public RangeInt blendRange;
        public float blendRangeStart;
        public float blendRangeLength;
        public float blendFrame;

        public string[] bones
        {
            get
            {
                List<string> list = new List<string> { };
                for (int i = 0; i < poses.Length; i++)
                {
                    for (int n = 0; n < poses[i].bones.Length; n++)
                    {
                        if (!list.Contains(poses[i].bones[n]))
                            list.Add(poses[i].bones[n]);
                    }
                }
                return list.ToArray();
            }
            set
            {
                for (int i = 0; i < poses.Length; i++)
                {
                    poses[i].bones = value;
                }
            }
        }
        public void ForceBoneConsistency()
        {
            string[] bs = bones;
            bones = bs;
        }
        public void PlayBlind()
        {
            frame += 0.1f;//Time.deltaTime;
            if (frame >= keyframes[keyframes.Length - 1])
            {
                frame = 0.0f;
                cycles += 1;
            }
            if (blendFrom != null)
            {
                blendFrom.frame += 0.1f;//Time.deltaTime;
                blendFrame += 0.1f;//Time.deltaTime;
            }
        }
        public ArmaturePose currentPose
        {
            get
            {
                int last = previousKeyframe;
                int next = nextKeyframe;
                float low = keyframes[last];
                float high = keyframes[next];
                float diff = System.Math.Abs(high - low);
                float progress = frame - low;
                float ratio = progress / diff;
                ArmaturePose lerp = ArmaturePose.Lerp(poses[last], poses[next], ratio);
                if (blendFrom != null && blendFrame <= blendRangeLength)
                {
                    float r = (blendFrame - blendRangeStart) / blendRangeLength;
                    lerp = ArmaturePose.Lerp(blendFrom.currentPose, lerp, r);
                }
                else
                {
                    blendFrom = null;
                }
                return lerp;
            }
        }
        public ArmaturePose previousKeyPose
        {
            get { return poses[previousKeyframe]; }
        }
        public ArmaturePose nextKeyPose
        {
            get { return poses[nextKeyframe]; }
        }
        public int previousKeyframe
        {
            get
            {
                int f = -1;
                for (int i = 0; i < keyframes.Length; i++)
                {
                    if (keyframes[i] > frame)
                        break;
                    f = i;
                }
                if (f == -1)
                    f = keyframes.Length - 1;
                return f;
            }
        }
        public int nextKeyframe
        {
            get
            {
                int f = previousKeyframe + 1;
                if (f >= keyframes.Length)
                    f = 0;
                return f;
            }
        }

        public void CreateTransition(DynamicAction fromAction, float seconds)
        {
            blendFrom = fromAction;
            blendFrame = 0.0f;
            //blendRange = new RangeInt(0, Math.CeilToInt(seconds));
            blendRangeStart = 0f;
            blendRangeLength = seconds;
        }
        public void CreateTransition(ArmaturePose fromPose, float seconds, Armature a)
        {
            blendFrom = Action.CreateDynamicEmptyAction(a.armatureObj).Dynamic;
            blendFrame = 0.0f;
            blendFrom.poses = new ArmaturePose[] { fromPose, fromPose };
            blendFrom.keyframes = new float[] { 0.0f, 100.0f };
            blendFrom.speed = 1.0f;
            //blendRange = new RangeInt(0, Math.CeilToInt(seconds));
            blendRangeStart = 0f;
            blendRangeLength = seconds;
        }
        //public DynamicAction Capture (){}
        public static DynamicAction ReadAction(string address)
        {
            if (!System.IO.File.Exists(address))
                return null;
            string[] lines = System.IO.File.ReadAllLines(address);
            return ReadAction(lines);
        }
        /*
        public static DynamicAction ReadActionFromResources(string name)
        {
            TextAsset textasset = (TextAsset)Resources.Load("anim/" + name, typeof(TextAsset));
            if (textasset == null) return null;
            string[] text = textasset.text.Split(new string[] { "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);
            return DynamicAction.ReadAction(text);
        }
        */
        public static DynamicAction ReadAction(string[] text)
        {
            if (text == null || text.Length == 0)
                return null;
            DynamicAction action = new DynamicAction();
            action.speed = 1.0f;
            bool hasActionData = text[0].IndexOf("action") == 0;
            List<int> keys = new List<int> { };
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i].IndexOf("key") == 0 || text[i].IndexOf("k") == 0)
                    keys.Add(i);
            }
            keys.Add(text.Length);
            string[][] texts = new string[keys.Count - 1][];
            int t = 0;
            for (int i = keys[0]; i < text.Length; i++)
            {
                bool isKey = keys.Contains(i);
                int nextKey = isKey ? keys[keys.IndexOf(i) + 1] : -1;
                int lastKey = 0;
                foreach (int k in keys)
                {
                    if (k > i)
                        break;
                    lastKey = k;
                }
                if (isKey)
                {
                    texts[t] = new string[nextKey - i];//-1;
                    texts[t][0] = text[i];
                    t += 1;
                }
                else
                {//lastKey - 1
                    texts[t - 1][i - lastKey] = text[i];
                }
            }
            action.poses = new ArmaturePose[texts.Length];
            action.keyframes = new float[texts.Length];
            for (int i = 0; i < texts.Length; i++)
            {
                action.poses[i] = ArmaturePose.ReadPose(texts[i]);
                action.keyframes[i] = action.poses[i].linkedFrame;
            }
            return action;
        }
        public Action ToAction()
        {
            Action action = new Action();
            action.type = "dynamic";
            action.Dynamic = this;
            action.Dynamic.speed = 1.0f;
            return action;
        }
    }
    public class SimpleAction
    {
        public string name;
        public float frame;
        public float speed; //default 1.0
        public int cycles;
        public string loop;
        public float[] keyframes;
        public BonePose[] poses;

        public void PlayBlind()
        {
            frame += 0.1f;//Time.deltaTime;
            if (frame >= keyframes[keyframes.Length - 1])
            {
                frame = 0.0f;
                cycles += 1;
            }
        }
        public BonePose currentPose
        {
            get
            {
                int last = previousKeyframe;
                int next = nextKeyframe;
                float low = keyframes[last];
                float high = keyframes[next];
                float diff = System.Math.Abs(high - low);
                float progress = frame - low;
                float ratio = progress / diff;
                return BonePose.Lerp(poses[last], poses[next], ratio);
            }
        }
        public BonePose previousKeyPose
        {
            get { return poses[previousKeyframe]; }
        }
        public BonePose nextKeyPose
        {
            get { return poses[nextKeyframe]; }
        }
        public int previousKeyframe
        {
            get
            {
                int f = -1;
                for (int i = 0; i < keyframes.Length; i++)
                {
                    if (keyframes[i] > frame)
                        break;
                    f = i;
                }
                if (f == -1)
                    f = keyframes.Length - 1;
                return f;
            }
        }
        public int nextKeyframe
        {
            get
            {
                int f = previousKeyframe + 1;
                if (f >= keyframes.Length)
                    f = 0;
                return f;
            }
        }
    }

    public class ArmaturePose
    {
        public bool basicArmature;//
        public BonePose basicPose;//
        public float linkedFrame;
        public string[] bones;
        public Dictionary<string, BonePose> dictionary;

        public BonePose this[string key]
        {
            get { return dictionary.ContainsKey(key) ? dictionary[key] : null; }
            set { dictionary[key] = value; }
        }
        public void Play(Armature rig)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                string bone = bones[i];
                BonePose p = this[bone];
                transform t = p.hasShortcut ? p.boneShortcut : rig[bone];
                if (p.hasLocationData)
                    t.location_local = p.location;
                if (p.hasRotationData)
                    t.rotation_local = p.rotationQuat;
                //if (p.hasScaleData) t.localScale = p.scale;
            }
        }
        public static ArmaturePose Lerp(ArmaturePose a, ArmaturePose b, float t) // "a" SHOULD CONTAIN MOST BONES
        {
            //if (t < 0.01f)
            //	return a;
            //else if (t > 0.99f)
            //	return b;
            ArmaturePose pose = new ArmaturePose();
            pose.basicArmature = false;//a.basicArmature;
                                       /*
                                       if (a.basicArmature != null)
                                           pose.basicArmature = a.basicArmature;
                                       else if (b.basicArmature != null)
                                           pose.basicArmature = b.basicArmature;*/

            pose.bones = a.bones != null ? a.bones : b.bones;
            if (a.bones.Length != b.bones.Length)
            {
                List<string> list = new List<string> { };
                foreach (string s in a.bones)
                {
                    if (!list.Contains(s))
                        list.Add(s);
                }
                foreach (string s in b.bones)
                {
                    if (!list.Contains(s))
                        list.Add(s);
                }
                pose.bones = list.ToArray();
            }
            if (pose.basicArmature && a.basicPose != null && b.basicPose != null)
            {
                pose.basicPose = BonePose.Lerp(a.basicPose, b.basicPose, t);
                return pose;
            }
            pose.dictionary = new Dictionary<string, BonePose> { };
            for (int i = 0; i < pose.bones.Length; i++)
            {
                string bone = pose.bones[i];
                if (a[bone] != null && b[bone] != null)
                    pose.dictionary.Add(bone, BonePose.Lerp(a[bone], b[bone], t));
                else if (a[bone] != null)
                    pose.dictionary.Add(bone, a[bone]);
                else if (b[bone] != null)
                    pose.dictionary.Add(bone, b[bone]);
            }
            //for (int i = 0; i < b.bones.Length; i++){} //TODO: IF NOT IN A.BONES!!!
            return pose;
        }
        public void Capture(Armature rig)
        {
            if (bones == null)
                return;
            dictionary = new Dictionary<string, BonePose> { };
            for (int i = 0; i < bones.Length; i++)
            {
                transform t = rig[bones[i]];
                if (t != null)
                    dictionary[bones[i]] = BonePose.Capture(rig[bones[i]]);
            }
        }
        public static ArmaturePose CreateAndCapture(Armature rig, params string[] transforms)
        {
            ArmaturePose pose = new ArmaturePose();
            pose.basicArmature = false;
            List<string> b = new List<string> { };
            foreach (string s in transforms)
                if (rig[s] != null)
                    b.Add(s);
            pose.bones = b.ToArray();
            pose.Capture(rig);
            return pose;
        }
        public static ArmaturePose ReadPose(string[] text)
        {
            if (text == null || text.Length == 0)
                return null;
            ArmaturePose pose = new ArmaturePose();
            pose.basicArmature = false;
            bool hasKey = text[0].IndexOf("key") == 0;
            if (hasKey)
            {
                string r = text[0].Replace(" :", ": ").Replace(":", ": ").Replace("  ", " ").Replace("  ", " ");
                pose.linkedFrame = float.Parse(r.Split()[1]);
            }
            pose.bones = hasKey ? new string[text.Length - 1] : new string[text.Length];
            int start = hasKey ? 1 : 0;
            pose.dictionary = new Dictionary<string, BonePose> { };
            for (int i = start; i < text.Length; i++)
            {
                BonePose b = BonePose.ReadPose(text[i]);
                pose.dictionary[b.boneName] = b;
                pose.bones[i - start] = b.boneName;
            }
            return pose;
        }
        public string[] WritePose()
        {
            string[] output = new string[bones.Length + 1];
            output[0] = "key: " + linkedFrame;
            for (int i = 0; i < bones.Length; i++)
            {
                output[i + 1] = dictionary[bones[i]].WritePose();
            }
            return output;
        }
    }
    public class BonePose
    {
        public string boneName;
        public bool hasShortcut;
        public transform boneShortcut;

        public bool hasConstraint;
        public BoneConstraint constraint;

        public bool isLocal;
        public transform relativeTo;
        public bool hasLocationData;
        public bool hasRotationData;
        public bool hasScaleData;
        public vector location;
        public quat rotationQuat;
        public vector scale;

        public bool isFixed
        {
            get { return hasConstraint == false; }
        }
        public vector rotation
        {
            get { return rotationQuat.euler_angles(); }
            set { rotationQuat = quat.ToQuaternion(value.z, value.y, value.x); }
        }
        public static BonePose Lerp(BonePose a, BonePose b, float t)
        {
            //if (t < 0.01f)
            //	return a;
            //else if (t > 0.99f)
            //	return b;
            BonePose pose = new BonePose();
            pose.boneName = a.boneName;
            pose.hasShortcut = a.hasShortcut;
            pose.boneShortcut = a.boneShortcut;
            pose.hasConstraint = a.hasConstraint;
            pose.constraint = a.constraint;
            pose.isLocal = a.isLocal;
            pose.relativeTo = a.relativeTo;
            pose.hasLocationData = a.hasLocationData;
            pose.hasRotationData = a.hasRotationData;
            pose.hasScaleData = a.hasScaleData;
            pose.location = a.location == b.location ? a.location : vector.lerp(a.location, b.location, t);
            pose.rotationQuat = /*a.rotationQuat == b.rotationQuat ? a.rotationQuat : */quat.lerp(a.rotationQuat, b.rotationQuat, t);
            pose.scale = a.scale == b.scale ? a.scale : vector.lerp(a.scale, b.scale, t);
            return pose;
        }
        public static BonePose ReadPose(string text)
        {
            // Example:
            // brachial_L = pose: l = 0.0 0.0 1.0, r = ...

            string rewrite = text.Replace(":", ": ").Replace(" :", ":").Replace(",", " , ").Replace("  ", " ");
            if (rewrite.Contains("  "))
                rewrite = rewrite.Replace("  ", " ");
            string[] split = rewrite.Split();
            BonePose pose = new BonePose();
            pose.hasShortcut = false;
            if (split[1] == "=" && split[2] == "pose:")
                pose.boneName = split[0];
            if (System.Array.IndexOf(split, "l:") >= 0)
            {
                int l = System.Array.IndexOf(split, "l:");
                pose.hasLocationData = true;
                float x = float.Parse(split[l + 1]);
                float y = float.Parse(split[l + 2]);
                float z = float.Parse(split[l + 3]);
                pose.location = new vector(x, y, z);
                //Debug.Log ("??!! " + split[l + 1] + " " + split[l + 2] + " " + split[l + 3]);
                //Debug.Log ("??? " + x + " " + y + " " + z);
                //Debug.Log ("??? " + (float.Parse (split[l + 1]) > 5) + " " + (y > 5) + " " + (z > 5));
                //Debug.Log ("?? " + pose.location);
            }
            if (System.Array.IndexOf(split, "r:") >= 0)
            {
                int r = System.Array.IndexOf(split, "r:");
                float parse = 0.0f;
                bool fourth = r + 4 < split.Length && float.TryParse(split[r + 4], out parse);
                pose.hasRotationData = true;
                if (fourth)
                    pose.rotationQuat = new quat(float.Parse(split[r + 1]), float.Parse(split[r + 2]), float.Parse(split[r + 3]), parse);
                else
                    pose.rotation = new vector(float.Parse(split[r + 1]), float.Parse(split[r + 2]), float.Parse(split[r + 3]));
            }
            if (System.Array.IndexOf(split, "s:") >= 0)
            {
                int s = System.Array.IndexOf(split, "s:");
                pose.hasScaleData = true;
                pose.scale = new vector(float.Parse(split[s + 1]), float.Parse(split[s + 2]), float.Parse(split[s + 3]));
            }

            /*
			BonePose pose = new BonePose ();
			string arg = text.Replace (":", ": ");
			while (arg.IndexOf ("  ") >= 0)
				arg = arg.Replace ("  ", " ");
			string[] split = arg.Split ();
			int b = System.Array.IndexOf (split, "b:");
			int l = System.Array.IndexOf (split, "l:");
			int r = System.Array.IndexOf (split, "r:");
			int s = System.Array.IndexOf (split, "s:");
			
			pose.boneName = split[1];
			pose.location = new vector ();
			if (l >= 0)
			{
				pose.location = new vector (float.Parse (split[l + 1]), float.Parse (split[l + 2]), float.Parse (split[l + 3]));
				pose.hasLocationData = true;
			}
			pose.rotationQuat = new quat ();
			if (r >= 0)
			{
				pose.rotation = new vector (float.Parse (split[r + 1]), float.Parse (split[r + 2]), float.Parse (split[r + 3]));
				pose.hasRotationData = true;
			}
			pose.scale = new vector ();
			if (s >= 0)
			{
				pose.scale = new vector (float.Parse (split[s + 1]), float.Parse (split[s + 2]), float.Parse (split[s + 3]));
				pose.hasScaleData = true;
			}
			CSL_Engine.World.armature a = new CSL_Engine.World.armature (GameObject.Find ("theta").transform.Find ("Armature/root"), true);
			CSL_Engine.World.position p = new CSL_Engine.World.position (a, text);
			pose.location = p.location;
			pose.rotation = p.rotation;
			pose.scale = p.scale;*/
            return pose;
        }
        public static BonePose Capture(transform obj)
        {
            BonePose pose = new BonePose();
            pose.hasShortcut = false;
            pose.boneShortcut = obj;
            pose.boneName = obj.name;
            pose.hasLocationData = true;
            pose.hasRotationData = true;
            pose.hasScaleData = true;
            pose.location = obj.location_local;
            pose.rotationQuat = obj.rotation_local;
            //pose.scale = obj.localScale;
            return pose;
        }
        public string WritePose()
        {
            string output = boneName + " = pose: ";
            output += "l: " + location.x + " " + location.y + " " + location.z + ", ";
            output += "r: " + rotationQuat.x + " " + rotationQuat.y + " " + rotationQuat.z + " " + rotationQuat.w + ", ";
            output += "s: " + scale.x + " " + scale.y + " " + scale.z;
            return output;
        }
    }
    public class BoneConstraint
    {

    }


    public class Position
    {
        public quat rotationQuat;
        public vector rotation
        {
            get { return rotationQuat.euler_angles(); }
            set
            {
                rotationQuat = quat.ToQuaternion(value.z, value.y, value.x);
            }
        }
        public vector front
        {
            get { return rotationQuat * vector.forward; }
        }
        public vector backward
        {
            get { return front * -1.0f; }
        }
        public vector right
        {
            get { return rotationQuat * vector.right; }
        }
        public vector left
        {
            get { return right * -1.0f; }
        }
        public vector upward
        {
            get { return rotationQuat * vector.upward; }
        }
        public vector downward
        {
            get { return upward * -1.0f; }
        }

        public static Position Copytransform(transform t)
        {
            Position p = new Position();
            p.rotationQuat = t.rotation;
            return p;
        }
    }

}

