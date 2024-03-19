using Verse;

namespace VanillaTemperatureExpanded.Buildings;

public class Building_CeilingFan : Building
{
    public float angle;

    public override void Draw()
    {
        Graphic.Draw(DrawPos, Rotation, this, angle);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref angle, "angle");
    }
}
