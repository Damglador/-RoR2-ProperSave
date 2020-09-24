﻿using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ProperSave.Data.Artifacts
{
    public class EnigmaData
    {
        [DataMember(Name = "sier")]
        public RngData serverInitialEquipmentRng;
        [DataMember(Name = "saer")]
        public RngData serverActivationEquipmentRng;

        public EnigmaData()
        {
            serverInitialEquipmentRng = new RngData(EnigmaArtifactManager.serverInitialEquipmentRng);
            serverActivationEquipmentRng = new RngData(EnigmaArtifactManager.serverActivationEquipmentRng);
        }

        public void LoadData()
        {
            serverInitialEquipmentRng.LoadDataRef(ref EnigmaArtifactManager.serverInitialEquipmentRng);
            serverActivationEquipmentRng.LoadDataRef(ref EnigmaArtifactManager.serverActivationEquipmentRng);
        }
    }
}
