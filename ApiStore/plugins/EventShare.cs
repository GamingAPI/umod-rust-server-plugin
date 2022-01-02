using Oxide.Ext.GamingEvent;MessageQueue
using Oxide.Ext.GamingEvent.MessageQueue;
using RequestForServerPlayerCombatPlayerhitNameSpace;
using RequestForServerPlayerConnectedNameSpace;
using RequestForServerPlayerDisconnectedNameSpace;
using RequestForServerPlayerItemPickupNameSpace;
using RequestForServerPlayerResourceGatheredNameSpace;
using RequestForServerPlayerRespawnedNameSpace;

namespace Oxide.Plugins
{
    [Info("EventShare", "jonaslagoni", "0.0.2")]
    [Description("Share all the in-game events with the GamingEventAPI network")]
    class Processor : RustPlugin
    {
        private void Init()
        {
            Puts("Starting plugin");
            GamingEventMessageQueue.Instance.AddToMessageQueue(MessageImportance.NORMAL, (System.Action success, System.Action failed) =>
            {
                var resp = DefaultPluginInformation.GetInstance().NatsClient.RequestRustApiprocessServersServerIdEventsStarted(DefaultPluginInformation.GetServerId());
                if (resp.StatusCode != 200)
                {
                    Puts("OnStarted : Returned " + resp.StatusCode + " message : " + resp.StatusMessage);
                    failed();
                }
                else
                {
                    success();
                }
            });
        }
        
        protected override void LoadDefaultConfig()
        {
            Config["ShouldBroadcastNotifications"] = true;
        }


        #region Hooks

