// Put in a shared folder/namespace both Orchestrator and HC can see.
using Hentul;
using Hentul.Hippocampal_Entorinal_complex;

public sealed class VisualSensationBundle
{
    public Sensation_Location V1 { get; }
    public Sensation_Location? V2 { get; }
    public Sensation_Location? V3 { get; }
    public Orchestrator.POINT Cursor { get; }

    public VisualSensationBundle(
        Sensation_Location v1,
        Sensation_Location? v2,
        Sensation_Location? v3,
        Orchestrator.POINT cursor)
    {
        V1 = v1 ?? throw new ArgumentNullException(nameof(v1));
        V2 = v2;
        V3 = v3;
        Cursor = cursor;
    }
}
