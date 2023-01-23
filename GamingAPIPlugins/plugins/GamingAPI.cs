using Asyncapi.Nats.Client.Models;
using ConVar;
using Oxide.Ext.GamingApi;
using Oxide.Ext.GamingApi.MessageQueue;
using System;

namespace Oxide.Plugins
{
    [Info("GamingAPI", "jonaslagoni", "0.0.2")]
    [Description("Share all the in-game events with the GamingAPI network")]
    class GamingAPI : RustPlugin
    {
        private void Init()
        {
            Puts("Starting plugin");
            GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.NORMAL, (System.Action success, System.Action failure) =>
            {
                ServerStarted message = new ServerStarted
                {
                    Timestamp = System.DateTime.Now.ToString()
                };
                try
                {
                    Puts("Sending message");
                    DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdEventsStarted(message, DefaultPluginInformation.GetServerId());
                    success();
                }
                catch (Exception e)
                {
                    Puts("Failed sending message");
                    Puts(e.ToString());
                    failure();
                }
            });
        }

        protected override void LoadDefaultConfig()
        {
            Config["ShouldBroadcastNotifications"] = true;
        }


        #region Hooks

        //object OnPlayerChat(BasePlayer player, string message, Chat.ChatChannel channel)
        //{

        //    GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
        //    {
        //        ChatMessage chatMessage = new ChatMessage
        //        {
        //            SteamId = player.UserIDString,
        //            Timestamp = System.DateTime.Now.ToString(),
        //            FullMessage = message,
        //            IsAdmin = player.IsAdmin,
        //            PlayerName = player.displayName
        //        };

        //        DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdEventsPlayerSteamIdChatted(chatMessage, DefaultPluginInformation.GetServerId(), player.UserIDString);
        //    });
        //    return null;
        //}

        //#region Resource gathering
        //private void ResourceGathered(BasePlayer player, Item item)
        //{
        //   GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.NORMAL, (System.Action success, System.Action failed) =>
        //   {
        //       DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsGatheredResources(ConvertToResouceRequestMessage(item, player), DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
        //   });
        //}

        //private ServerPlayerResourceGathered ConvertToResouceRequestMessage(Item item, BasePlayer player)
        //{
        //   ServerPlayerResourceGathered re = new ServerPlayerResourceGathered();
        //   re.Amount = item.amount;
        //   re.GatheredTimestamp = "" + System.DateTime.Now.ToString();
        //   ActiveItem gatheringItem = new ActiveItem
        //   {
        //       Uid = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().uid)),
        //       ItemId = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().info.itemid))
        //   };
        //   PlayerPosition gatheringPosition = new PlayerPosition
        //   {
        //       X = player.transform.position.x,
        //       Y = player.transform.position.y,
        //       Z = player.transform.position.z
        //   };
        //   re.GatheringItem = gatheringItem;
        //   re.GatheringPosition = gatheringPosition;
        //   re.ItemId = item.info.itemid;
        //   re.ItemUid = unchecked((int)item.uid);
        //   re.SteamId = player.IPlayer.Id;
        //   return re;
        //}

        ////Called before the player is given a bonus item for gathering
        //void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        //{
        //   ResourceGathered(player, item);
        //}

        ////Called when the player collects an item
        //object OnCollectiblePickup(CollectibleEntity collectible, BasePlayer player)
        //{
        //   ResourceGathered(player, collectible.GetItem());
        //   return null;
        //}
        ////Called before the player is given items from a resource
        //object OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        //{
        //   ResourceGathered(entity.ToPlayer(), item);
        //   return null;
        //}
        //#endregion

        ///**
        //* Player respawn 
        //**/
        //object OnPlayerRespawned(BasePlayer player)
        //{
        //   GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
        //   {
        //       ServerPlayerRespawned playerRespawn = new ServerPlayerRespawned
        //       {
        //           SteamId = player.IPlayer.Id,
        //           RespawnTimestamp = System.DateTime.Now.ToString(),
        //           RespawnPosition = new PlayerPosition
        //           {
        //               X = player.transform.position.x,
        //               Y = player.transform.position.y,
        //               Z = player.transform.position.z
        //           }
        //       };

        //       DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsRespawned(playerRespawn, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
        //   });
        //    return null;
        //}

