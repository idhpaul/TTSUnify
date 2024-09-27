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
        [Gender(GENDERVOICE.Male),Description("Alloy")]
        m_Alloy,
        [Gender(GENDERVOICE.Male), Description("Echo")]
        m_Echo,
        [Gender(GENDERVOICE.Male), Description("Fable")]
        m_Fable,
        [Gender(GENDERVOICE.Male), Description("Onyx")]
        m_Onyx,
        [Gender(GENDERVOICE.Female), Description("Nova")]
        f_Nova,
        [Gender(GENDERVOICE.Female), Description("Shimmer")]
        f_Shimmer,
    }

    public enum GOOGLEVOICE
    {
        [Gender(GENDERVOICE.Female), Description("여1/ko-KR-Standard-A")]
        f_A_Standard,
        [Gender(GENDERVOICE.Female), Description("여2/ko-KR-Standard-B")]
        f_B_Standard,
        [Gender(GENDERVOICE.Male), Description("남1/ko-KR-Standard-C")]
        m_C_Standard,
        [Gender(GENDERVOICE.Male), Description("남2/ko-KR-Standard-D")]
        m_D_Standard,
    }

    public enum AZUREVOICE
    {
        [Gender(GENDERVOICE.Male), Description("인준/ko-KR-InJoonNeural")]
        m_InJoon,
        [Gender(GENDERVOICE.Male), Description("봉진/ko-KR-BongJinNeural")]
        m_BongJin,
        [Gender(GENDERVOICE.Male), Description("국민/ko-KR-GookMinNeural")]
        m_GookMin,
        [Gender(GENDERVOICE.Male), Description("현수/ko-KR-HyunsuNeural")]
        m_Hyunsu,
        [Gender(GENDERVOICE.Female), Description("선히/ko-KR-SunHiNeural")]
        f_SunHi,
        [Gender(GENDERVOICE.Female), Description("지민/ko-KR-JiMinNeural")]
        f_JiMin,
        [Gender(GENDERVOICE.Female), Description("순복/ko-KR-SoonBokNeural")]
        f_SoonBok,
        [Gender(GENDERVOICE.Female), Description("유진/ko-KR-YuJinNeural")]
        f_YuJin,
    }
}
