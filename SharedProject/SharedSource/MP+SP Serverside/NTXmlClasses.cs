using Barotrauma.Items.Components;
using FarseerPhysics.Collision;
using System.Xml.Linq;
using static Neurotrauma.HF;

namespace Neurotrauma
{
    /// <summary>
    /// Calls an NT Function Defined in the NTXmlMethods
    /// </summary>
    public class NTCall : StatusEffect
    {
        public string MyAttribute { get; init; }
        public ContentXElement Element;
        public NTCall(ContentXElement element, string printDebugName) : base(element, printDebugName)
        {
            HF.Print("init mate");
            MyAttribute = element.GetAttributeString("NTMethod", "default value");
            Element = element;
        }

        protected void Apply(float deltaTime, Entity entity, IReadOnlyList<ISerializableEntity> targets, Vector2? worldPosition = null)
        {
            HF.Print("use");
            Dictionary<string, object> MyValue = new();

            if (entity is Item EntityItem)
            {
                MyValue["item"] = EntityItem;
            }
            else if (entity is Character EntityCharacter)
            {
                MyValue["character"] = EntityCharacter;
            }

            MyValue["deltatime"] = deltaTime;
            MyValue["targets"] = targets;
            MyValue["worldposition"] = worldPosition;

            foreach (ContentXElement X in Element.Elements())
            {
                if (X.Name != "NTMethod") MyValue[X.Name.ToString()] = Element.GetAttributeString(X.Name.ToString(), "default value");
            }

            if (NTXmlMethods.HasMethod(MyAttribute))
            {
                NTXmlMethods.CallMethod(MyAttribute, MyValue);
                return;
            }
            HF.PrintError("Attempted to invoke method [" + MyAttribute + "] but the method doesn't exist.");
        }
    }

    /// <summary>
    /// Sets an NT property in NTXmlProperties
    /// </summary>
    public class NTSet : ItemComponent
    {
        public string MyAttribute { get; init; }
        public NTSet(Item item, ContentXElement element) : base(item, element)
        {
            MyAttribute = element.GetAttributeString("NTProperty", "default value");
        }
    }

    public static class NTXmlMethods
    {

        private static Dictionary<string,Action<Dictionary<string, object>>> NTCallMethods = new();

        /// <summary>
        /// Adds a new method to be called into the XML Methods.
        /// </summary>
        public static void AddMethod(string Name, Action<Dictionary<string, object>> Method)
        {
            NTCallMethods[Name] = Method;
        }

        public static void RemoveMethod(string Name)
        {
            NTCallMethods.Remove(Name);
        }

        public static void CallMethod(string Name, Dictionary<string, object> _)
        {
            NTCallMethods[Name].Invoke(_);
        }

        public static bool HasMethod(string Name)
        {
            return NTCallMethods.ContainsKey(Name);
        }
    }

    public static class NTXmlProperties
    {
        private static Dictionary<string, object> NTSetProperties = new ();

        public static void AddProperty(string Name, object Value)
        {
            if (NTSetProperties.ContainsKey(Name)) return;
            NTSetProperties[Name] = Value;
        }

        public static void RemoveProperty(string Name)
        {
            if (!NTSetProperties.ContainsKey(Name)) return;
            NTSetProperties.Remove(Name);
        }

        public static bool HasProperty(string Name) 
        { 
            return NTSetProperties.ContainsKey(Name); 
        }

        public static object GetProperty(string Name)
        {
            return NTSetProperties[Name];
        }

        public static void SetProperty(string Name, object Value)
        {
            NTSetProperties[Name] = Value;
        }
    }

}