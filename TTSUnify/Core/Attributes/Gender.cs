using TTSUnify.Core.Enums;

namespace TTSUnify.Core.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class GenderAttribute : Attribute
    {
        public GENDERVOICE Gender { get; }

        public GenderAttribute(GENDERVOICE gender)
        {
            Gender = gender;
        }
    }
}
