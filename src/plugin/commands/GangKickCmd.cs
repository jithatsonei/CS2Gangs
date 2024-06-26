using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;
using plugin.services;
using plugin.utils;

namespace plugin.commands;

public class GangKickCmd(ICS2Gangs gangs) : Command(gangs)
{
    public override void OnCommand(CCSPlayerController? executor, CommandInfo info)
    {
        if (executor == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_execute_in_game");
            return;
        }
        if (!executor.IsReal())
            return;

        var steam = executor.AuthorizedSteamID;
        if (steam == null)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "SteamID not authorized yet. Try again in a few seconds.");
            return;
        }

        if (info.ArgCount <= 1)
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_usage",
                "css_gangkick <SteamID>");
            return;
        }

        if(!ulong.TryParse(info.GetArg(1), out ulong targetSteamId))
        {
            executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                "Invalid SteamID.");
            return;
        }

        Task.Run(async () => {
            GangPlayer? senderPlayer = await gangs.GetGangsService().GetGangPlayer(steam.SteamId64);
            if (senderPlayer == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You were not found in the database. Try again in a few seconds.");
                });
                return;
            }
            if (senderPlayer.GangId == null) {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You are not in a gang.");
                });
                return;
            }

            Gang? senderGang = await gangs.GetGangsService().GetGang(senderPlayer.GangId.Value);
            if (senderGang == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Your gang was not found in the database. Try again in a few seconds.");
                });
                return;
            }

            GangPlayer? targetPlayer = await gangs.GetGangsService().GetGangPlayer(targetSteamId);
            if (targetPlayer == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player not found in the database.");
                });
                return;
            }

            if (targetPlayer.GangId == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player is not in a gang.");
                });
                return;
            }

            Gang? targetGang = await gangs.GetGangsService().GetGang(targetPlayer.GangId.Value);
            if (targetGang == null)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player's gang was not found in the database.");
                });
                return;
            }

            if (senderPlayer.GangRank <= (int?)GangRank.Member)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You must at least be an officer to kick a player.");
                });
                return;
            }

            if (senderPlayer.GangId != targetPlayer.GangId)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "Player is not in your gang.");
                });
                return;
            }

            if (senderPlayer.SteamId == targetPlayer.SteamId)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You cannot kick yourself!");
                });
                return;
            }

            if (targetPlayer.GangRank >= senderPlayer.GangRank)
            {
                Server.NextFrame(() => {
                    executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_error",
                        "You cannot kick a player with the same or higher rank than you.");
                });
                return;
            }

            targetPlayer.GangId = null;
            targetPlayer.GangRank = null;
            targetPlayer.InvitedBy = null;

            gangs.GetGangsService().PushPlayerUpdate(targetPlayer);

            Server.NextFrame(() => {
                if (!executor.IsReal())
                    return;
                executor.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangkick_success", targetPlayer.PlayerName ?? "Unknown");
                CCSPlayerController target = Utilities.GetPlayerFromSteamId((ulong)targetPlayer.SteamId);
                if (target != null)
                {
                    target.PrintLocalizedChat(gangs.GetBase().Localizer, "command_gangkick_kicked", targetGang.Name);
                }
                gangs.GetAnnouncerService().AnnounceToGangLocalized(senderGang, gangs.GetBase().Localizer, "gang_announce_kick", targetPlayer.PlayerName ?? "Unknown", senderPlayer.PlayerName ?? "Unknown");
            });

        });
    }
}