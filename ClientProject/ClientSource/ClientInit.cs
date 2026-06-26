namespace Neurotrauma
{
    // Clientside code ONLY!
    public partial class NeurotraumaInit
    {
        public void InitClientOnly()
        {
            ConfigurationMenu.AddConfigToPauseMenu();
            DynamicItems.InitDynamicItemsClient();
        }
    }
}
