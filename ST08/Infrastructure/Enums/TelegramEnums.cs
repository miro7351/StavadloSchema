using System;

namespace PA.Stavadlo.MH.Enums
{
    [Flags]
    public enum STAV_NAVESTIDLA
    {
        CERVENA = 0x0001,
        ZELENA = 0x0002,
        BIELA = 0x0004,
        OBS_KO = 0x0008,
        PRECHODOVY_STAV = 0x0040,
        PORUCHA = 0x0080,
    }

    [Flags]
    public enum STAV_VYMENA
    {
        PLUS = 0x0001,
        MINUS = 0x0002,
        ZAVER = 0x0004,
        OBS_KO = 0x0008,
        ROZREZ = 0x0010,
        NADPRUD = 0x0020,
        MANIPULACIA = 0x0040,
        PORUCHA = 0x0080
    }

    public enum VYLUKA_NAVESTIDLA
    {
        BEZ_VYLUKY = 0,
        VO_VYLUKE = 1
    }

    public enum VYLUKA_VYMENY
    {
        BEZ_VYLUKY = 0,
        VO_VYLUKE = 1
    }

}
