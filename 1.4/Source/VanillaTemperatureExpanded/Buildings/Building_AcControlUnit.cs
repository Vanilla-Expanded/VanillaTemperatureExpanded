using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaTemperatureExpanded.Buildings;

//TODO actually use
public class Building_AcControlUnit : Building
{
    public CompFacility facilityComp;
    public CompPowerTrader powerComp;
    public CompResource resourceComp;

    public float AcEfficiency
    {
        get
        {
            if (resourceComp.PipeNet is AcPipeNet)
            {
                
            }

            return 0f;
        }
    }
    
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        facilityComp = GetComp<CompFacility>();
        resourceComp = GetComp<CompResource>();
        powerComp = GetComp<CompPowerTrader>();
    }

    public override void TickRare()
    {
        
    }
}