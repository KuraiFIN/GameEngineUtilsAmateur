// This file is cringe; I was young okay??

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSL_Engine.Theta.World
{
    public static class Useful
    {
        public static T[] SubArray<T>(this T[] array, int startIndex, int endIndex)
        {
            int abs_start = startIndex < 0 ? array.Length + startIndex : startIndex;
            int abs_end = endIndex < 0 ? array.Length + endIndex : endIndex;
            bool forward = abs_end >= abs_start;
            int true_start = forward ? abs_start : abs_end;
            int true_end = forward ? abs_end : abs_start;
            if (true_start < 0 || true_end >= array.Length) return new T[] { };
            else if (true_start == true_end) return new T[] { array[true_start] };
            else if (true_end - 1 == true_start) return new T[] { array[true_start], array[true_end] };
            T[] sub_array = new T[true_end - true_start + 1];
            for (int i = true_start; i <= true_end; i++) sub_array[i - true_start] = array[i];
            return sub_array;
        }
        public static T[] Add<T>(this T[] array, params T[] items)
        {
            if (items.Length == 0) return array;
            T[] output = new T[array.Length + items.Length];
            for (int i = 0; i < array.Length; i++) output[i] = array[i];
            for (int i = array.Length; i < output.Length; i++) output[i] = items[i - array.Length];
            return output;
        }
        public static T[] Remove<T>(this T[] array, T item) where T : class
        {
            T[] output = new T[array.Length];
            array.CopyTo(output, 0);
            output = output.Where(o => o != item).ToArray();
            return output;
        }
        public static List<T> ToList<T>(this T[] array) where T : class
        {
            List<T> list = new List<T> { };
            for (int i = 0; i < array.Length; i++) list.Add(array[i]);
            return list;
        }
        public static string str<T>(this T[] array) where T : class
        {
            return array.str(" ");
        }
        public static string str<T>(this T[] array, string div) where T : class
        {
            if (array == null || array.Length == 0) return string.Empty;
            string output = array[0].ToString();
            for (int i = 1; i < array.Length; i++) output += div + array[i].ToString();
            return output;
        }
        public static string str<T>(this List<T> list) where T : class
        {
            return list.str(" ");
        }
        public static string str<T>(this List<T> list, string div) where T : class
        {
            if (list == null || list.Count == 0) return string.Empty;
            string output = list[0].ToString();
            for (int i = 1; i < list.Count; i++) output += div + list[i].ToString();
            return output;
        }
    }
    public static class CommonBones
    {
        public static string[] allHuman = new string[] {"root", "root_L", "root_R", "iliac_L", "iliac_R", "femoral_L", "femoral_R", "crural_L", "crural_R",
            "gastric", "thoracic", "cervical", "acromial_L", "acromial_R", "brachial_L", "brachial_R", "antebrachial_L", "antebrachial_R", "palmar_L", "palmar_R"};
        public static string[] allHumanFace = new string[] {"head", "browDownA_L", "browDownA_R", "browRidge_L", "browRidge_R", "browUpA_L", "browUpA_R",
            "cheek_L", "cheek_R", "cheekbone_L", "cheekbone_R", "eyeA_L", "eyeA_R", "eyeB_L", "eyeB_R", "eyeC_L", "eyeC_R", "eyeD_L", "eyeD_R", "jaw",
            "lipsA_L", "lipsA_R", "lipsB_L", "lipsB_R", "lipsC_L", "lipsC_R", "lipsD_L", "lipsD_R", "nose", "nose_L", "nose_R"};
    }
    public class ArmatureMultiple
    {
        public bool standard; //true=use_fields; false=use_array
        public Dictionary<string, TransformMultiple> dictionary;
        public List<Armature> rigs;

        //standard_array

        //torso_rig
        //leg_rig
        //extra_rig
        public void UpdateArmatures(Armature rig)
        {
            if (dictionary == null || dictionary.Count == 0 || dictionary.Keys.Count == 0) return;
            string[] bones = rig.bones;
            for (int i = 0; i < bones.Length; i++)
            {
                string b = bones[i];
                TransformMultiple mult = dictionary[b];
                if (mult.array == null || mult.array.Length == 0) continue;
                //Debug.LogError (mult.array.str ("\n"));
                for (int n = 0; n < mult.array.Length; n++)
                {
                    transform t = mult.array[n];
                    if (t == null) continue;
                    //t.localRotation = rig[b].localRotation;
                    t.location = rig[b].location;
                    t.rotation = rig[b].rotation;
                    //t.localScale = rig[b].localScale;
                }
            }
        }
        public void AddArmature(Armature rig)
        {
            if (rigs == null)
            {
                rigs = new List<Armature> { };
                dictionary = new Dictionary<string, TransformMultiple> { };
            }
            rigs.Add(rig);
            string[] bones = rig.bones;
            for (int i = 0; i < bones.Length; i++)
            {
                TransformMultiple m = dictionary.ContainsKey(bones[i]) ? dictionary[bones[i]] : null;
                if (m == null)
                {
                    m = new TransformMultiple();
                    m.array = new transform[] { rig[bones[i]] };
                    dictionary[bones[i]] = m;
                }
                else
                {
                    m.array = m.array.Add(rig[bones[i]]);
                }

            }
        }
        public void RemoveArmature(Armature rig)
        {
            if (rigs == null) return;
            rigs.Remove(rig);
            string[] bones = rig.bones;
            for (int i = 0; i < bones.Length; i++) dictionary[bones[i]].array = dictionary[bones[i]].array.Remove(rig[bones[i]]);
        }
    }
    public class TransformMultiple
    {
        //used for single bone across separate rigs
        public transform[] array;
    }
    public class Armature
    {
        public transform armatureObj;
        public string type; // Human, Simple (only uses armatureObj), dynamic (declare your own bones in dic);
        public ArmaturePose rest_pose;
        public HumanArmature human;
        public DynamicArmature Dynamic;
        public string[] bones
        {
            get
            {
                if (human != null) return human.bones;
                else if (Dynamic != null) return Dynamic.bones.ToArray();
                return null;
            }
        }

        public transform this[string key]
        {
            get
            {
                transform trans = null;
                if (isHuman)
                    return human[key];
                else if (isDynamic)
                    return Dynamic.dictionary[key];
                return trans;
            }
            set
            {
                if (isHuman)
                    human[key] = value;
                else if (isDynamic)
                    Dynamic.dictionary[key] = value;
            }
        }


        public bool isHuman
        {
            get { return human != null && type.ToLower() == "human"; }
        }
        public bool isDynamic
        {
            get { return Dynamic != null && type.ToLower() == "dynamic"; }
        }

        public static Armature CreateBasicHuman(transform obj, bool useDictionary)
        {
            Armature rig = new Armature();
            rig.type = "human";
            rig.human = new HumanArmature();
            rig.human.body = useDictionary ? HumanBodyArmature.NewWithDictionary(obj) : HumanBodyArmature.NewWithVariables(obj);
            rig.human.bones = rig.human.body.bones;
            return rig;
        }
        public void AddHumanFace(bool useDictionary)
        {
            transform cervical = this["cervical"];
            human.face = useDictionary ? HumanFaceArmature.NewWithDictionary(cervical) : HumanFaceArmature.NewWithVariables(cervical);

            if (human.face.bones == null) return;
            else if (human.bones == null)
            {
                human.bones = human.face.bones;
                return;
            }
            string[] newbones = new string[human.bones.Length + human.face.bones.Length];
            human.bones.CopyTo(newbones, 0);
            human.face.bones.CopyTo(newbones, human.bones.Length);
            human.bones = newbones;
        }
        public static Armature CreateEmptyDynamic(transform obj)
        {
            Armature rig = new Armature();
            rig.type = "dynamic";
            rig.Dynamic = new DynamicArmature();
            rig.armatureObj = obj;
            rig.Dynamic.root = obj;
            string lower = obj.name.ToLower();
            if (lower.Contains("armature") || lower.Contains("rig"))
                rig.Dynamic.root = obj.find("root");
            else if (lower.Contains("root"))
                rig.armatureObj = obj.parent;
            else
            {
                rig.armatureObj = obj.children[0];
                foreach (transform t in obj.children)
                {
                    string l = t.name.ToLower();
                    if (l.Contains("armature") || l.Contains("rig"))
                    {
                        rig.armatureObj = t;
                        break;
                    }
                }
                rig.Dynamic.root = rig.armatureObj.find("root");
            }
            rig.Dynamic.dictionary = new Dictionary<string, transform> { };
            return rig;

        }

    }
    public class HumanArmature
    {
        public HumanBodyArmature body;
        public HumanFaceArmature face;
        public HumanHandArmature hands;
        public HumanFootArmature feet;

        public string[] bones;

        public transform this[string key]
        {
            get
            {
                transform trans = null;
                if (hasBody)
                {
                    transform t = body[key];
                    if (t != null)
                        return t;
                }
                if (hasFace)
                {
                    transform t = face[key];
                    if (t != null)
                        return t;
                }
                /*
				if (hasHands)
				{
					transform t = hands[key];
				}*/
                //if (hasBody)
                //	return body["root"];
                return trans;
            }
            set
            {
                if (hasBody)
                {
                    transform t = body[key];
                    if (t != null)
                    {
                        body[key] = value;
                        return;
                    }
                }
                if (hasFace)
                {
                    transform t = face[key];
                    if (t != null)
                    {
                        face[key] = value;
                        return;
                    }
                }
            }
        }

        public bool hasBody
        {
            get { return body != null; }
        }
        public bool hasFace
        {
            get { return face != null; }
        }
        public bool hasHands
        {
            get { return hands != null; }
        }
        public bool hasFeet
        {
            get { return feet != null; }
        }
    }
    public class HumanBodyArmature
    {
        public bool hasDictionary;
        public Dictionary<string, transform> dictionary;
        public HumanBodyArmatureVars variables;
        public string[] bones;

        public static HumanBodyArmature NewWithDictionary(transform obj)
        {
            HumanBodyArmature body = new HumanBodyArmature();
            body.dictionary = new Dictionary<string, transform> { };
            body.hasDictionary = true;

            HumanBodyArmatureVars vars = HumanBodyArmature.NewVariables(obj);
            body.SetKey("root", vars.root);
            body.SetKey("gastric", vars.gastric);
            body.SetKey("thoracic", vars.thoracic);
            body.SetKey("cervical", vars.cervical);

            body.SetKey("acromial_L", vars.acromial_L);
            body.SetKey("brachial_L", vars.brachial_L);
            body.SetKey("antebrachial_L", vars.antebrachial_L);
            body.SetKey("acromial_R", vars.acromial_R);
            body.SetKey("brachial_R", vars.brachial_R);
            body.SetKey("antebrachial_R", vars.antebrachial_R);
            body.SetKey("palmar_L", vars.palmar_L);
            body.SetKey("palmar_R", vars.palmar_R);

            body.SetKey("root_L", vars.root_L);
            body.SetKey("gluteal_L", vars.gluteal_L);
            body.SetKey("iliac_L", vars.iliac_L);
            body.SetKey("femoral_L", vars.femoral_L);
            body.SetKey("crural_L", vars.crural_L);
            body.SetKey("root_R", vars.root_R);
            body.SetKey("gluteal_R", vars.gluteal_R);
            body.SetKey("iliac_R", vars.iliac_R);
            body.SetKey("femoral_R", vars.femoral_R);
            body.SetKey("crural_R", vars.crural_R);

            body.bones = new string[body.dictionary.Count];
            body.dictionary.Keys.CopyTo(body.bones, 0);
            return body;
        }
        public static HumanBodyArmature NewWithVariables(transform obj)
        {
            HumanBodyArmature body = new HumanBodyArmature();
            body.hasDictionary = false;
            body.variables = HumanBodyArmature.NewVariables(obj);

            return body;
        }
        public static HumanBodyArmatureVars NewVariables(transform obj)
        {
            bool no_side_roots = false;

            string lower = obj.name.ToLower();
            transform root = obj; // assumed root
            if (lower != "root" && !lower.Contains("armature") && !lower.Contains("rig")) //must be main obj
            {
                foreach (transform t in obj.children)
                {
                    string l = t.name.ToLower();
                    if (l.Contains("armature") || l.Contains("rig"))
                    {
                        root = t;
                        break;
                    }
                }
            }
            lower = root.name.ToLower();
            if (lower.Contains("armature") || lower.Contains("rig"))
            {
                transform r = root;
                root = root.find("root");
                if (root == null) root = r.find("gastric");
            }

            HumanBodyArmatureVars body = new HumanBodyArmatureVars();
            body.SetVariable("root", root);
            body.SetVariable("gastric", body.root.find("gastric"));
            if (body.gastric == null) body.SetVariable("gastric", root);
            body.SetVariable("thoracic", body.gastric.find("thoracic"));
            body.SetVariable("cervical", body.thoracic.find("cervical"));

            body.SetVariable("acromial_L", body.thoracic.find("acromial_L"));
            body.SetVariable("brachial_L", body.acromial_L.find("brachial_L"));
            body.SetVariable("antebrachial_L", body.brachial_L.find("antebrachial_L"));
            body.SetVariable("acromial_R", body.thoracic.find("acromial_R"));
            body.SetVariable("brachial_R", body.acromial_R.find("brachial_R"));
            body.SetVariable("antebrachial_R", body.brachial_R.find("antebrachial_R"));
            body.SetVariable("palmar_L", body.antebrachial_L.find("palmar_L"));
            body.SetVariable("palmar_R", body.antebrachial_R.find("palmar_R"));

            body.SetVariable("root_L", body.root.find("root_L"));
            if (body.root_L == null) body.SetVariable("root_L", root);
            body.SetVariable("gluteal_L", body.root_L.find("gluteal_L"));
            body.SetVariable("iliac_L", body.root_L.find("iliac_L"));
            body.SetVariable("femoral_L", body.iliac_L.find("femoral_L"));
            body.SetVariable("crural_L", body.femoral_L.find("crural_L"));
            body.SetVariable("root_R", body.root.find("root_R"));
            if (body.root_R == null) body.SetVariable("root_R", root);
            body.SetVariable("gluteal_R", body.root_R.find("gluteal_R"));
            body.SetVariable("iliac_R", body.root_R.find("iliac_R"));
            body.SetVariable("femoral_R", body.iliac_R.find("femoral_R"));
            body.SetVariable("crural_R", body.femoral_R.find("crural_R"));

            if (body.root_L == root)
            {
                no_side_roots = true;
                body.SetVariable("root_L", null);
                body.SetVariable("root_R", null);
                body.SetVariable("root", null);
            }

            return body;
        }

        public transform this[string key]
        {
            get
            {
                return hasDictionary ? GetKey(key) : GetVariable(key);
            }
            set
            {
                if (hasDictionary)
                    SetKey(key, value);
                else
                    SetVariable(key, value);
            }
        }
        public transform GetKey(string name)
        {
            if (dictionary != null && dictionary.ContainsKey(name))
                return dictionary[name];
            return null;
        }
        public void SetKey(string name, transform val)
        {
            if (dictionary != null)
                dictionary[name] = val;
        }
        public transform GetVariable(string name)
        {
            if (variables != null)
                return variables.GetVariable(name);
            return null;
        }
        public void SetVariable(string name, transform val)
        {
            if (variables != null)
                variables.SetVariable(name, val);
        }

    }
    public class HumanBodyArmatureVars
    {
        public transform root;
        public transform gastric;
        public transform thoracic;
        public transform cervical;

        public transform acromial_L;
        public transform brachial_L;
        public transform antebrachial_L;
        public transform acromial_R;
        public transform brachial_R;
        public transform antebrachial_R;
        public transform palmar_L;
        public transform palmar_R;

        public transform root_L;
        public transform gluteal_L;
        public transform iliac_L;
        public transform femoral_L;
        public transform crural_L;
        public transform root_R;
        public transform gluteal_R;
        public transform iliac_R;
        public transform femoral_R;
        public transform crural_R;

        public transform GetVariable(string name)
        {
            switch (name)
            {
                case "root": return root;
                case "gastric": return gastric;
                case "thoracic": return thoracic;
                case "cervical": return cervical;

                case "acromial_L": return acromial_L;
                case "brachial_L": return brachial_L;
                case "antebrachial_L": return antebrachial_L;
                case "acromial_R": return acromial_R;
                case "brachial_R": return brachial_R;
                case "antebrachial_R": return antebrachial_R;
                case "palmar_L": return palmar_L;
                case "palmar_R": return palmar_R;

                case "root_L": return root_L;
                case "gluteal_L": return gluteal_L;
                case "iliac_L": return iliac_L;
                case "femoral_L": return femoral_L;
                case "crural_L": return crural_L;
                case "root_R": return root_R;
                case "gluteal_R": return gluteal_R;
                case "iliac_R": return iliac_R;
                case "femoral_R": return femoral_R;
                case "crural_R": return crural_R;
                default: return null;
            }
        }
        public void SetVariable(string name, transform val)
        {
            switch (name)
            {
                case "root":
                    root = val;
                    break;
                case "gastric":
                    gastric = val;
                    break;
                case "thoracic":
                    thoracic = val;
                    break;
                case "cervical":
                    cervical = val;
                    break;

                case "acromial_L":
                    acromial_L = val;
                    break;
                case "brachial_L":
                    brachial_L = val;
                    break;
                case "antebrachial_L":
                    antebrachial_L = val;
                    break;
                case "acromial_R":
                    acromial_R = val;
                    break;
                case "brachial_R":
                    brachial_R = val;
                    break;
                case "antebrachial_R":
                    antebrachial_R = val;
                    break;
                case "palmar_L":
                    palmar_L = val;
                    break;
                case "palmar_R":
                    palmar_R = val;
                    break;

                case "root_L":
                    root_L = val;
                    break;
                case "gluteal_L":
                    gluteal_L = val;
                    break;
                case "iliac_L":
                    iliac_L = val;
                    break;
                case "femoral_L":
                    femoral_L = val;
                    break;
                case "crural_L":
                    crural_L = val;
                    break;
                case "root_R":
                    root_R = val;
                    break;
                case "gluteal_R":
                    gluteal_R = val;
                    break;
                case "iliac_R":
                    iliac_R = val;
                    break;
                case "femoral_R":
                    femoral_R = val;
                    break;
                case "crural_R":
                    crural_R = val;
                    break;
            }
        }
    }
    
    public class HumanFaceArmature
    {
        public bool hasDictionary;
        public Dictionary<string, transform> dictionary;
        public HumanFaceArmatureVars variables;
        public string[] bones;

        public static HumanFaceArmature NewWithDictionary(transform cervical)
        {
            HumanFaceArmature face = new HumanFaceArmature();
            face.dictionary = new Dictionary<string, transform> { };
            face.hasDictionary = true;

            HumanFaceArmatureVars vars = HumanFaceArmature.NewVariables(cervical);
            face.SetKey("head", vars.head);
            face.SetKey("browDownA_L", vars.browDownA_L);
            face.SetKey("browDownA_R", vars.browDownA_R);
            face.SetKey("browRidge_L", vars.browRidge_L);
            face.SetKey("browRidge_R", vars.browRidge_R);
            face.SetKey("browUpA_L", vars.browUpA_L);
            face.SetKey("browUpA_R", vars.browUpA_R);
            face.SetKey("cheek_L", vars.cheek_L);
            face.SetKey("cheek_R", vars.cheek_R);
            face.SetKey("cheekbone_L", vars.cheekbone_L);
            face.SetKey("cheekbone_R", vars.cheekbone_R);

            face.SetKey("eyeA_L", vars.eyeA_L);
            face.SetKey("eyeA_R", vars.eyeA_R);
            face.SetKey("eyeB_L", vars.eyeB_L);
            face.SetKey("eyeB_R", vars.eyeB_R);
            face.SetKey("eyeC_L", vars.eyeC_L);
            face.SetKey("eyeC_R", vars.eyeC_R);
            face.SetKey("eyeD_L", vars.eyeD_L);
            face.SetKey("eyeD_R", vars.eyeD_R);
            face.SetKey("jaw", vars.jaw);

            face.SetKey("lipsA_L", vars.lipsA_L);
            face.SetKey("lipsA_R", vars.lipsA_R);
            face.SetKey("lipsB_L", vars.lipsB_L);
            face.SetKey("lipsB_R", vars.lipsB_R);
            face.SetKey("lipsC_L", vars.lipsC_L);
            face.SetKey("lipsC_R", vars.lipsC_R);
            face.SetKey("lipsD_L", vars.lipsD_L);
            face.SetKey("lipsD_R", vars.lipsD_R);
            face.SetKey("nose", vars.nose);
            face.SetKey("nose_L", vars.nose_L);
            face.SetKey("nose_R", vars.nose_R);

            face.bones = new string[face.dictionary.Count];
            face.dictionary.Keys.CopyTo(face.bones, 0);

            return face;
        }
        public static HumanFaceArmature NewWithVariables(transform cervical)
        {
            HumanFaceArmature face = new HumanFaceArmature();
            face.hasDictionary = false;
            face.variables = HumanFaceArmature.NewVariables(cervical);

            return face;
        }
        public static HumanFaceArmatureVars NewVariables(transform cervical)
        {
            HumanFaceArmatureVars face = new HumanFaceArmatureVars();
            face.SetVariable("head", cervical.find("head"));
            face.SetVariable("browDownA_L", face.head.find("browDownA_L"));
            face.SetVariable("browDownA_R", face.head.find("browDownA_R"));
            face.SetVariable("browRidge_L", face.head.find("browRidge_L"));
            face.SetVariable("browRidge_R", face.head.find("browRidge_R"));
            face.SetVariable("browUpA_L", face.head.find("browUpA_L"));
            face.SetVariable("browUpA_R", face.head.find("browUpA_R"));
            face.SetVariable("cheek_L", face.head.find("cheek_L"));
            face.SetVariable("cheek_R", face.head.find("cheek_R"));
            face.SetVariable("cheekbone_L", face.head.find("cheekbone_L"));
            face.SetVariable("cheekbone_R", face.head.find("cheekbone_R"));

            face.SetVariable("eyeA_L", face.head.find("eyeA_L"));
            face.SetVariable("eyeA_R", face.head.find("eyeA_R"));
            face.SetVariable("eyeB_L", face.head.find("eyeB_L"));
            face.SetVariable("eyeB_R", face.head.find("eyeB_R"));
            face.SetVariable("eyeC_L", face.head.find("eyeC_L"));
            face.SetVariable("eyeC_R", face.head.find("eyeC_R"));
            face.SetVariable("eyeD_L", face.head.find("eyeD_L"));
            face.SetVariable("eyeD_R", face.head.find("eyeD_R"));
            face.SetVariable("jaw", face.head.find("jaw"));
            face.SetVariable("lipsA_L", face.head.find("lipsA_L"));
            face.SetVariable("lipsA_R", face.head.find("lipsA_R"));
            face.SetVariable("lipsB_L", face.head.find("lipsB_L"));
            face.SetVariable("lipsB_R", face.head.find("lipsB_R"));
            face.SetVariable("lipsC_L", face.head.find("lipsC_L"));
            face.SetVariable("lipsC_R", face.head.find("lipsC_R"));
            face.SetVariable("lipsD_L", face.head.find("lipsD_L"));
            face.SetVariable("lipsD_R", face.head.find("lipsD_R"));
            face.SetVariable("nose", face.head.find("nose"));
            face.SetVariable("nose_L", face.head.find("nose_L"));
            face.SetVariable("nose_R", face.head.find("nose_R"));


            return face;
        }

        public transform this[string key]
        {
            get
            {
                return hasDictionary ? GetKey(key) : GetVariable(key);
            }
            set
            {
                if (hasDictionary)
                    SetKey(key, value);
                else
                    SetVariable(key, value);
            }
        }
        public transform GetKey(string name)
        {
            if (dictionary != null && dictionary.ContainsKey(name))
                return dictionary[name];
            return null;
        }
        public void SetKey(string name, transform val)
        {
            if (dictionary != null)
                dictionary[name] = val;
        }
        public transform GetVariable(string name)
        {
            if (variables != null)
                return variables.GetVariable(name);
            return null;
        }
        public void SetVariable(string name, transform val)
        {
            if (variables != null)
                variables.SetVariable(name, val);
        }
    }
    public class HumanFaceArmatureVars
    {
        public transform head;
        public transform browDownA_L;
        public transform browDownA_R;
        public transform browRidge_L;
        public transform browRidge_R;
        public transform browUpA_L;
        public transform browUpA_R;
        public transform cheek_L;
        public transform cheek_R;
        public transform cheekbone_L;
        public transform cheekbone_R;
        public transform eyeA_L;
        public transform eyeA_R;
        public transform eyeB_L;
        public transform eyeB_R;
        public transform eyeC_L;
        public transform eyeC_R;
        public transform eyeD_L;
        public transform eyeD_R;
        public transform jaw;
        public transform lipsA_L;
        public transform lipsA_R;
        public transform lipsB_L;
        public transform lipsB_R;
        public transform lipsC_L;
        public transform lipsC_R;
        public transform lipsD_L;
        public transform lipsD_R;
        public transform nose;
        public transform nose_L;
        public transform nose_R;
        public transform GetVariable(string name)
        {
            switch (name)
            {
                case "head": return head;
                case "browDownA_L": return browDownA_L;
                case "browDownA_R": return browDownA_R;
                case "browRidge_L": return browRidge_L;
                case "browRidge_R": return browRidge_R;
                case "browUpA_L": return browUpA_L;
                case "browUpA_R": return browUpA_R;
                case "cheek_L": return cheek_L;
                case "cheek_R": return cheek_R;
                case "cheekbone_L": return cheekbone_L;
                case "cheekbone_R": return cheekbone_R;
                case "eyeA_L": return eyeA_L;
                case "eyeA_R": return eyeA_R;
                case "eyeB_L": return eyeB_L;
                case "eyeB_R": return eyeB_R;
                case "eyeC_L": return eyeC_L;
                case "eyeC_R": return eyeC_R;
                case "eyeD_L": return eyeD_L;
                case "eyeD_R": return eyeD_R;
                case "jaw": return jaw;
                case "lipsA_L": return lipsA_L;
                case "lipsA_R": return lipsA_R;
                case "lipsB_L": return lipsB_L;
                case "lipsB_R": return lipsB_R;
                case "lipsC_L": return lipsC_L;
                case "lipsC_R": return lipsC_R;
                case "lipsD_L": return lipsD_L;
                case "lipsD_R": return lipsD_R;
                case "nose": return nose;
                case "nose_L": return nose_L;
                case "nose_R": return nose_R;
                default: return null;
            }
        }
        public void SetVariable(string name, transform val)
        {
            switch (name)
            {
                case "head":
                    head = val;
                    break;
                case "browDownA_L":
                    browDownA_L = val;
                    break;
                case "browDownA_R":
                    browDownA_R = val;
                    break;
                case "browRidge_L":
                    browRidge_L = val;
                    break;
                case "browRidge_R":
                    browRidge_R = val;
                    break;
                case "browUpA_L":
                    browUpA_L = val;
                    break;
                case "browUpA_R":
                    browUpA_R = val;
                    break;
                case "cheek_L":
                    cheek_L = val;
                    break;
                case "cheek_R":
                    cheek_R = val;
                    break;
                case "cheekbone_L":
                    cheekbone_L = val;
                    break;
                case "cheekbone_R":
                    cheekbone_R = val;
                    break;
                case "eyeA_L":
                    eyeA_L = val;
                    break;
                case "eyeA_R":
                    eyeA_R = val;
                    break;
                case "eyeB_L":
                    eyeB_L = val;
                    break;
                case "eyeB_R":
                    eyeB_R = val;
                    break;
                case "eyeC_L":
                    eyeC_L = val;
                    break;
                case "eyeC_R":
                    eyeC_R = val;
                    break;
                case "eyeD_L":
                    eyeD_L = val;
                    break;
                case "eyeD_R":
                    eyeD_R = val;
                    break;
                case "jaw":
                    jaw = val;
                    break;
                case "lipsA_L":
                    lipsA_L = val;
                    break;
                case "lipsA_R":
                    lipsA_R = val;
                    break;
                case "lipsB_L":
                    lipsB_L = val;
                    break;
                case "lipsB_R":
                    lipsB_R = val;
                    break;
                case "lipsC_L":
                    lipsC_L = val;
                    break;
                case "lipsC_R":
                    lipsC_R = val;
                    break;
                case "lipsD_L":
                    lipsD_L = val;
                    break;
                case "lipsD_R":
                    lipsD_R = val;
                    break;
                case "nose":
                    nose = val;
                    break;
                case "nose_L":
                    nose_L = val;
                    break;
                case "nose_R":
                    nose_R = val;
                    break;
            }
        }
    }
    public class HumanHandArmature
    {

    }
    public class HumanFootArmature
    {

    }

    public class DynamicArmature
    {
        public transform root;
        public Dictionary<string, transform> dictionary;
        public List<string> bones;
        public void Add(string key, transform val)
        {
            dictionary.Add(key, val);
            bones.Add(key);
        }
        public void Add(string key, string val)
        {
            string[] split = val.Split('/');
            if (split.Length == 2)
            {
                transform t = dictionary[split[0]];
                if (t != null)
                {
                    dictionary.Add(key, t.find(split[1]));
                    bones.Add(key);
                }
            }
        }
    }
    public class SimpleArmature
    {
        public transform root;
        public transform[] points;
        public SimpleArmaturePose rest_pose;
        public transform this[int i]
        {
            get
            {
                if (i < 0 || points == null || i >= points.Length) return null;
                return points[i];
            }
            set
            {
                if (i < 0 || points == null || i >= points.Length) return;
                points[i] = value;
            }
        }
        public transform this[string s]
        {
            get { return this[s.Length]; }
            set { this[s.Length] = value; }
        }
    }
    public class SimpleArmaturePose
    {
        public BonePose[] positions;
        public BonePose this[int i]
        {
            get
            {
                if (i < 0 || positions == null || i >= positions.Length) return null;
                return positions[i];
            }
            set
            {
                if (i < 0 || positions == null || i >= positions.Length) return;
                positions[i] = value;
            }
        }
        public BonePose this[string s]
        {
            get { return this[s.Length]; }
            set { this[s.Length] = value; }
        }
    }
}

