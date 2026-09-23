using System.Text.Json;

namespace Neurotrauma;

public class NTAnalyzer
{

    private static readonly string ConfigDirectoryPath = Path.Combine(SaveUtil.DefaultSaveFolder, "ModConfigs").Replace('\\', '/');
    private string ConfigFilePath;

    private string FileName;

    private Dictionary<string, List<long>> Data;

    public NTAnalyzer(string fileName)
    {
        this.FileName = fileName;
        this.ConfigFilePath = Path.Combine(ConfigDirectoryPath, FileName).Replace('\\', '/');
        this.Data = new Dictionary<string, List<long>>();

        Load();

    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(ConfigDirectoryPath))
            {
                Directory.CreateDirectory(ConfigDirectoryPath);
            }

            string json = JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            LuaCsLogger.LogError("[Neurotrauma] Error saving Stats: " + ex.Message);
        }
    }

    public void Add(string AfflictionID, long TickTime)
    {
        if (!this.Data.ContainsKey(AfflictionID))
        {
            this.Data.Add(AfflictionID, new List<long>());
        }

        this.Data[AfflictionID].Add(TickTime);

        
    }

    public void Load()
    {

        if (!File.Exists(ConfigFilePath))
        {
            return;
        }
        
        try
        {

            string jsonContent = File.ReadAllText(ConfigFilePath);
            Dictionary<string, List<long>>? readConfig = JsonSerializer.Deserialize<Dictionary<string, List<long>>>(jsonContent);

            if (readConfig == null) return;

            this.Data = readConfig;

        }
        catch (Exception ex)
        {
            HF.PrintError("Error loading config: " + ex.Message);
        }
    }
}