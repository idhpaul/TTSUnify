using TTSUnify.Core.Attributes;

namespace TTSUnify.Core.Enums
{

    public enum GENDERVOICE
    {
        Male,
        Female
    }

    public enum OPENAIVOICE
    {
        [Gender(GENDERVOICE.Male),Description("alloy")]
        m_Alloy,
        [Gender(GENDERVOICE.Male), Description("echo")]
        m_Echo,
        [Gender(GENDERVOICE.Male), Description("fable")]
        m_Fable,
        [Gender(GENDERVOICE.Male), Description("onyx")]
        m_Onyx,
        [Gender(GENDERVOICE.Female), Description("nova")]
        f_Nova,
        [Gender(GENDERVOICE.Female), Description("shimmer")]
        f_Shimmer,
    }

    public enum GOOGLEVOICE
    {
        [Gender(GENDERVOICE.Female), Description("ko-KR-Standard-A")]
        f_A_Standard,
        [Gender(GENDERVOICE.Female), Description("ko-KR-Standard-B")]
        f_B_Standard,
        [Gender(GENDERVOICE.Male), Description("ko-KR-Standard-C")]
        m_C_Standard,
        [Gender(GENDERVOICE.Male), Description("ko-KR-Standard-D")]
        m_D_Standard,
    }

    public enum AZUREVOICE
    {
        [Gender(GENDERVOICE.Male), Description("ko-KR-InJoonNeural")]
        m_InJoon,
        [Gender(GENDERVOICE.Male), Description("ko-KR-BongJinNeural")]
        m_BongJin,
        [Gender(GENDERVOICE.Male), Description("ko-KR-GookMinNeural")]
        m_GookMin,
        [Gender(GENDERVOICE.Male), Description("ko-KR-HyunsuNeural")]
        m_Hyunsu,
        [Gender(GENDERVOICE.Female), Description("ko-KR-SunHiNeural")]
        f_SunHi,
        [Gender(GENDERVOICE.Female), Description("ko-KR-JiMinNeural")]
        f_JiMin,
        [Gender(GENDERVOICE.Female), Description("ko-KR-SoonBokNeural")]
        f_SoonBok,
        [Gender(GENDERVOICE.Female), Description("ko-KR-YuJinNeural")]
        f_YuJin,
    }
}
