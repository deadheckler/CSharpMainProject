﻿using System.Collections.Generic;
using System.Numerics;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();
            
            for (int numberOfShells = 0; numberOfShells < currentTemperature + 1; numberOfShells++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            Vector2Int minVector = new Vector2Int(0, 0);
            float max = float.MaxValue;

            List<Vector2Int> result = GetReachableTargets();
 
            foreach (Vector2Int vector in result)
            {
                float distanseToOwnBase = DistanceToOwnBase(vector);
                if (max > distanseToOwnBase) 
                {
                    max = distanseToOwnBase;
                    minVector = vector;
                }
            }

            if (max != float.MaxValue) 
            {
                result.Clear();
                result.Add(minVector);
            }
            
            while (result.Count > 1)
            {
                result.RemoveAt(result.Count - 1);
            }

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}