        #region Resource gathering
        private void ResourceGathered(BasePlayer player, Item item)
        {
            GamingEventMessageQueue.Instance.AddToMessageQueue(MessageImportance.NORMAL, (System.Action success, System.Action failed) =>
            {
                var resp = DefaultPluginInformation.GetInstance().NatsClient.RequestRustApiprocessServersServerIdPlayersSteamIdEventsResourcesGathered(ConvertToResouceRequestMessage(item, player), DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
                if (resp.StatusCode != 200)
                {
                    Puts("OnDispenserBonus : Returned " + resp.StatusCode + " message : " + resp.StatusMessage);
                    failed();
                }
                else
                {
                    success();
                }
            });
        }

        private RequestForServerPlayerResourceGathered ConvertToResouceRequestMessage(Item item, BasePlayer player)
        {
            RequestForServerPlayerResourceGathered re = new RequestForServerPlayerResourceGathered();
            re.Amount = item.amount;
            re.GatheredTimestamp = "" + System.DateTime.Now.ToString();
            GatheringItem gatheringItem = new GatheringItem
            {
                Uid = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().uid)),
                ItemId = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().info.itemid))
            };
            GatheringPosition gatheringPosition = new GatheringPosition
            {
                X = player.transform.position.x,
                Y = player.transform.position.y,
                Z = player.transform.position.z
            };
            re.GatheringItem = gatheringItem;
            re.GatheringPosition = gatheringPosition;
            re.ItemId = item.info.itemid;
            re.ItemUid = unchecked((int)item.uid);
            re.SteamId = player.IPlayer.Id;
            return re;
        }

        //Called before the player is given a bonus item for gathering
        object OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            ResourceGathered(player, item);
            return null;
        }

        //Called when the player collects an item
        object OnCollectiblePickup(Item item, BasePlayer player)
        {
            ResourceGathered(player, item);
            return null;
        }
        //Called before the player is given items from a resource
        object OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            ResourceGathered(entity.ToPlayer(), item);
            return null;
        }
        #endregion
        
        /**
         * Player respawn 
         **/
        void OnPlayerRespawned(BasePlayer player)
        {
            GamingEventMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOW, (System.Action success, System.Action failed) =>
            {
                RequestForServerPlayerRespawned playerRespawn = new RequestForServerPlayerRespawned
                {
                    SteamId = player.IPlayer.Id,
                    RespawnTimestamp = System.DateTime.Now.ToString(),
                    RespawnPosition = new RespawnPosition
                    {
                        X = player.transform.position.x,
                        Y = player.transform.position.y,
                        Z = player.transform.position.z
                    }
                };

                var resp = DefaultPluginInformation.GetInstance().NatsClient.RequestRustApiprocessServersServerIdPlayersSteamIdEventsRespawned(playerRespawn, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
                if (resp.StatusCode != 200)
                {
                    Puts("OnPlayerRespawned : Returned " + resp.StatusCode + " message : " + resp.StatusMessage);
                    failed();
                }
                else
                {
                    success();
                }
            });
        }

        /**
         * On player disconnected
         **/
        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            RequestForServerPlayerDisconnected p = new RequestForServerPlayerDisconnected();
            p.DisconnectedTimestamp = System.DateTime.Now.ToString();
            RequestForServerPlayerDisconnectedNameSpace.Player dp = new RequestForServerPlayerDisconnectedNameSpace.Player();
            dp.Id = player.IPlayer.Id;
            p.Player = dp;
            p.Reason = reason;
            //Must be the lowest importance to ensure all other events are are processed first
            GamingEventMessageQueue.Instance.AddToMessageQueue(MessageImportance.LOWEST, (System.Action success, System.Action failed) =>
            {

                var resp = DefaultPluginInformation.GetInstance().NatsClient.RequestRustApiprocessServersServerIdPlayersSteamIdEventsDisconnected(p, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
                if (resp.StatusCode != 200)
                {
                    Puts("OnPlayerDisconnected : Returned " + resp.StatusCode + " message : " + resp.StatusMessage);
                    failed();
                }
                else
                {
                    success();
                }
            });
        }

        /**
         * On player connected
         **/
        void OnPlayerInit(BasePlayer player)
        {
            RequestForServerPlayerConnected p = new RequestForServerPlayerConnected();
            p.ConnectedTimestamp = System.DateTime.Now.ToString();
            RequestForServerPlayerConnectedNameSpace.Player p2 = new RequestForServerPlayerConnectedNameSpace.Player
            {
                Id = player.IPlayer.Id,
                Name = player.IPlayer.Name,
                Address = player.IPlayer.Address
            };
            p.Player = p2;

            GamingEventMessageQueue.Instance.AddToMessageQueue(MessageImportance.IMPORTANT, (System.Action success, System.Action failed) =>
            {
                var resp = DefaultPluginInformation.GetInstance().NatsClient.RequestRustApiprocessServersServerIdPlayersSteamIdEventsConnected(p, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
                if (resp.StatusCode != 200)
                {
                    Puts("OnPlayerInit : Returned " + resp.StatusCode + " message : " + resp.StatusMessage);
                    failed();
                }
                else
                {
                    success();
                }
            });
        }

        /**
         * On server wipe
         **/
        void OnNewSave(string filename)
        {
            GamingEventMessageQueue.Instance.AddToMessageQueue(MessageImportance.STRICT, (System.Action success, System.Action failed) =>
            {
                var resp = DefaultPluginInformation.GetInstance().NatsClient.RequestRustApiprocessServersServerIdEventsWiped(DefaultPluginInformation.GetServerId());
                if (resp.StatusCode != 200)
                {
                    Puts("OnNewSave : Returned " + resp.StatusCode + " message : " + resp.StatusMessage);
                    failed();
                }
                else
                {
                    success();
                }
            });
        }

        /**
         * Weapon pickup notifications
         **/
        void OnWeaponPickup(BasePlayer player, Item item)
        {
            RequestForServerPlayerItemPickup i = new RequestForServerPlayerItemPickup
            {
                ItemId = item.info.itemid,
                ItemUid = unchecked((int)item.uid),
                SteamId = player.IPlayer.Id,
                PickupTimestamp = System.DateTime.Now.ToString(),
                Amount = 1
            };
            GamingEventMessageQueue.Instance.AddToMessageQueue(MessageImportance.IMPORTANT, (System.Action success, System.Action failed) =>
            {
                var resp = DefaultPluginInformation.GetInstance().NatsClient.RequestRustApiprocessServersServerIdPlayersSteamIdEventsItemsItemIdPickup(i, DefaultPluginInformation.GetServerId(), player.IPlayer.Id, "" + item.info.itemid);
                if (resp.StatusCode != 200)
                {
                    Puts("OnWeaponPickup : Returned " + resp.StatusCode + " message : " + resp.StatusMessage);
                    failed();
                }
                else
                {
                    success();
                }
            });
        }


        /**
         * On player damage
         **/
        void OnPlayerDamage(BasePlayer player, HitInfo info)
        {
            if (info == null) return;
            if (player == null || info.InitiatorPlayer == null || info.InitiatorPlayer.IPlayer.Id == player.IPlayer.Id || player is NPCPlayer || info.InitiatorPlayer is NPCPlayer) return;

            // Need to actually retrieve detailed information on next server tick, because HitInfo will not have been scaled according to hitboxes, protection, etc until then:
            NextTick(() =>
            {
                var attacker = info.InitiatorPlayer;
                int boneArea = (int)info.boneArea;
                RequestForServerPlayerCombatPlayerhit pophWrapper = new RequestForServerPlayerCombatPlayerhit();
                pophWrapper.HitTimestamp = System.DateTime.Now.ToString();
                PlayerHit poph = new PlayerHit();
                poph.HitAreaId = boneArea;
                poph.HitDistance = info.ProjectileDistance;
                poph.HitDamage = info.damageTypes.Total();
                poph.IsKill = player.IsDead();

                Attacker attackerSwagger = new Attacker();
                AttackerPosition attackerPos = new AttackerPosition
                {
                    X = attacker.transform.position.x,
                    Y = attacker.transform.position.y,
                    Z = attacker.transform.position.z
                };
                attackerSwagger.SteamId = attacker.IPlayer.Id;
                attackerSwagger.Position = attackerPos;
                AttackerActiveItem attackerItem = new AttackerActiveItem
                {
                    Uid = unchecked((int)(attacker.GetActiveItem() == null ? 0 : attacker.GetActiveItem().uid)),
                    ItemId = attacker.GetActiveItem() == null ? 0 : attacker.GetActiveItem().info.itemid
                };
                attackerSwagger.ActiveItem = attackerItem;
                poph.Attacker = attackerSwagger;
                Victim victimSwagger = new Victim();
                VictimPosition victimPos = new VictimPosition
                {
                    X = player.transform.position.x,
                    Y = player.transform.position.y,
                    Z = player.transform.position.z
                };
                victimSwagger.SteamId = player.IPlayer.Id;
                victimSwagger.Position = victimPos;
                VictimActiveItem victimItem = new VictimActiveItem
                {
                    Uid = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().uid)),
                    ItemId = unchecked((int)(player.GetActiveItem() == null ? 0 : player.GetActiveItem().info.itemid))
                };
                victimSwagger.ActiveItem = victimItem;
                poph.Victim = victimSwagger;
                pophWrapper.PlayerHit = poph;
                GamingEventMessageQueue.Instance.AddToMessageQueue(MessageImportance.IMPORTANT, (System.Action success, System.Action failed) =>
                {
                    var resp = DefaultPluginInformation.GetInstance().NatsClient.RequestRustApiprocessServersServerIdPlayersSteamIdEventsCombatPlayerhit(pophWrapper, DefaultPluginInformation.GetServerId(), player.IPlayer.Id);
                    if (resp.StatusCode != 200)
                    {
                        Puts("OnPlayerDamage : Returned " + resp.StatusCode + " message : " + resp.StatusMessage);
                        failed();
                    }
                    else
                    {
                        success();
                    }
                });
            });
        }
        #endregion

    }

}
