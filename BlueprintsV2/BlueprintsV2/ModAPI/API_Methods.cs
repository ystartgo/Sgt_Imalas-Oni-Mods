﻿using Blueprints;
using BlueprintsV2.BlueprintsV2.BlueprintData;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UtilLibs;
using static BlueprintsV2.BlueprintsV2.BlueprintData.SensorTransferHelper;

namespace BlueprintsV2.BlueprintsV2.ModAPI
{

    /// <summary>
    /// The following type is an example on how to easily add additional data to blueprints for your modded buildings:
    /// if you implement two static Methods "Blueprints_SetData" and "Blueprints_GetData" with the same parameters as shown here, the blueprints will automatically store and apply them
    /// </summary>
    class IntegrationExample : KMonoBehaviour
    {

        internal void SetData(string key1, string key2)
        {
            this.key1 = key1;
            this.key2 = key2;
        }
        string key1 = "", key2 = "";

        /// <summary>
        /// This static method will allow you to apply any additional data stored previously in Blueprints_GetData.
        /// </summary>
        /// <param name="source">The Gameobject of the newly constructed building</param>
        /// <param name="data">the additional data stored in the object that was generated by Blueprints_GetData</param>
        public static void Blueprints_SetData(GameObject source, JObject data)
        {
            if (source.TryGetComponent<IntegrationExample>(out var behavior))
            {
                var t1 = data.GetValue("Key1");
                if (t1 == null)
                    return;
                var Key1 = t1.Value<string>(); //"Value1"

                var t2 = data.GetValue("Key2");
                if (t2 == null)
                    return;
                var Key2 = t1.Value<string>(); //"Value2"
                behavior.SetData(Key1, Key2);

            }
        }
        /// <summary>
        /// This static method takes in the source gameobject and returns a JObject with all the additional data you give it. this data is then stored in the blueprint
        /// </summary>
        /// <param name="source">the gameobject the blueprint is made from</param>
        /// <returns>JObject of all the data to be stored in the blueprint</returns>
        public static JObject Blueprints_GetData(GameObject source)
        {
            if (source.TryGetComponent<IntegrationExample>(out var behavior))
            {
                return new JObject()
                {
                    { "Key1", behavior.key1},
                    { "Key2", behavior.key2},
                };
            }
            return null;
        }
    }


    /// <summary>
    /// Api methods for modifying the blueprint behavior and to add additional data
    /// </summary>

    internal class API_Methods
    {
        /// <summary>
        /// if atleast one of these is true, the building is considered constructable
        /// </summary>
        public static Dictionary<string, System.Func<BuildingDef, bool>> OneOf_IsBuildableExtensions = new();

        /// <summary>
        /// only if all of these are true, the building is constructable
        /// </summary>
        public static Dictionary<string, System.Func<BuildingDef, bool>> AllReq_IsBuildableExtensions = new();

        /// <summary>
        /// Register 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="AlwaysAllowIfTrue"></param>
        /// <param name="Check"> function that returns a boolean value and takes in the buildingDef</param>
        public static void RegisterAdditionalBuildableCheck(string ID, bool AlwaysAllowIfTrue, Func<BuildingDef, bool> Check)
        {
            if (AlwaysAllowIfTrue)
            {
                OneOf_IsBuildableExtensions[ID] = Check;
            }
            else
            {
                AllReq_IsBuildableExtensions[ID] = Check;
            }
        }

        public static bool IsBuildable(BuildingDef buildingDef)
        {
            if (!BlueprintsAssets.Options.RequireConstructable || SaveGame.Instance.sandboxEnabled)
            {
                return true;//todo: register as oneOf
            }
            if (buildingDef.BuildingComplete.HasTag("DecorPackA_StainedGlass"))
            {
                return true;//todo: either me or aki; register as oneOf
            }

            foreach (var check in OneOf_IsBuildableExtensions)
            {
                if (check.Value(buildingDef))
                    return true;
            }

            foreach (var check in AllReq_IsBuildableExtensions)
            {
                if (!check.Value(buildingDef))
                    return false;
            }

            return PlanScreen.Instance.IsDefBuildable(buildingDef);
        }

