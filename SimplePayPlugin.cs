using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;
using OpenMod.Core.Commands;
using OpenMod.API.Commands;
using UnityEngine.Android;

[assembly: PluginMetadata("Tanese.SimplePayPlugin", DisplayName = "Simple Pay Plugin")]

namespace SimplePayPluginNamespace
{
    public class SimplePayPlugin : OpenModUnturnedPlugin
    {
        public SimplePayPlugin(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnLoadAsync()
        {
            Logger.LogInformation("SimplePayPlugin has been loaded!");
            await UniTask.CompletedTask;
        }

        protected override async UniTask OnUnloadAsync()
        {
            Logger.LogInformation("SimplePayPlugin has been unloaded!");
            await UniTask.CompletedTask;
        }
    }

    [Command("adminpay")]
    [CommandSyntax("<player> <amount>")]
    public class CommandAdminPay : OpenMod.Core.Commands.Command
    {
        private readonly SimplePayPlugin m_Plugin;
        private readonly IUnturnedUserDirectory m_userDirectory;

        public CommandAdminPay(
            IUnturnedUserDirectory userDirectory,
            SimplePayPlugin plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Plugin = plugin;
            m_userDirectory = userDirectory;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            UnturnedUser? target = m_userDirectory.FindUser(await Context.Parameters.GetAsync<string>(0), OpenMod.API.Users.UserSearchMode.FindByNameOrId);

            if (target == null)
            {
                throw new UserFriendlyException("No player found!");
            }

            uint amount = await Context.Parameters.GetAsync<uint>(1);

            if (amount < 1)
            {
                throw new CommandWrongUsageException(Context);
            }

            target.Player.Player.skills.askAward(amount);

            await user.PrintMessageAsync($"[Payments] You have given {amount}$ to {target.Player.Player.channel.owner.playerID.characterName}");
            await UniTask.CompletedTask;
        }
    }

    [Command("money")]
    public class CommandMoney : OpenMod.Core.Commands.Command
    {
        private readonly SimplePayPlugin m_plugin;
        private readonly IUnturnedUserDirectory m_userDirectory;

        public CommandMoney(
            IUnturnedUserDirectory userDirectory,
            SimplePayPlugin plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_plugin = plugin;
            m_userDirectory = userDirectory;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            await user.PrintMessageAsync($"[Payments] You currently have {user.Player.Player.skills.experience}$");
            await UniTask.CompletedTask;
        }
    }

    [Command("pay")]
    [CommandSyntax("<player> <amount>")]
    public class CommandPay : OpenMod.Core.Commands.Command
    {
        private readonly SimplePayPlugin m_plugin;
        private readonly IUnturnedUserDirectory m_userDirectory;

        public CommandPay(
            IUnturnedUserDirectory userDirectory,
            SimplePayPlugin plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_plugin = plugin;
            m_userDirectory = userDirectory;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            UnturnedUser? target = m_userDirectory.FindUser(await Context.Parameters.GetAsync<string>(0), OpenMod.API.Users.UserSearchMode.FindByNameOrId);

            if (target == null)
            {
                throw new UserFriendlyException("[Payments] No player found!");
            }

            uint amount = await Context.Parameters.GetAsync<uint>(1);

            if (amount < 1)
            {
                throw new CommandWrongUsageException(Context);
            }
            if (amount > user.Player.Player.skills.experience)
            {
                throw new UserFriendlyException("[Payments] You don't have enough money to complete this transaction!");
            }
            if (target == Context.Actor)
            {
                throw new UserFriendlyException("[Payments] You can't pay yourself!");
            }

            user.Player.Player.skills.askSpend(amount);
            target.Player.Player.skills.askAward(amount);

            await target.PrintMessageAsync($"[Payments] {user.Player.Player.channel.owner.playerID.characterName} has paid you {amount}$");
            await user.PrintMessageAsync($"[Payments] You have paid {amount}$ to {target.Player.Player.channel.owner.playerID.characterName}");
            await UniTask.CompletedTask;
        }
    }

    [Command("seeplayermoney")]
    [CommandSyntax("<player>")]
    public class CommandSeePlayerMoney : OpenMod.Core.Commands.Command
    {
        private readonly SimplePayPlugin m_plugin;
        private readonly IUnturnedUserDirectory m_userDirectory;

        public CommandSeePlayerMoney(
            IUnturnedUserDirectory userDirectory,
            SimplePayPlugin plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_plugin = plugin;
            m_userDirectory = userDirectory;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            UnturnedUser? target = m_userDirectory.FindUser(await Context.Parameters.GetAsync<string>(0), OpenMod.API.Users.UserSearchMode.FindByNameOrId);

            if (target == null)
            {
                throw new UserFriendlyException("[Payments] No player found!");
            }

            await user.PrintMessageAsync($"[Payments] {target.Player.Player.channel.owner.playerID.characterName} has {target.Player.Player.skills.experience}$");
            await UniTask.CompletedTask;
        }
    }
}