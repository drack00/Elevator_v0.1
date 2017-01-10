using UnityEngine;
using System.Collections;

public class ActiveFrameData : FrameData
{
    [System.Serializable]
    public class Behaviour
    {
        public ApplyMovement force;
        public ApplyMovement torque;

        public ApplyStat damage;
        public ApplyStat stun;

        public Spawn[] spawns;

        public virtual void Do(bool continuous, FrameData hit, FrameData hurt = null, bool invert = false)
        {
            if (hurt == null)
            {
                hurt = hit;
            }

            FrameData _hit = !invert ? hit : hurt;
            FrameData _hurt = !invert ? hurt : hit;

            force.Do(continuous, _hit, _hurt);
            torque.Do(continuous, _hit, _hurt, SortVectors.VectorType.Torque);

            _hit.mo.health = damage.Do(_hit.mo.health);
            _hit.mo.stun = stun.Do(_hit.mo.stun);

            foreach (Spawn spawn in spawns)
            {
                spawn.Do(continuous, _hit, hurt);
            }
        }
    }
}
