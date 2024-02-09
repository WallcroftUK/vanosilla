using System.Collections.Generic;
using System.Text;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.Quicklist
{
    public static class QuicklistPacketExtensions
    {
        public static void RefreshQuicklist(this IClientSession session) => session.SendPackets(session.GenerateQuicklist());


        public static IEnumerable<string> GenerateQuicklist(this IClientSession session)
        {
            // to rework
            StringBuilder[] pktQs = { new("qslot 0"), new("qslot 1") };

            int morphId = session.PlayerEntity.UseSp ? session.PlayerEntity.Specialist?.GameItem.Morph ?? 0 : 0;

            for (short i = 0; i < 30; i++)
            {
                for (short j = 0; j < 2; j++)
                {
                    IReadOnlyList<CharacterQuicklistEntryDto> tmp = session.PlayerEntity.QuicklistComponent.GetQuicklistByTab(j, morphId);

                    if (tmp?[i] == null)
                    {
                        pktQs[j].Append(" 0.-1.-1");
                        continue;
                    }

                    CharacterQuicklistEntryDto qi = tmp[i];
                    pktQs[j].Append($" {(byte?)qi?.Type ?? 0}.{qi?.InventoryTypeOrSkillTab ?? -1}.{qi?.InvSlotOrSkillSlotOrSkillVnum.ToString() ?? "-1"}");
                }
            }

            return new[] { pktQs[0].ToString(), pktQs[1].ToString() };
        }

        public static void SendQuicklistSlot(this IClientSession session, CharacterQuicklistEntryDto entry, short? skillCastId = null)
        {
            session.SendPacket(session.GenerateQsetPacket(entry, skillCastId));
        }

        public static string GenerateQsetPacket(this IClientSession session, CharacterQuicklistEntryDto quicklistEntryDto, short? skillCastId = null) =>
            $"qset {quicklistEntryDto.QuicklistTab} {quicklistEntryDto.QuicklistSlot} {(byte)quicklistEntryDto.Type}.{quicklistEntryDto.InventoryTypeOrSkillTab}.{skillCastId ?? quicklistEntryDto.InvSlotOrSkillSlotOrSkillVnum}.0";

        public static void SendEmptyQuicklistSlot(this IClientSession session, short tab, short slot)
        {
            session.SendPacket(session.GenerateEmptyQsetPacket(tab, slot));
        }

        public static string GenerateEmptyQsetPacket(this IClientSession session, short tab, short slot) => $"qset {tab} {slot} 0.-1.-1.0";
    }
}