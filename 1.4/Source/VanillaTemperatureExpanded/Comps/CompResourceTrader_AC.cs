using System.Collections.Generic;
using PipeSystem;
using Verse;

namespace VanillaTemperatureExpanded;

public class CompResourceTrader_AC : CompResourceTrader
{
    //TODO: implement
}
public class CompResourceTrader_Compressor : CompResourceTrader
{
    public override bool CanBeOn()
    {
        return base.CanBeOn() && !RoofUtility.IsAnyCellUnderRoof(parent);
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
        if (RoofUtility.IsAnyCellUnderRoof(parent))
        {
            this.ResourceOn = false;
        }
    }
}