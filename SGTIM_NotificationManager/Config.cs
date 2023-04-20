﻿using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGTIM_NotificationManager
{
    [Serializable]
    [RestartRequired]
    [ConfigFile(SharedConfigLocation: true)]
    [ModInfo("Warning Notification Manager")]
    public class Config : SingletonOptions<Config>
    {
        public Config()
        {
            MUTE_STARVATION_SOUND = false;
            STARVATION_THRESHOLD = 1000;

            MUTE_SUFFOCATION_SOUND = false;
            SUFFOCATION_THRESHOLD = 50;
        }

        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP","STRINGS.NOTIFICATION_CONFIG.STARVATION.CATEGORY")]
        [JsonProperty]
        public bool MUTE_STARVATION_SOUND { get; set; }

        [Option("STRINGS.NOTIFICATION_CONFIG.STARVATION.THRESHOLD", "STRINGS.NOTIFICATION_CONFIG.STARVATION.THRESHOLD_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.STARVATION.CATEGORY")]
        [JsonProperty]
        [Limit(100, 2000)]
        public int STARVATION_THRESHOLD { get; set; }

        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.STARVATION.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_STARVATION { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.STARVATION.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_STARVATION { get; set; }



        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.SUFFOCATION.CATEGORY")]
        [JsonProperty]
        public bool MUTE_SUFFOCATION_SOUND { get; set; }

        [Option("STRINGS.NOTIFICATION_CONFIG.SUFFOCATION.THRESHOLD", "STRINGS.NOTIFICATION_CONFIG.SUFFOCATION.THRESHOLD_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.SUFFOCATION.CATEGORY")]
        [JsonProperty]
        [Limit(15, 100)]
        public int SUFFOCATION_THRESHOLD { get; set; }

        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.SUFFOCATION.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_SUFFOCATION { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.SUFFOCATION.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_SUFFOCATION { get; set; }


        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.ATTACK.CATEGORY")]
        [JsonProperty]
        public bool MUTE_ATTACK_SOUND { get; set; }

        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.ATTACK.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_ATTACK { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.ATTACK.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_ATTACK { get; set; }


        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.PEE.CATEGORY")]
        [JsonProperty]
        public bool MUTE_PEE_SOUND { get; set; }

        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.PEE.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_PEE { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.PEE.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_PEE { get; set; }



        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.FLEE.CATEGORY")]
        [JsonProperty]
        public bool MUTE_FLEE_SOUND { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.FLEE.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_FLEE { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.FLEE.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_FLEE { get; set; }



        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.STRESS.CATEGORY")]
        [JsonProperty]
        public bool MUTE_STRESS_SOUND { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.STRESS.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_STRESS { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.STRESS.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_STRESS { get; set; }


        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.SCALDING.CATEGORY")]
        [JsonProperty]
        public bool MUTE_SCALDING_SOUND { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.SCALDING.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_SCALDING { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.SCALDING.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_SCALDING { get; set; }

        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.ENTOMBED.CATEGORY")]
        [JsonProperty]
        public bool MUTE_ENTOMBED_SOUND { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.ENTOMBED.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_ENTOMBED { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.ENTOMBED.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_ENTOMBED { get; set; }

        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.INCAPACITATED.CATEGORY")]
        [JsonProperty]
        public bool MUTE_INCAPACITATED_SOUND { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.INCAPACITATED.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_INCAPACITATED { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.INCAPACITATED.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_INCAPACITATED { get; set; }


        [Option("STRINGS.NOTIFICATION_CONFIG.MUTE_PING", "STRINGS.NOTIFICATION_CONFIG.MUTE_PING_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.PLANTDEATH.CATEGORY")]
        [JsonProperty]
        public bool MUTE_PLANTDEATH_SOUND { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.PAUSE_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.PLANTDEATH.CATEGORY")]
        [JsonProperty]
        public bool PAUSE_ON_PLANTDEATH { get; set; }
        [Option("STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION", "STRINGS.NOTIFICATION_CONFIG.ZOOM_ON_NOTIFICATION_TOOLTIP", "STRINGS.NOTIFICATION_CONFIG.PLANTDEATH.CATEGORY")]
        [JsonProperty]
        public bool PAN_TO_PLANTDEATH { get; set; }
    }
}
