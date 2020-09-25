﻿using ProperSave.Data;
using RoR2;
using RoR2.Stats;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProperSave.SaveData
{
    public class PlayerData {
        [DataMember(Name = "si")]
        public ulong steamId;

        [DataMember(Name = "m")]
        public uint money;

        [DataMember(Name = "sf")]
        public string[] statsFields;
        [DataMember(Name = "su")]
        public int[] statsUnlockables;

        [DataMember(Name = "ms")]
        public MinionData[] minions;

        [DataMember(Name = "i")]
        public InventoryData inventory;

        [DataMember(Name = "cbn")]
        public string characterBodyName;

        [DataMember(Name = "l")]
        public LoadoutData loadout;

        [DataMember(Name = "lccm")]
        public float lunarCoinChanceMultiplier;

        [DataMember(Name = "lc")]
        public uint lunarCoins;

        internal PlayerData(NetworkUser player) {
            var master = player.master;
            steamId = player.Network_id.steamId.value;

            money = player.master.money;
            inventory = new InventoryData(master.inventory);
            loadout = new LoadoutData(master.loadout);
            
            characterBodyName = player.master.bodyPrefab.name;
            lunarCoinChanceMultiplier = player.masterController.lunarCoinChanceMultiplier;
            lunarCoins = player.lunarCoins;

            var tmpMinions = new List<MinionData>();
            foreach (var instance in CharacterMaster.readOnlyInstancesList)
            {
                var ownerMaster = instance.minionOwnership.ownerMaster;
                if (ownerMaster != null && ownerMaster.netId == player.master.netId)
                {
                    tmpMinions.Add(new MinionData(instance));
                }
            }
            minions = new MinionData[tmpMinions.Count];
            for (var i = 0; i < tmpMinions.Count; i++)
            {
                minions[i] = tmpMinions[i];
            }

            var stats = player.masterController.GetComponent<PlayerStatsComponent>().currentStats;
            statsFields = new string[stats.fields.Length];
            for (var i = 0; i < stats.fields.Length; i++)
            {
                var field = stats.fields[i];
                statsFields[i] = field.ToString();
            }
            statsUnlockables = new int[stats.GetUnlockableCount()];
            for (var i = 0; i < stats.GetUnlockableCount(); i++)
            {
                var unlockable = stats.GetUnlockableIndex(i);
                statsUnlockables[i] = unlockable.value;
            }
        }

        internal void LoadPlayer(NetworkUser player) {
            var master = player.master;
            foreach(var minion in minions)
            {
                minion.LoadMinion(master);
            }

            var bodyPrefab = BodyCatalog.FindBodyPrefab(characterBodyName);

            master.bodyPrefab = bodyPrefab;

            loadout.LoadData(master.loadout);

            inventory.LoadInventory(master.inventory);

            if (ModSupport.IsSSLoaded)
            {
                ProperSave.Instance.StartCoroutine(LoadShareSuiteMoney(money));
            }
            player.master.money = money;

            player.masterController.lunarCoinChanceMultiplier = lunarCoinChanceMultiplier;
            var stats = player.masterController.GetComponent<PlayerStatsComponent>().currentStats;
            for (var i = 0; i < statsFields.Length; i++)
            {
                var fieldValue = statsFields[i];
                stats.SetStatValueFromString(StatDef.allStatDefs[i], fieldValue);
            }
            for (var i = 0; i < statsUnlockables.Length; i++)
            {
                var unlockableIndex = statsUnlockables[i];
                stats.AddUnlockable(new UnlockableIndex(unlockableIndex));
            }

            if (ModSupport.IsTLCLoaded || ModSupport.IsBDTLCLoaded)
            {
                Stage.onStageStartGlobal += ResetLunarCoins;
                void ResetLunarCoins(Stage stage)
                {
                    Stage.onStageStartGlobal -= ResetLunarCoins;
                    player.DeductLunarCoins(player.lunarCoins);
                    player.AwardLunarCoins(lunarCoins);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private IEnumerator LoadShareSuiteMoney(uint money)
        {
            yield return new WaitUntil(() => !ShareSuite.MoneySharingHooks.MapTransitionActive);
            ShareSuite.MoneySharingHooks.SharedMoneyValue = (int)money;
        }
    }
}
