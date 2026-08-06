using Neurotrauma;

public static class NTSurgeryTable
{
    private const int NormalHeartrate = 60;
    private const int MaxTachycardiaHeartrate = 180;
    private const int MaxFibrillationHeartrate = 300;

    private static readonly Random random = new Random();

    // Pulls the Heartrate of a character
    private static float GetHeartrate(Character character)
    {
        // Heartrate is 0 if not human or dead
        if (character == null || character.CharacterHealth == null || character.IsDead)
        {
            return 0;
        }

        float Heartrate = NormalHeartrate;

        // Heartrate is 0 if in Cardiac Arrest
        var cardiacarrest = character.CharacterHealth.GetAffliction("cardiacarrest");
        if (cardiacarrest != null && cardiacarrest.Strength >= 0.5f)
        {
            return 0;
        }

        // Adjust based on fibrillation status
        var IncreasedHeartrate = character.CharacterHealth.GetAffliction("increasedheartrate");
        var Fibrillation = character.CharacterHealth.GetAffliction("fibrillation");

        if (Fibrillation != null)
        {
            Heartrate = Single.Lerp(MaxTachycardiaHeartrate, MaxFibrillationHeartrate, Fibrillation.Strength / 100f * (1 + (float)random.NextDouble() * 0.5f));
        }
        else if (IncreasedHeartrate != null)
        {
            Heartrate = Single.Lerp(NormalHeartrate, MaxTachycardiaHeartrate, IncreasedHeartrate.Strength / 100f);
        }

        return Heartrate;
    }

    // Gets Acidosis / Alkalosis value
    private static float GetPH(Character character)
    {
        if (character == null || character.CharacterHealth == null)
        {
            return 0;
        }

        float Acidosis = HF.GetAfflictionStrength(character, "acidosis", 0);
        float Alkalosis = HF.GetAfflictionStrength(character, "alkalosis", 0);

        return Alkalosis - Acidosis;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public static void InitializeSurgeryTableHooks()
    {
        LuaCsSetup.Instance.Hook.Add("surgerytable.update", "surgerytable.update", (params object[] args) =>
        {
            var Effect = args[0];

            var DeltaTime = args[1];

            var Item = args[2] as Barotrauma.Item;

            var Targets = args[3] as IEnumerable<Barotrauma.Character>;

            var Position = args[4];

            // Fetch controller component
            var ControllerComponent = Item.GetComponentString("Controller");

            if (ControllerComponent == null)
            {
                Item.SendSignal("0", "state_out");
                return null;
            }

            // Check if anyone is laying on the table
            // Let me do this like lua, fucking tupid
            var Sleeper = ControllerComponent.GetType().GetProperty("User");
            var Target = Sleeper?.GetValue(ControllerComponent) as Barotrauma.Character;

            // If no one on the table, find the character with the least vitality in the targets
            if (Target == null || !Target.IsHuman)
            {
                float minVitality = 999f;

                if (Targets != null)
                {
                    foreach (var value in Targets)
                    {
                        if (value.Name != null && value.IsHuman && value.Vitality < minVitality)
                        {
                            minVitality = value.Vitality;
                            Target = value;
                        }
                    }
                }
            }

            // No target found
            if (Target == null || !Target.IsHuman)
            {
                Item.SendSignal("0", "state_out");
                return null;
            }

            // Send signals:
            // Is there a character to check right now?
            Item.SendSignal("1", "state_out");

            // Is currently alive?
            Item.SendSignal(Target.IsDead ? "0" : "1", "alive_out");

            // Is currently unconscious?
            Item.SendSignal(Target.IsDead || HF.HasAffliction(Target, "unconsciousness", 0.1f) ? "0" : "1", "conscious_out");

            // What is the character name?
            Item.SendSignal(Target.Name, "name_out");

            // What is the character's vitality?
            Item.SendSignal(MathF.Round(Target.Vitality).ToString(), "vitality_out");

            // What is the character's blood pressure?
            Item.SendSignal(Target.IsDead ? "0" : MathF.Round(HF.GetAfflictionStrength(Target, "bloodpressure", 100)).ToString(), "bloodpressure_out");

            // What is their current blood 02 level?
            Item.SendSignal(MathF.Round(100 - HF.GetAfflictionStrength(Target, "hypoxemia", 0)).ToString(), "bloodoxygen_out");

            // What is their current amount of Neurotrauma?
            Item.SendSignal(MathF.Round(HF.GetAfflictionStrength(Target, "neurotrauma", 0)).ToString(), "neurotrauma_out");

            // What is their current amount of VANILLA organ damage?
            Item.SendSignal(MathF.Round(HF.GetAfflictionStrength(Target, "organdamage", 0)).ToString(), "organdamage_out");

            // What is their current heartrate?
            Item.SendSignal(MathF.Round(GetHeartrate(Target)).ToString(), "heartrate_out");

            // Determine breathing rate
            int BreathingRate = random.Next(15, 19);
            // Not breathing if dead
            if (HF.HasAffliction(Target, "respiratoryarrest") || Target.IsDead)
            {
                BreathingRate = 0;
            }
            else if (HF.HasAffliction(Target, "hyperventilation"))
            {
                BreathingRate += random.Next(6, 9);
            }
            else if (HF.HasAffliction(Target, "hypoventilation"))
            {
                BreathingRate -= random.Next(6, 9);
            }

            // What is their current breathing rate?
            Item.SendSignal(BreathingRate.ToString(), "breathingrate_out");

            // Are they in surgery?
            Item.SendSignal(HF.BoolToNum(HF.HasAffliction(Target, "surgeryincision")).ToString(), "insurgery_out");

            // If dead, what was the cause of death?
            if (Target.IsDead && Target.CauseOfDeath != null)
            {
                Item.SendSignal(HF.CauseOfDeathToString(Target.CauseOfDeath), "causeofdeath_out");
            }

            // What is their Alkalosis/Acidosis value right now?
            Item.SendSignal(MathF.Round(GetPH(Target)).ToString(), "bloodph_out");

            return null;
        });
    }
}