        public class BuildingDataStorage
        {
            //unique data storage ID
            public string Id;
            //returns (if applicable to the given Gameobject)
            //called when storing the data, takes in GameObject and returns the data as a string
            public GetBlueprintDataDelegate GetDataToStore;
            //called when applying the data, takes in GameObject and the data value as a string
            public SetBlueprintDataDelegate ApplyStoredData;
            //override priority of this data storage rule
            public int OverridePriority = 0;

            public BuildingDataStorage(string iD, GetBlueprintDataDelegate onStore, SetBlueprintDataDelegate onApply, int overridePriority = 0)
            {
                Id = iD;
                this.GetDataToStore = onStore;
                this.ApplyStoredData = onApply;
                OverridePriority = overridePriority;
            }
        }

        public static Dictionary<string, BuildingDataStorage> AdditionalBuildingDataEntries = new();

        /// <summary>
        /// Register methods to store additional data inside the blueprints
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="IsApplicable">returns a bool if the given gameobject is a viable candidate for the additional data transfer</param>
        /// <param name="OnStore">recieves the current building game object and should return the data to store as a string</param>
        /// <param name="OnApply">takes in the current building game object and the data stored in the blueprint</param>
        /// <param name="OverridePriority">priority to override data storage for other mods</param>
        public static void RegisterAdditionalStorableBuildingData(string ID, GetBlueprintDataDelegate GetDataToStore, SetBlueprintDataDelegate ApplyStoredData, int OverridePriority = 0)
        {
            if (AdditionalBuildingDataEntries.TryGetValue(ID, out var value))
            {
                if (OverridePriority <= value.OverridePriority)
                {
                    SgtLogger.warning($"Registering additional blueprint data storage with the ID {ID}, but there was already an entry with that ID!. Override priority {OverridePriority} was too low to override");
                    return;
                }
                SgtLogger.l($"Registering additional blueprint data storage with the ID {ID}, but there was already an entry with that ID. Overriding with higher priority");
            }
            else
            {
                SgtLogger.l($"Registering additional blueprint data storage with the ID {ID}.");
            }
            AdditionalBuildingDataEntries[ID] = new BuildingDataStorage(ID, GetDataToStore, ApplyStoredData);
        }


        /// <summary>
        /// Stores any registered building data values in the blueprint building
        /// </summary>
        /// <param name="gameObject">the gameobject of the building</param>
        /// <param name="buildingConfig">the blueprint data where the additional data entries are added via key-value system</param>
        public static void StoreAdditionalBuildingData(GameObject gameObject, BuildingConfig buildingConfig)
        {
            foreach (var kvp in AdditionalBuildingDataEntries)
            {
                var DataHandler = kvp.Value;

                var data = DataHandler.GetDataToStore(gameObject);
                if (data != null)
                    buildingConfig.AddBuildingData(kvp.Key, data);

            }
        }

        /// <summary>
        /// applies any additional data stored in the blueprint to the newly placed blueprint building plan (or finished building in sandbox)
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="buildingConfig"></param>
        public static void ApplyAdditionalBuildingData(GameObject gameObject, BuildingConfig buildingConfig)
        {
            bool isUnderConstruction = (gameObject.TryGetComponent<UnderConstructionDataTransfer>(out var transfer));

            foreach (var kvp in AdditionalBuildingDataEntries)
            {
                var DataHandler = kvp.Value;

                if (buildingConfig.TryGetDataValue(kvp.Key, out var data))
                {
                    if(data == null)
                    {
                        SgtLogger.l("data was null for " + kvp.Key); return;
                    }

                    if (isUnderConstruction)
                    {
                        //storing on component to be applied on finished construction
                        transfer.SetDataToApply(kvp.Key, data);
                    }
                    else
                    {
                        //directly apply data to finished gameObject
                        DataHandler.ApplyStoredData(gameObject, data);
                    }
                }
            }
        }
        public static void TryApplyingStoredData(GameObject gameObject, string Key, JObject data)
        {
            if (AdditionalBuildingDataEntries.TryGetValue(Key, out var Methods) && data !=null)
            {
                Methods.ApplyStoredData(gameObject, data);
            }
        }


