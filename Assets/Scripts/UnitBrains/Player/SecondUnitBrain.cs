using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Model;
using Model.Runtime.Projectiles;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities;

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
        private List<Vector2Int> unreachebleTargets;
        
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
            List<Vector2Int> targets = SelectTargets();

            Vector2Int unitPos = unit.Pos;
            Vector2Int nextTarget = new Vector2Int(0, 0);
            if (targets.Count > 0)
            { 
                foreach (var target in targets)
                {
                    if (IsTargetInRange(target))
                    {
                        return unit.Pos;
                    }
                    else
                    {
                        nextTarget = target;
                    }
                }
            }
            else
            {
                int playerId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[playerId];
                if (IsTargetInRange(enemyBase))
                {
                    return unit.Pos;
                }
                else
                {
                    unreachebleTargets.Add(enemyBase);
                    nextTarget = enemyBase;
                }
            }

            return unitPos.CalcNextStepTowards(nextTarget);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            Vector2Int minVector = new Vector2Int(0, 0);
            float min = float.MaxValue;

            unreachebleTargets = new();

            foreach (Vector2Int vector in GetAllTargets())
            {
                float distanseToOwnBase = DistanceToOwnBase(vector);
                if (min > distanseToOwnBase) 
                {
                    min = distanseToOwnBase;
                    minVector = vector;
                }
            }

            if (min != float.MaxValue)
            {
                unreachebleTargets.Add(minVector);
            }

            return unreachebleTargets;
            /***
            if (max != float.MaxValue)
            {
                if (IsTargetInRange(minVector))
                {
                    result.Add(minVector);
                    return result;
                }
                else
                {
                    unreachebleTargets.Add(minVector);
                    return unreachebleTargets;
                }

            }
            else
            {
                int playerId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[playerId];
                if (IsTargetInRange(enemyBase))
                {
                    result.Add(enemyBase);
                    return result;
                }
                else
                {
                    unreachebleTargets.Add(enemyBase);
                    return unreachebleTargets;
                }
            }
            ***/
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