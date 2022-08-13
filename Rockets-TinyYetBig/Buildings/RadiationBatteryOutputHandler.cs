﻿using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Rockets_TinyYetBig
{
    class RadiationBatteryOutputHandler : KMonoBehaviour, 
        IHighEnergyParticleDirection,
        ISim200ms,
        //IUserControlledCapacity, 
        ISingleSliderControl
    {
        [MyCmpReq]
        private KSelectable selectable;
        [Serialize]
        private EightDirection _direction;
        private EightDirectionController directionController;

        private static readonly EventSystem.IntraObjectHandler<RadiationBatteryOutputHandler> OnStorageChangedDelegate
            = new EventSystem.IntraObjectHandler<RadiationBatteryOutputHandler>((System.Action<RadiationBatteryOutputHandler, object>)((component, data) => component.OnStorageChange(data)));

        [MyCmpReq]
        public HighEnergyParticleStorage hepStorage;
        private MeterController m_meter;

        public int GetOutputCell()
        {
            var build = GetComponent<Building>();
            var cell = build.GetHighEnergyParticleOutputCell();
            return cell;
        }


        public bool AllowSpawnParticles => this.hasLogicWire && this.isLogicActive;
        private bool hasLogicWire;
        private bool isLogicActive;
        private float launchTimer = 0;
        private readonly float minLaunchInterval = 1f;
        public void Sim200ms(float dt)
        {
            launchTimer += dt;
            if ((double)launchTimer < (double)minLaunchInterval || !AllowSpawnParticles || (double)hepStorage.Particles < (double)particleThreshold)
                return;
            launchTimer = 0.0f;
            this.Fire();
        }

        private void OnLogicValueChanged(object data)
        {
            LogicValueChanged logicValueChanged = (LogicValueChanged)data;
            if (!(logicValueChanged.portID == HEPBattery.FIRE_PORT_ID))
                return;
            this.isLogicActive = logicValueChanged.newValue > 0;
            this.hasLogicWire = this.GetNetwork() != null;
        }
        private LogicCircuitNetwork GetNetwork() => Game.Instance.logicCircuitManager.GetNetworkForCell(this.GetComponent<LogicPorts>().GetPortCell(HEPBattery.FIRE_PORT_ID));

        public void UpdateOutputCell()
        {
            int x = 0, y = 0;
            if (Direction.ToString().Contains("Down"))
                y -= 1;
            else if (Direction.ToString().Contains("Up"))
                y += 1;
            if (Direction.ToString().Contains("Right"))
                x += 1;
            else if (Direction.ToString().Contains("Left"))
                x -= 1;
            var build = GetComponent<Building>();

            var offset = build.GetHighEnergyParticleInputOffset();
            offset.x += x;
            offset.y += y;
            build.Def.HighEnergyParticleOutputOffset = offset;
        }

        public void Fire()
        {
            int particleOutputCell = this.GetOutputCell();
            GameObject gameObject = GameUtil.KInstantiate(Assets.GetPrefab((Tag)"HighEnergyParticle"), Grid.CellToPosCCC(particleOutputCell, Grid.SceneLayer.FXFront2), Grid.SceneLayer.FXFront2);
            gameObject.SetActive(true);
            if (!((UnityEngine.Object)gameObject != (UnityEngine.Object)null))
                return;
            HighEnergyParticle component = gameObject.GetComponent<HighEnergyParticle>();
            component.payload = hepStorage.ConsumeAndGet(particleThreshold);
            component.SetDirection(Direction);
        }


        public LocString CapacityUnits => UI.UNITSUFFIXES.HIGHENERGYPARTICLES.PARTRICLES;

        public IStorage Storage => (IStorage)this.hepStorage;
        public EightDirection Direction
        {
            get => this._direction;
            set
            {
                this._direction = value;
                if (this.directionController == null)
                    return;
                UpdateOutputCell();
                this.directionController.SetRotation((float)(45 * EightDirectionUtil.GetDirectionIndex(this._direction)));
                this.directionController.controller.enabled = false;
                this.directionController.controller.enabled = true;
            }
        }

        #region CapacityChange

        public float physicalFuelCapacity;
        public float UserMaxCapacity
        {
            get => this.hepStorage.capacity;
            set
            {
                this.hepStorage.capacity = value;
                this.Trigger((int)GameHashes.ParticleStorageCapacityChanged, (object)this);
            }
        }
        public float MinCapacity => 0.0f;
        public float MaxCapacity => this.physicalFuelCapacity;

        public float AmountStored => this.hepStorage.Particles;

        public bool WholeValues => false;
        #endregion

        protected override void OnSpawn()
        {
            base.OnSpawn();
            //if (infoStatusItem_Logic == null)
            //{
            //    infoStatusItem_Logic = new StatusItem("HEPRedirectorLogic", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID);
            //    infoStatusItem_Logic.resolveStringCallback = new Func<string, object, string>(ResolveInfoStatusItem);
            //    infoStatusItem_Logic.resolveTooltipCallback = new Func<string, object, string>(ResolveInfoStatusItemTooltip);
            //}
            //this.selectable.AddStatusItem(infoStatusItem_Logic, (object)this);


            this.directionController = new EightDirectionController((KAnimControllerBase)this.GetComponent<KBatchedAnimController>(), "redirector_target", "redirector", EightDirectionController.Offset.Infront);
            this.Direction = this.Direction;

            this.m_meter = new MeterController((KAnimControllerBase)this.GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, Array.Empty<string>());
            this.m_meter.gameObject.GetComponent<KBatchedAnimTracker>().matchParentOffset = true;


            this.OnStorageChange((object)null);
            //this.Subscribe<RadiationBatteryOutputHandler>((int)GameHashes.ParticleStorageCapacityChanged, OnStorageChangedDelegate);
            this.Subscribe<RadiationBatteryOutputHandler>((int)GameHashes.OnParticleStorageChanged, OnStorageChangedDelegate);
            this.Subscribe((int)GameHashes.LogicEvent, new System.Action<object>(this.OnLogicValueChanged));

        }


        private void OnStorageChange(object data) => this.m_meter.SetPositionPercent(this.hepStorage.Particles / Mathf.Max(1f, this.hepStorage.capacity));
        protected override void OnCleanUp()
        {
            base.OnCleanUp();
        }

        //private bool OnParticleCaptureAllowed(HighEnergyParticle particle) => true;

        #region SidescreenSliderForCapacityThrowout

        [Serialize]
        public float particleThreshold = 50f;

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.RADBOLTTHRESHOLDSIDESCREEN.TITLE";

        public string SliderUnits => (string)UI.UNITSUFFIXES.HIGHENERGYPARTICLES.PARTRICLES;


        public int SliderDecimalPlaces(int index) => 0;

        public float GetSliderMin(int index) => (float)25;

        public float GetSliderMax(int index) => (float)250;

        public float GetSliderValue(int index) => this.particleThreshold;

        public void SetSliderValue(float value, int index) => this.particleThreshold = value;

        public string GetSliderTooltipKey(int index) => "STRINGS.UI.UISIDESCREENS.RADBOLTTHRESHOLDSIDESCREEN.TOOLTIP";

        string ISliderControl.GetSliderTooltip() => string.Format((string)Strings.Get("STRINGS.UI.UISIDESCREENS.RADBOLTTHRESHOLDSIDESCREEN.TOOLTIP"), (object)this.particleThreshold);

        #endregion
    }
}