        public delegate JObject GetBlueprintDataDelegate(GameObject go);
        public delegate void SetBlueprintDataDelegate(GameObject go, JObject data);


        static void RegisterInternally(string ID, System.Func<GameObject, JObject> GetDataToStore, System.Action<GameObject, JObject> ApplyStoredData, int OverridePriority = 0)
        {
            RegisterAdditionalStorableBuildingData(ID, (GetBlueprintDataDelegate)Delegate.CreateDelegate(typeof(GetBlueprintDataDelegate), GetDataToStore.Method), (SetBlueprintDataDelegate)Delegate.CreateDelegate(typeof(SetBlueprintDataDelegate), ApplyStoredData.Method), OverridePriority);
        }
        private static void RegisterLogicSensors()
        {
            RegisterInternally(nameof(Filterable), DataTransfer_Filterable.TryGetData, DataTransfer_Filterable.TryApplyData);
            RegisterInternally(nameof(TreeFilterable), DataTransfer_TreeFilterable.TryGetData, DataTransfer_TreeFilterable.TryApplyData);
            RegisterInternally(nameof(LogicCritterCountSensor), DataTransfer_LogicCritterCountSensor.TryGetData, DataTransfer_LogicCritterCountSensor.TryApplyData);
            RegisterInternally(nameof(LogicTimeOfDaySensor), DataTransfer_LogicTimeOfDaySensor.TryGetData, DataTransfer_LogicTimeOfDaySensor.TryApplyData);
            RegisterInternally(nameof(LogicTimerSensor), DataTransfer_LogicTimerSensor.TryGetData, DataTransfer_LogicTimerSensor.TryApplyData);
            RegisterInternally(nameof(LogicClusterLocationSensor), DataTransfer_LogicClusterLocationSensor.TryGetData, DataTransfer_LogicClusterLocationSensor.TryApplyData);


            RegisterInternally(nameof(ConduitThresholdSensor), DataTransfer_GenericThresholdSensor<ConduitThresholdSensor>.TryGetData, DataTransfer_GenericThresholdSensor<ConduitThresholdSensor>.TryApplyData);
            RegisterInternally(nameof(LogicDiseaseSensor), DataTransfer_GenericThresholdSensor<LogicDiseaseSensor>.TryGetData, DataTransfer_GenericThresholdSensor<LogicDiseaseSensor>.TryApplyData);
            RegisterInternally(nameof(LogicWattageSensor), DataTransfer_GenericThresholdSensor<LogicWattageSensor>.TryGetData, DataTransfer_GenericThresholdSensor<LogicWattageSensor>.TryApplyData);
            RegisterInternally(nameof(LogicHEPSensor), DataTransfer_GenericThresholdSensor<LogicHEPSensor>.TryGetData, DataTransfer_GenericThresholdSensor<LogicHEPSensor>.TryApplyData);
            RegisterInternally(nameof(LogicLightSensor), DataTransfer_GenericThresholdSensor<LogicLightSensor>.TryGetData, DataTransfer_GenericThresholdSensor<LogicLightSensor>.TryApplyData);
            RegisterInternally(nameof(LogicMassSensor), DataTransfer_GenericThresholdSensor<LogicMassSensor>.TryGetData, DataTransfer_GenericThresholdSensor<LogicMassSensor>.TryApplyData);
            RegisterInternally(nameof(LogicPressureSensor), DataTransfer_GenericThresholdSensor<LogicPressureSensor>.TryGetData, DataTransfer_GenericThresholdSensor<LogicPressureSensor>.TryApplyData);
            RegisterInternally(nameof(LogicRadiationSensor), DataTransfer_GenericThresholdSensor<LogicRadiationSensor>.TryGetData, DataTransfer_GenericThresholdSensor<LogicRadiationSensor>.TryApplyData);
            RegisterInternally(nameof(LogicTemperatureSensor), DataTransfer_GenericThresholdSensor<LogicTemperatureSensor>.TryGetData, DataTransfer_GenericThresholdSensor<LogicTemperatureSensor>.TryApplyData);

            RegisterInternally(nameof(LogicGateBuffer), DataTransfer_GenericLogicGateDelay<LogicGateBuffer>.TryGetData, DataTransfer_GenericLogicGateDelay<LogicGateBuffer>.TryApplyData);
            RegisterInternally(nameof(LogicGateFilter), DataTransfer_GenericLogicGateDelay<LogicGateFilter>.TryGetData, DataTransfer_GenericLogicGateDelay<LogicGateFilter>.TryApplyData);

            RegisterInternally(nameof(LogicRibbonReader), DataTransfer_GenericRibbonData<LogicRibbonReader>.TryGetData, DataTransfer_GenericRibbonData<LogicRibbonReader>.TryApplyData);
            RegisterInternally(nameof(LogicRibbonWriter), DataTransfer_GenericRibbonData<LogicRibbonWriter>.TryGetData, DataTransfer_GenericRibbonData<LogicRibbonWriter>.TryApplyData);


            RegisterInternally(nameof(HighEnergyParticleSpawner), DataTransfer_HighEnergyParticleSpawner.TryGetData, DataTransfer_HighEnergyParticleSpawner.TryApplyData);
            RegisterInternally(nameof(HighEnergyParticleRedirector), DataTransfer_HighEnergyParticleRedirector.TryGetData, DataTransfer_HighEnergyParticleRedirector.TryApplyData);
            RegisterInternally(nameof(HEPBattery), DataTransfer_HEPBattery.TryGetData, DataTransfer_HEPBattery.TryApplyData);

        }

