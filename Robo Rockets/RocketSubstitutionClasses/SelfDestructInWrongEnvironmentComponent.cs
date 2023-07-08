﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KnastoronOniMods
{
    class SelfDestructInWrongEnvironmentComponent : KMonoBehaviour, ISim4000ms
    {
        int homeWorld;

        protected override void OnSpawn()
        {
            homeWorld = this.GetMyWorldId();
        }

        public void SelfDestruct()
        {
            homeWorld = -1;
        }

        public void Sim4000ms(float dt)
        {

            // Debug.Log(homeWorld + "<- ->" + this.GetMyWorldId());
            if (this.gameObject == null||this.GetMyWorld()==null|| this.GetMyWorldId() != homeWorld)
            {
                Destroy(this.gameObject);
            }

        }
    }
}