        ///**
        //* On player disconnected
        //**/
        //void OnPlayerDisconnected(BasePlayer player, string reason)
        //{
        //   ServerPlayerDisconnected p = new ServerPlayerDisconnected();
        //   p.DisconnectedTimestamp = System.DateTime.Now.ToString();
        //   ServerPlayerDisconnectedPlayer dp = new ServerPlayerDisconnectedPlayer();
        //   dp.Id = player.IPlayer.Id;
        //   p.Player = dp;
        //   p.Reason = reason;
        //   //Must be the lowest importance to ensure all other events are are processed first
        //   GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOWEST, (System.Action success, System.Action failed) =>
        //   {
        //       DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsDisconnected(p, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
        //   });
        //}

        ///**
        //* On player connected
        //**/
        //void OnPlayerConnected(BasePlayer player)
        //{
        //   ServerPlayerConnected p = new ServerPlayerConnected();
        //   p.ConnectedTimestamp = System.DateTime.Now.ToString();
        //   Asyncapi.Nats.Client.Models.Player p2 = new Asyncapi.Nats.Client.Models.Player
        //   {
        //       Id = player.IPlayer.Id,
        //       Name = player.IPlayer.Name,
        //       Address = player.IPlayer.Address
        //   };
        //   p.Player = p2;

        //   GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.IMPORTANT, (System.Action success, System.Action failed) =>
        //   {
        //       DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsConnected(p, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
        //   });
        //}

        ///**
        //* On server wipe
        //**/
        //void OnNewSave(string filename)
        //{
        //   GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.STRICT, (System.Action success, System.Action failed) =>
        //   {
        //       DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdEventsWiped(DefaultPluginInformation.GetServerId());
        //   });
        //}

        //object OnItemPickup(Item item, BasePlayer player)
        //{
        //    ServerPlayerItemPickup i = new ServerPlayerItemPickup
        //    {
        //        ItemId = item.info.itemid,
        //        ItemUid = unchecked((int)item.uid),
        //        SteamId = player.IPlayer.Id,
        //        PickupTimestamp = System.DateTime.Now.ToString(),
        //        Amount = item.amount
        //    };
        //    GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
        //    {
        //        DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsItemsItemIdPickup(i, DefaultPluginInformation.GetServerId(), player.IPlayer.Id, "" + item.info.itemid);
        //    });
        //    return null;
        //}

        //object OnLootItem(PlayerLoot playerLoot, Item item)
        //{
        //    BasePlayer player = playerLoot.GetComponent<BasePlayer>();
        //    if (player == null)
        //    {
        //        Puts("On item loot player null");
        //        return null;
        //    }
        //    ServerPlayerItemLoot i = new ServerPlayerItemLoot
        //    {
        //        ItemId = item.info.itemid,
        //        ItemUid = unchecked((int)item.uid),
        //        SteamId = player.IPlayer.Id,
        //        LootTimestamp = System.DateTime.Now.ToString(),
        //        Amount = item.amount
        //    };
        //    GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
        //    {
        //        DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsItemsItemIdLoot(i, DefaultPluginInformation.GetServerId(), player.IPlayer.Id, "" + item.info.itemid);
        //    });
        //    return null;
        //}

        //void OnItemCraftFinished(ItemCraftTask task, Item item)
        //{
        //    BasePlayer player = task.owner;
        //    ServerPlayerItemCrafted i = new ServerPlayerItemCrafted
        //    {
        //        ItemId = item.info.itemid,
        //        ItemUid = unchecked((int)item.uid),
        //        SteamId = player.IPlayer.Id,
        //        CraftTimestamp = System.DateTime.Now.ToString(),
        //        Amount = item.amount
        //    };
        //    GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
        //    {
        //        DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsItemsItemIdCrafted(i, DefaultPluginInformation.GetServerId(), player.IPlayer.Id, "" + item.info.itemid);
        //    });
        //}

        //void OnPlayerBanned(string name, ulong id, string address, string reason)
        //{
        //    BasePlayer player = BasePlayer.Find(id + "");
        //    if (player == null)
        //    {
        //        Puts("On player banned not found");
        //        return;
        //    }
        //    TimeSpan banTimeRemaining = player.IPlayer.BanTimeRemaining;
        //    ServerPlayerBanned i = new ServerPlayerBanned
        //    {
        //        SteamId = player.IPlayer.Id,
        //        Timestamp = System.DateTime.Now.ToString(),
        //        Duration = banTimeRemaining.Duration().Seconds + "",
        //        Reason = reason
        //    };
        //    GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
        //    {
        //        DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsBanned(i, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
        //    });
        //}

