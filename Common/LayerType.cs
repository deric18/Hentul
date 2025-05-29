namespace Common
{
    public enum LayerType
    {
        Layer_4,
        Layer_3A,
        Layer_3B,
        UNKNOWN
    }

    public enum SchemaType
    {
        FOMSCHEMA,
        SOMSCHEMA_VISION,
        SOMSCHEMA_TEXT,
        INVALID
    }

    public enum LogMode
    {
        All,
        Info,
        Trace,
        BurstOnly,        
        None,
    }
}
