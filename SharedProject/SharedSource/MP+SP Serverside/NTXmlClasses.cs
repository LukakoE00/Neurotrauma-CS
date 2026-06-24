using static Neurotrauma.HF;
using Barotrauma.Items.Components;

namespace Neurotrauma
{
    /// <summary>
    /// Calls an NT Function Defined in the NTXmlMethods
    /// </summary>
    public class NTCall : ItemComponent
    {
        public string MyAttribute { get; init; }
        public NTCall(Item item, ContentXElement element) : base(item, element)
        {
            MyAttribute = element.GetAttributeString("NTFunc", "default value");
            var Method = typeof(NTXmlMethods).GetMethod(MyAttribute);
            if (Method != null)
            {
                Method.Invoke(this, new object[] { });
            }
        }
    }

    public static class NTXmlMethods
    {
        public static void PrintExample()
        {
            Print("THIS IS AN EXAMPLE");
        }
    }

}