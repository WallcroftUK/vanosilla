using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyIdRequest
    {
        [ProtoMember(1)]
        public long FamilyId { get; set; }
    }
}