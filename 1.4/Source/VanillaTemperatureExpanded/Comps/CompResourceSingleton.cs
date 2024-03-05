using PipeSystem;
using RimWorld;

namespace VanillaTemperatureExpanded;

public class CompProperties_ResourceSingleton : CompProperties_Resource
{
    public CompProperties_ResourceSingleton()
    {
        compClass = typeof(CompResourceSingleton);
    }
}

public class CompResourceSingleton : CompResource
{
    public OverlayHandle? overlayHandle;

    private PipeNetOverlayDrawer pipeNetOverlayDrawer;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        this.UpdateOverlayHandle();
        pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();
    }

    public void UpdateOverlayHandle()
    {
        if (!this.parent.Spawned)
        {
            return;
        }

        // this.parent.Map.overlayDrawer.Disable(this.parent, ref this.overlayHandle);

        //TODO: test if togglepulsing actually works
        if (this.parent.Spawned && this.PipeNet is AcPipeNet acPipeNet &&
            acPipeNet.singletonDict[this.parent.def].Count > 1)
        {
            pipeNetOverlayDrawer.TogglePulsing(this.parent, null, true);
            // this.overlayHandle =
            //     new OverlayHandle?(this.parent.Map.overlayDrawer.Enable(this.parent, OverlayTypes.BrokenDown));
        }
        else
        {
            
            pipeNetOverlayDrawer.TogglePulsing(this.parent, null, false);
        }
    }
}