        //void OnPlayerReported(BasePlayer reporter, string targetName, string targetId, string subject, string message, string type)
        //{
        //    ServerPlayerReported i = new ServerPlayerReported
        //    {
        //        Timestamp = System.DateTime.Now.ToString(),
        //        ReportedTargetSteamId= targetId, 
        //        ReporterSteamId = reporter.IPlayer.Id,
        //        Message = message,
        //        ReportedType = type,
        //        Subject = subject,
        //    };
        //    GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
        //    {
        //        DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsReported(i, DefaultPluginInformation.GetServerId(), reporter.IPlayer.Id);
        //    });
        //}

        //// object OnServerCommand(ConsoleSystem.Arg arg)
        //// {
        ////     if(arg.Args == null)
        ////     {
        ////         Puts("On server command args null");
        ////         return null;
        ////     }
        ////     ServerCommand i = new ServerCommand
        ////     {
        ////         Timestamp = System.DateTime.Now.ToString(),
        ////         Command = arg.cmd.Name,
        ////         Arguments = string.Join(" ", arg.Args)
        ////     };
        ////     GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
        ////     {
        ////         DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdEventsCommand(i, DefaultPluginInformation.GetServerId());
        ////     });
        ////     return null;
        //// }

        ///**
        //* When players are damaged
        //**/
        //object OnPlayerAttack(BasePlayer player, HitInfo info)
        //{
        //   if (info != null && player != null && info.InitiatorPlayer != null && info.InitiatorPlayer.IPlayer.Id != player.IPlayer.Id && !(player is NPCPlayer) && !(info.InitiatorPlayer is NPCPlayer))
        //    {
        //        // Need to actually retrieve detailed information on next server tick, because HitInfo will not have been scaled according to hitboxes, protection, etc until then:
        //        NextTick(() =>
        //       {
        //           var attacker = info.InitiatorPlayer;
        //           int boneArea = (int)info.boneArea;
        //           ServerPlayerCombatPlayerhit pophWrapper = new ServerPlayerCombatPlayerhit();
        //           pophWrapper.HitTimestamp = System.DateTime.Now.ToString();
        //           PlayerHit poph = new PlayerHit();
        //           //poph.HitAreaId = boneArea;
        //           //poph.HitDistance = info.ProjectileDistance;
        //           //poph.HitDamage = info.damageTypes.Total();
        //           //poph.IsKill = player.IsDead();

        //           //Attacker attackerSwagger = new Attacker();
        //           poph.Position = new PlayerPosition
        //           {
        //               X = attacker.transform.position.x,
        //               Y = attacker.transform.position.y,
        //               Z = attacker.transform.position.z
        //           };
        //           //attackerSwagger.SteamId = attacker.IPlayer.Id;
        //           //attackerSwagger.Position = attackerPos;
        //           ActiveItem attackerItem = new ActiveItem
        //           {
        //               Uid = unchecked((int)(attacker.GetActiveItem() == null ? 0 : attacker.GetActiveItem().uid)),
        //               ItemId = attacker.GetActiveItem() == null ? 0 : attacker.GetActiveItem().info.itemid
        //           };
        //           poph.ActiveItem = attackerItem;
        //           //attackerSwagger.ActiveItem = attackerItem;
        //           //poph.Attacker = attackerSwagger;
        //           //Victim victimSwagger = new Victim();
        //           //PlayerPosition victimPos = new PlayerPosition
        //           //{
        //           //    X = player.transform.position.x,
        //           //    Y = player.transform.position.y,
        //           //    Z = player.transform.position.z
        //           //};
        //           //victimSwagger.SteamId = player.IPlayer.Id;
        //           //victimSwagger.Position = victimPos;
        //           //VictimActiveItem victimItem = new VictimActiveItem
        //           //{
        //           //    Uid = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().uid)),
        //           //    ItemId = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().info.itemid))
        //           //};
        //           //victimSwagger.ActiveItem = victimItem;
        //           //poph.Victim = victimSwagger;
        //           //pophWrapper.PlayerHit = poph;
        //           poph.SteamId = attacker.IPlayer.Id;
        //           GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.IMPORTANT, (System.Action success, System.Action failed) =>
        //           {
        //               DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsCombatHit(pophWrapper, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
        //           });
        //       });
        //    } else
        //    {
        //        Puts("Attack skipped");
        //    }

        //    return null;
        //}
        #endregion
    }
}
