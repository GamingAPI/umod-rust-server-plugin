using Asyncapi.Nats.Client.Models;
using ConVar;
using Newtonsoft.Json;
using Oxide.Ext.GamingApi;
using Oxide.Ext.GamingApi.MessageQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using VLB;


//ExtraLoot created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("GamingAPI", "jonaslagoni", "0.0.2")]
    [Description("Share all the in-game events with the GamingAPI network")]
    class GamingAPI : RustPlugin
    {
        private void Init()
        {
            Puts("Starting plugin");
            Unsubscribe(nameof(OnPlayerAttack));
            Unsubscribe(nameof(OnPlayerChat));
            Unsubscribe(nameof(OnDispenserGather));
            Unsubscribe(nameof(OnDispenserBonus));
            Unsubscribe(nameof(OnGrowableGather));
            Unsubscribe(nameof(OnCollectiblePickup));
            Unsubscribe(nameof(OnPlayerRespawned));
            Unsubscribe(nameof(OnPlayerDisconnected));
            Unsubscribe(nameof(OnPlayerConnected));
            Unsubscribe(nameof(OnNewSave));
            Unsubscribe(nameof(OnItemCraftFinished));
            Unsubscribe(nameof(OnPlayerBanned));
            Unsubscribe(nameof(OnPlayerReported));
            Unsubscribe(nameof(OnServerCommand));
            Unsubscribe(nameof(OnItemPickup));
            Unsubscribe(nameof(OnLootEntity));
            Unsubscribe(nameof(OnLootEntityEnd));
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

        public class Configuration
        {
            [JsonProperty(PropertyName = "If true, when a player writes in the chat the event will be saved")]
            public bool onChat = false;
            [JsonProperty(PropertyName = "If true, when a player farms a resource the event will be saved")]
            public bool onResourceGathered = false;
            [JsonProperty(PropertyName = "If true, when a player respawns the event will be saved")]
            public bool onRespawn = false;
            [JsonProperty(PropertyName = "If true, when a player disconnects the event will be saved")]
            public bool onPlayerDisconnected = false;
            [JsonProperty(PropertyName = "If true, when a player connects the event will be saved")]
            public bool onPlayerConnected = false;
            [JsonProperty(PropertyName = "If true, when the server wipes the event will be saved")]
            public bool onWipe = false;
            [JsonProperty(PropertyName = "If true, when a player crafts an item the event will be saved")]
            public bool onCrafted = false;
            [JsonProperty(PropertyName = "If true, when a player is banned the event will be saved")]
            public bool onBanned = false;
            [JsonProperty(PropertyName = "If true, when a player is reported the event will be saved")]
            public bool onReportedd = false;
            [JsonProperty(PropertyName = "If true, when a server command is run the event will be saved")]
            public bool onServerCommand = false;
            [JsonProperty(PropertyName = "If true, when a player hits another player the event will be saved")]
            public bool onPlayerHit = false;
            [JsonProperty(PropertyName = "If true, when a player picks up an item the event will be saved")]
            public bool onItemPickup = false;
            [JsonProperty(PropertyName = "If true, when a player loots an item the event will be saved")]
            public bool onitemLooted = false;

            public static Configuration DefaultConfig()
            {
                return new Configuration
                {
                };
            }
        }

        private static Configuration _config;
        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null) LoadDefaultConfig();
                SaveConfig();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                PrintWarning("Creating new config file.");
                LoadDefaultConfig();
            }
        }

        private void OnServerInitialized()
        {
            if (_config.onChat)
            {
                Subscribe(nameof(OnPlayerChat));
            }
            if (_config.onResourceGathered)
            {
                Subscribe(nameof(OnDispenserGather));
                Subscribe(nameof(OnDispenserBonus));
                Subscribe(nameof(OnGrowableGather));
                Subscribe(nameof(OnCollectiblePickup));
            }
            if (_config.onRespawn)
            {
                Subscribe(nameof(OnPlayerRespawned));
            }
            if (_config.onPlayerConnected)
            {
                Subscribe(nameof(OnPlayerConnected));
            }
            if (_config.onPlayerDisconnected)
            {
                Subscribe(nameof(OnPlayerDisconnected));
            }
            if (_config.onWipe)
            {
                Subscribe(nameof(OnNewSave));
            }
            if (_config.onCrafted)
            {
                Subscribe(nameof(OnItemCraftFinished));
            }
            if (_config.onBanned)
            {
                Subscribe(nameof(OnPlayerBanned));
            }
            if (_config.onReportedd)
            {
                Subscribe(nameof(OnPlayerReported));
            }
            if (_config.onServerCommand)
            {
                Subscribe(nameof(OnServerCommand));
            }
            if (_config.onPlayerHit)
            {
                Subscribe(nameof(OnPlayerAttack));
            }
            if (_config.onItemPickup)
            {
                Subscribe(nameof(OnItemPickup));
            }
            if (_config.onitemLooted)
            {
                Subscribe(nameof(OnLootEntity));
                Subscribe(nameof(OnLootEntityEnd));
            }
        }

        protected override void LoadDefaultConfig() => _config = Configuration.DefaultConfig();
        protected override void SaveConfig() => Config.WriteObject(_config);

        #region Utils
        private void ResourceGathered(ServerPlayerResourceGathered req, string playerId)
        {
            GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
            {
                DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsGatheredResources(req, DefaultPluginInformation.GetServerId(), playerId);
            });
        }
        private void ResourceGathered(BasePlayer player, Item item)
        {
            ServerPlayerResourceGathered req = ConvertToResouceRequestMessage(item, player);
            this.ResourceGathered(req, player.IPlayer.Id);
        }
        private void ResourceGathered(BasePlayer player, ItemAmount item)
        {
            ServerPlayerResourceGathered req = ConvertToResouceRequestMessage(item, player);
            this.ResourceGathered(req, player.IPlayer.Id);
        }

        private ServerPlayerResourceGathered ConvertToResouceRequestMessage(ItemAmount itemAmount, BasePlayer player)
        {
            Item item = player.inventory.FindItemID(itemAmount.itemid);
            ServerPlayerResourceGathered re = new ServerPlayerResourceGathered();
            try
            {
                re.GatheredTimestamp = "" + System.DateTime.Now.ToString();
                re.Amount = (int)itemAmount.amount;
                if (item != null)
                {
                    re.ItemId = item.info.itemid;
                    re.ItemUid = unchecked((int)item.uid);
                }
                if (player != null)
                {
                    ActiveItem gatheringItem = new ActiveItem
                    {
                        Uid = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().uid)),
                        ItemId = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().info.itemid))
                    };
                    PlayerPosition gatheringPosition = new PlayerPosition
                    {
                        X = player.transform.position.x,
                        Y = player.transform.position.y,
                        Z = player.transform.position.z
                    };
                    re.GatheringItem = gatheringItem;
                    re.GatheringPosition = gatheringPosition;
                    re.SteamId = player.IPlayer.Id;
                }
            }
            catch (Exception e)
            {
                Puts("Failed sending message");
                Puts(e.ToString());
            }
            return re;
        }

        private ServerPlayerResourceGathered ConvertToResouceRequestMessage(Item item, BasePlayer player)
        {
            ServerPlayerResourceGathered re = new ServerPlayerResourceGathered();
            re.GatheredTimestamp = "" + System.DateTime.Now.ToString();
            try
            {
                if (item != null)
                {
                    re.Amount = item.amount;
                    re.ItemId = item.info.itemid;
                    re.ItemUid = unchecked((int)item.uid);
                }
                if (player != null)
                {
                    ActiveItem gatheringItem = new ActiveItem
                    {
                        Uid = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().uid)),
                        ItemId = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().info.itemid))
                    };
                    PlayerPosition gatheringPosition = new PlayerPosition
                    {
                        X = player.transform.position.x,
                        Y = player.transform.position.y,
                        Z = player.transform.position.z
                    };
                    re.GatheringItem = gatheringItem;
                    re.GatheringPosition = gatheringPosition;
                    re.SteamId = player.IPlayer.Id;
                }
            }
            catch (Exception e)
            {
                Puts("Failed sending message");
                Puts(e.ToString());
            }
            return re;
        }
        #endregion

        #region Hooks
        object OnPlayerChat(BasePlayer player, string message, Chat.ChatChannel channel)
        {
            GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
            {
                ChatMessage chatMessage = new ChatMessage
                {
                    SteamId = player.UserIDString,
                    Timestamp = System.DateTime.Now.ToString(),
                    FullMessage = message,
                    IsAdmin = player.IsAdmin,
                    PlayerName = player.displayName
                };

                DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdEventsPlayerSteamIdChatted(chatMessage, DefaultPluginInformation.GetServerId(), player.UserIDString);
            });
            return null;
        }
        object OnCollectiblePickup(CollectibleEntity collectible, BasePlayer player)
        {
            Puts("OnCollectiblePickup works!");
            foreach (ItemAmount itemAmount in collectible.itemList)
            {
                ResourceGathered(player, itemAmount);
            }
           return null;
        }
        object OnDispenserGather(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            Puts("OnDispenserGather works!");
            ServerPlayerResourceGathered re = ConvertToResouceRequestMessage(item, player);
            ResourceGathered(re, player.IPlayer.Id);
            return null;
        }
        void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            Puts("OnDispenserBonus works!");
            ServerPlayerResourceGathered re = ConvertToResouceRequestMessage(item, player);
            ResourceGathered(re, player.IPlayer.Id);
        }
        object OnGrowableGather(GrowableEntity plant, BasePlayer player)
        {
            Puts("OnGrowableGather works!");
            ResourceGathered(player, plant.GetItem());
            return null;
        }
        object OnPlayerRespawned(BasePlayer player)
        {
           GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
           {
               ServerPlayerRespawned playerRespawn = new ServerPlayerRespawned
               {
                   SteamId = player.IPlayer.Id,
                   RespawnTimestamp = System.DateTime.Now.ToString(),
                   RespawnPosition = new PlayerPosition
                   {
                       X = player.transform.position.x,
                       Y = player.transform.position.y,
                       Z = player.transform.position.z
                   }
               };

               DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsRespawned(playerRespawn, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
           });
            return null;
        }
        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
           ServerPlayerDisconnected p = new ServerPlayerDisconnected();
           p.DisconnectedTimestamp = System.DateTime.Now.ToString();
           ServerPlayerDisconnectedPlayer dp = new ServerPlayerDisconnectedPlayer();
           dp.Id = player.IPlayer.Id;
           p.Player = dp;
           p.Reason = reason;
           //Must be the lowest importance to ensure all other events are are processed first
           GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOWEST, (System.Action success, System.Action failed) =>
           {
               DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsDisconnected(p, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
           });
        }
        void OnPlayerConnected(BasePlayer player)
        {
           ServerPlayerConnected p = new ServerPlayerConnected();
           p.ConnectedTimestamp = System.DateTime.Now.ToString();
           Asyncapi.Nats.Client.Models.Player p2 = new Asyncapi.Nats.Client.Models.Player
           {
               Id = player.IPlayer.Id,
               Name = player.IPlayer.Name,
               Address = player.IPlayer.Address
           };
           p.Player = p2;

           GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.IMPORTANT, (System.Action success, System.Action failed) =>
           {
               DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsConnected(p, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
           });
        }
        void OnNewSave(string filename)
        {
           GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.STRICT, (System.Action success, System.Action failed) =>
           {
               DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdEventsWiped(DefaultPluginInformation.GetServerId());
           });
        }
        void OnItemCraftFinished(ItemCraftTask task, Item item)
        {
            Puts("OnItemCraftFinished works!");
            BasePlayer player = task.owner;
            ServerPlayerItemCrafted i = new ServerPlayerItemCrafted
            {
                ItemId = item.info.itemid,
                ItemUid = unchecked((int)item.uid),
                SteamId = player.IPlayer.Id,
                CraftTimestamp = System.DateTime.Now.ToString(),
                Amount = item.amount
            };
            GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
            {
                DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsItemsItemIdCrafted(i, DefaultPluginInformation.GetServerId(), player.IPlayer.Id, "" + item.info.itemid);
            });
        }
        void OnPlayerBanned(string name, ulong id, string address, string reason)
        {
            BasePlayer player = BasePlayer.Find(id + "");
            if (player == null)
            {
                Puts("On player banned not found");
                return;
            }
            TimeSpan banTimeRemaining = player.IPlayer.BanTimeRemaining;
            ServerPlayerBanned i = new ServerPlayerBanned
            {
                SteamId = player.IPlayer.Id,
                Timestamp = System.DateTime.Now.ToString(),
                Duration = banTimeRemaining.Duration().Seconds + "",
                Reason = reason
            };
            GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
            {
                DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsBanned(i, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
            });
        }
        void OnPlayerReported(BasePlayer reporter, string targetName, string targetId, string subject, string message, string type)
        {
            ServerPlayerReported i = new ServerPlayerReported
            {
                Timestamp = System.DateTime.Now.ToString(),
                ReportedTargetSteamId= targetId, 
                ReporterSteamId = reporter.IPlayer.Id,
                Message = message,
                ReportedType = type,
                Subject = subject,
            };
            GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
            {
                DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsReported(i, DefaultPluginInformation.GetServerId(), reporter.IPlayer.Id);
            });
        }
        object OnServerCommand(ConsoleSystem.Arg arg)
        {
            if(arg.Args == null)
            {
                Puts("On server command args null");
                return null;
            }
            ServerCommand i = new ServerCommand
            {
                Timestamp = System.DateTime.Now.ToString(),
                Command = arg.cmd.Name,
                Arguments = string.Join(" ", arg.Args)
            };
            GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
            {
                DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdEventsCommand(i, DefaultPluginInformation.GetServerId());
            });
            return null;
        }
        object OnPlayerAttack(BasePlayer player, HitInfo info)
        {
           if (info != null && player != null && info.InitiatorPlayer != null && info.InitiatorPlayer.IPlayer.Id != player.IPlayer.Id && !(player is NPCPlayer) && !(info.InitiatorPlayer is NPCPlayer))
            {
                // Need to actually retrieve detailed information on next server tick, because HitInfo will not have been scaled according to hitboxes, protection, etc until then:
                NextTick(() =>
               {
                   var attacker = info.InitiatorPlayer;
                   int boneArea = (int)info.boneArea;
                   ServerPlayerCombatPlayerhit pophWrapper = new ServerPlayerCombatPlayerhit();
                   pophWrapper.HitTimestamp = System.DateTime.Now.ToString();
                   PlayerHit poph = new PlayerHit();
                   //poph.HitAreaId = boneArea;
                   //poph.HitDistance = info.ProjectileDistance;
                   //poph.HitDamage = info.damageTypes.Total();
                   //poph.IsKill = player.IsDead();

                   //Attacker attackerSwagger = new Attacker();
                   poph.Position = new PlayerPosition
                   {
                       X = attacker.transform.position.x,
                       Y = attacker.transform.position.y,
                       Z = attacker.transform.position.z
                   };
                   //attackerSwagger.SteamId = attacker.IPlayer.Id;
                   //attackerSwagger.Position = attackerPos;
                   ActiveItem attackerItem = new ActiveItem
                   {
                       Uid = unchecked((int)(attacker.GetActiveItem() == null ? 0 : attacker.GetActiveItem().uid)),
                       ItemId = attacker.GetActiveItem() == null ? 0 : attacker.GetActiveItem().info.itemid
                   };
                   poph.ActiveItem = attackerItem;
                   //attackerSwagger.ActiveItem = attackerItem;
                   //poph.Attacker = attackerSwagger;
                   //Victim victimSwagger = new Victim();
                   //PlayerPosition victimPos = new PlayerPosition
                   //{
                   //    X = player.transform.position.x,
                   //    Y = player.transform.position.y,
                   //    Z = player.transform.position.z
                   //};
                   //victimSwagger.SteamId = player.IPlayer.Id;
                   //victimSwagger.Position = victimPos;
                   //VictimActiveItem victimItem = new VictimActiveItem
                   //{
                   //    Uid = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().uid)),
                   //    ItemId = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().info.itemid))
                   //};
                   //victimSwagger.ActiveItem = victimItem;
                   //poph.Victim = victimSwagger;
                   //pophWrapper.PlayerHit = poph;
                   poph.SteamId = attacker.IPlayer.Id;
                   GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.IMPORTANT, (System.Action success, System.Action failed) =>
                   {
                       DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsCombatHit(pophWrapper, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
                   });
               });
            } else
            {
                Puts("Attack skipped");
            }

            return null;
        }
        object OnItemPickup(Item item, BasePlayer player)
        {
            ServerPlayerItemPickup i = new ServerPlayerItemPickup
            {
                ItemId = item.info.itemid,
                ItemUid = unchecked((int)item.uid),
                SteamId = player.IPlayer.Id,
                PickupTimestamp = System.DateTime.Now.ToString(),
                Amount = item.amount
            };
            GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
            {
                DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsItemsItemIdPickup(i, DefaultPluginInformation.GetServerId(), player.IPlayer.Id, "" + item.info.itemid);
            });
            return null;
        }

        private Dictionary<uint, List<LocalItem>> containerItems = new Dictionary<uint, List<LocalItem>>();

        private class LocalItem : IEquatable<LocalItem>
        {
            public int itemid;
            public int amount;
            public int itemUid;

            public override bool Equals(object obj)
            {
                var item = obj as LocalItem;
                if (item == null)
                    return false;

                return itemid == item.itemid &&
                       amount == item.amount &&
                       itemUid == item.itemUid;
            }

            public bool Equals(LocalItem other)
            {
                return this.Equals(other as object);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return "ItemId:" + itemid;
            }
        }
        private void OnLootEntity(BasePlayer looter, BaseEntity entity)
        {
            StorageContainer container = entity as StorageContainer;
            DroppedItemContainer droppedContainer = entity as DroppedItemContainer;
            LootableCorpse lootableCorpse = entity as LootableCorpse;
            Puts("OnLootEntity works!" + (entity.GetType()));
            if (container == null && droppedContainer == null && lootableCorpse == null)
            {
                return;
            }
            List<LocalItem> items = new List<LocalItem>();
            List<Item> inventoryList = new List<Item>();
            uint containerId = 0;
            if(container != null)
            {
                inventoryList = container.inventory.itemList;
                containerId = container.net.ID;
            } else if (droppedContainer != null)
            {
                inventoryList = droppedContainer.inventory.itemList;
                containerId = droppedContainer.net.ID;
            } else if (lootableCorpse != null)
            {
                foreach (ItemContainer lootableContainer in lootableCorpse.containers)
                {
                    inventoryList.AddRange(lootableContainer.itemList);
                }
                containerId = lootableCorpse.net.ID;
            }
            foreach (Item item in inventoryList)
            {
                items.Add(new LocalItem { amount = item.amount, itemid = item.info.itemid, itemUid = (int)item.uid });
            }
            containerItems[containerId] = items;
        }

        private void OnLootEntityEnd(BasePlayer looter, BaseEntity entity)
        {
            StorageContainer container = entity as StorageContainer;
            DroppedItemContainer droppedContainer = entity as DroppedItemContainer;
            LootableCorpse lootableCorpse = entity as LootableCorpse;
            Puts("OnLootEntityEnd works!" + (entity.GetType()));
            if (container == null && droppedContainer == null && lootableCorpse == null)
            {
                return;
            }
            List<Item> inventoryList = new List<Item>();
            uint containerId = 0;
            if (container != null)
            {
                inventoryList = container.inventory.itemList;
                containerId = container.net.ID;
            }
            else if (droppedContainer != null)
            {
                inventoryList = droppedContainer.inventory.itemList;
                containerId = droppedContainer.net.ID;
            }
            else if (lootableCorpse != null)
            {
                foreach (ItemContainer lootableContainer in lootableCorpse.containers)
                {
                    inventoryList.AddRange(lootableContainer.itemList);
                }
                containerId = lootableCorpse.net.ID;
            }

            List<LocalItem> initialItems;
            if (!containerItems.TryGetValue(containerId, out initialItems))
            {
                return;
            }

            foreach (LocalItem initialItem in initialItems)
            {
                bool stillInContainer = true;
                int amountTaken = initialItem.amount;
                if(inventoryList.Count != 0)
                {
                    foreach (Item containerItem in inventoryList)
                    {
                        stillInContainer = containerItem.info.itemid == initialItem.itemid;
                        if (stillInContainer)
                        {
                            amountTaken = initialItem.amount - containerItem.amount;
                            break;
                        }
                    }
                } else
                {
                    stillInContainer = false;
                }
                if (!stillInContainer || amountTaken != 0)
                {
                    ServerPlayerItemLoot i = new ServerPlayerItemLoot
                    {
                        ItemId = initialItem.itemid,
                        ItemUid = unchecked((int)initialItem.itemUid),
                        SteamId = looter.IPlayer.Id,
                        LootTimestamp = System.DateTime.Now.ToString(),
                        ContainerPosition = new PlayerPosition
                        {
                            X = entity.transform.position.x,
                            Y = entity.transform.position.y,
                            Z = entity.transform.position.z
                        },
                        ContainerUid = (int)containerId,
                        ContainerPrefab = entity.ShortPrefabName,
                        Amount = amountTaken
                    };
                    GamingApiMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
                    {
                        DefaultPluginInformation.GetInstance().NatsClient.JetStreamPublishToV0RustServersServerIdPlayersSteamIdEventsItemsItemIdLoot(i, DefaultPluginInformation.GetServerId(), looter.IPlayer.Id, "" + initialItem.itemid);
                    });
                }
            }

            // Cleanup
            containerItems.Remove(containerId);
        }
    }
    #endregion
}
