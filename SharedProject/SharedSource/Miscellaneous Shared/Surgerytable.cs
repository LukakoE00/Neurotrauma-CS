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
            var effect = args[0];

            var deltaTime = args[1];

            var item = args[2] as Barotrauma.Item;

            var targets = args[3] as IEnumerable<Barotrauma.Character>;

            var position = args[4];

            // Fetch controller component
            var controllerComponent = item.GetComponentString("Controller");

            if (controllerComponent == null)
            {
                item.SendSignal("0", "state_out");
                return null;
            }

            // Check if anyone is laying on the table
            var target = (controllerComponent as dynamic)?.User as Character;

            // If no one on the table, find the character with the least vitality in the targets
            if (target == null || !target.IsHuman)
            {
                float minVitality = 999f;
                if (targets != null)
                {
                    foreach (var value in targets)
                    {
                        if (value?.Name != null && value.IsHuman && value.Vitality < minVitality)
                        {
                            minVitality = value.Vitality;
                            target = value;
                        }
                    }
                }
            }

            // No target found
            if (target == null || !target.IsHuman)
            {
                item.SendSignal("0", "state_out");
                return null;
            }

            // Send signals:
            // Is there a character to check right now?
            item.SendSignal("1", "state_out");

            // Is currently alive?
            item.SendSignal(target.IsDead ? "0" : "1", "alive_out");

            // Is currently unconscious?
            item.SendSignal(target.IsDead || HF.HasAffliction(target, "unconsciousness", 0.1f) ? "0" : "1", "conscious_out");

            // What is the character name?
            item.SendSignal(target.Name, "name_out");

            // What is the character's vitality?
            item.SendSignal(MathF.Round(target.Vitality).ToString(), "vitality_out");

            // What is the character's blood pressure?
            item.SendSignal(target.IsDead ? "0" : MathF.Round(HF.GetAfflictionStrength(target, "bloodpressure", 100)).ToString(), "bloodpressure_out");

            // What is their current blood 02 level?
            item.SendSignal(MathF.Round(100 - HF.GetAfflictionStrength(target, "hypoxemia", 0)).ToString(), "bloodoxygen_out");

            // What is their current amount of Neurotrauma?
            item.SendSignal(MathF.Round(HF.GetAfflictionStrength(target, "cerebralhypoxia", 0)).ToString(), "neurotrauma_out");

            // What is their current amount of VANILLA organ damage?
            item.SendSignal(MathF.Round(HF.GetAfflictionStrength(target, "organdamage", 0)).ToString(), "organdamage_out");

            // What is their current heartrate?
            item.SendSignal(MathF.Round(GetHeartrate(target)).ToString(), "heartrate_out");

            // Determine breathing rate
            int BreathingRate = random.Next(15, 19);
            // Not breathing if dead
            if (HF.HasAffliction(target, "respiratoryarrest") || target.IsDead)
            {
                BreathingRate = 0;
            }
            else if (HF.HasAffliction(target, "hyperventilation"))
            {
                BreathingRate += random.Next(6, 9);
            }
            else if (HF.HasAffliction(target, "hypoventilation"))
            {
                BreathingRate -= random.Next(6, 9);
            }

            // What is their current breathing rate?
            item.SendSignal(BreathingRate.ToString(), "breathingrate_out");

            // Are they in surgery?
            item.SendSignal(HF.BoolToNum(HF.HasAffliction(target, "surgeryincision")).ToString(), "insurgery_out");

            // If dead, what was the cause of death?
            if (target.IsDead && target.CauseOfDeath != null)
            {
                item.SendSignal(HF.CauseOfDeathToString(target.CauseOfDeath), "causeofdeath_out");
            }

            // What is their Alkalosis/Acidosis value right now?
            item.SendSignal(MathF.Round(GetPH(target)).ToString(), "bloodph_out");

            return null;
        });
    }
}