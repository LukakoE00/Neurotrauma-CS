using Barotrauma;
using static Neurotrauma.HF;


namespace Neurotrauma;

public class NeurotraumaHuman
{
    public Character Human { get; private set; }
    public List<Affliction> AfflictionsLimbSpecific { get; private set; }
    public List<Affliction> AfflictionsNonLimbSpecific { get; private set; }
    public List<Affliction> Symptoms { get; private set; }
    public List<String> Stats { get; private set; }

    public NeurotraumaHuman(Character Human)
    {
        this.Human = Human;
    }
}