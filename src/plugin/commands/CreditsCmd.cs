using api.plugin;
using api.plugin.models;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Menu;
using plugin.extensions;
using plugin.menus;

namespace plugin.commands;

public class CreditsCmd(ICS2Gangs gangs) : Command(gangs)
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
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                "SteamID not authorized yet. Try again in a few seconds.");
            return;
        }

        if (info.ArgCount <= 1)
        {
            GangPlayer? playerInfo = gangs.GetGangsService().GetGangPlayer(steam.SteamId64).GetAwaiter()
                .GetResult();

            if (playerInfo == null)
            {
                info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                    "You were not found in the database. Try again in a few seconds.");
                return;
            }

            info.ReplyLocalized(gangs.GetBase().Localizer, "command_credits", playerInfo.Credits);
            return;
        }

        TargetResult? target = GetTarget(info);
        if (target == null)
        {
            info.ReplyLocalized(gangs.GetBase().Localizer, "command_error", "Player not found.");
            return;
        }

        foreach (var player in target.Players)
        {
            GangPlayer? playerInfo = gangs.GetGangsService().GetGangPlayer(player.SteamID).GetAwaiter()
                .GetResult();

            if (playerInfo == null)
            {
                info.ReplyLocalized(gangs.GetBase().Localizer, "command_error",
                    "Could not load information for player. Try again in a few seconds.");
                return;
            }

            info.ReplyLocalized(gangs.GetBase().Localizer, "command_credits_other", player.PlayerName, playerInfo.Credits);
        }
    }
}