        internal static void RegisterExtraData()
        {
            RegisterInternally(API_Consts.BuildingSkinID, SkinHelper.TryStoreBuildingSkin, SkinHelper.TryApplyBuildingSkin);
            RegisterInternally("Backwalls_Backwall", SkinHelper.TryStoreBackwall, SkinHelper.TryApplyBackwall);
            RegisterLogicSensors();

            var q = AppDomain.CurrentDomain.GetAssemblies()
                   .SelectMany(t => t.GetTypes());

            foreach (var type in q)
            {
                ///This method should return a JObject that contains all data the component on the given gameobject transfers to the blueprint, see the example at the top
                var DataGetter = AccessTools.Method(type, "Blueprints_GetData",
                new[]
                {
                    typeof(GameObject)
                });

                ///This method recieves the target gameobject and the JObject data it stored with the method above. it should apply the data from that JObject to the given gameobject, see the example at the top
                var DataApplier = AccessTools.Method(type, "Blueprints_SetData",
                new[]
                {
                    typeof(GameObject)
                    , typeof(JObject)
                });

                if (DataGetter != null && DataApplier != null)
                {
                    SgtLogger.l("trying to register additional blueprint data for type " + type.AssemblyQualifiedName);
                    var getterDelegate = (GetBlueprintDataDelegate)Delegate.CreateDelegate(typeof(GetBlueprintDataDelegate), DataGetter);
                    var setterDelegate = (SetBlueprintDataDelegate)Delegate.CreateDelegate(typeof(SetBlueprintDataDelegate), DataApplier);
                    if (getterDelegate != null && setterDelegate != null)
                        RegisterAdditionalStorableBuildingData(type.AssemblyQualifiedName, getterDelegate, setterDelegate);
                    else
                        SgtLogger.warning("failed to create delegates for " + type.AssemblyQualifiedName);
                }
            }

        }

    }
}