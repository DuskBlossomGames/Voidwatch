using System.Collections;
using System.Collections.Generic;
using Spawnables.Damage;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawnables.Controllers.Worms
{
    public class WormBeamAttack : MonoBehaviour
    {
        public float chargeTime;
        public float dodgeTime;
        public float ramp;
        public float baseDmg;
        public float shieldMult, bleedPerc;

        public Material material;
        public Gradient warnColor;
        public Gradient beamColor;
        public Gradient littlebeamColor;

        private bool _isShooting;
        private float _atkTimer;

        private List<GameObject> _spawns = new();
        
        public void Shoot()
        {
            if (!_isShooting) {
                _isShooting = true;
                StartCoroutine(_Shoot());
            };
        }

        IEnumerator _Shoot()
        {
            GetComponent<WormSegment>().aroundPather.snakeyness /= 3;
            GetComponent<WormSegment>().pathmode = WormSegment.PathMode.Direct;
            yield return new WaitForSeconds(chargeTime);

            GetComponent<WormSegment>().speed /= 4;
            Vector2 dir = GetComponent<WormSegment>().PredDir(dodgeTime+0.1f);

            GameObject ray = new GameObject("Warning Beam");
            _spawns.Add(ray);
            ray.transform.position-= Vector3.forward * 2;
            var lrend = ray.AddComponent<LineRenderer>();
            lrend.colorGradient = warnColor;
            lrend.material = material;
            lrend.widthMultiplier = .17f;
            lrend.SetPositions(new[] { transform.position + 1 * (Vector3)dir, transform.position + 200 * (Vector3)dir });


            yield return new WaitForSeconds(dodgeTime);
            Destroy(ray);
            _spawns.Remove(ray);

            GetComponent<WormSegment>().speed *= 4;
            GetComponent<WormSegment>().speed = Mathf.Max(5, GetComponent<WormSegment>().speed);


            List <GameObject> beams =  new List<GameObject>();

            float dmg = baseDmg;

            for (int i = 1; i < transform.parent.childCount-1; i++)
            {
                dmg *= (1 + transform.parent.GetChild(i).GetComponent<WormSegment>().bend)/2 * ramp;
            }

            for (int i = 0; i < 5; i++)
            {
                GameObject lilray = new GameObject("Mini-Beam");
                _spawns.Add(lilray);
                lilray.transform.position -= Vector3.forward * 2;
                beams.Add(lilray);
                lrend = lilray.AddComponent<LineRenderer>();
                lrend.colorGradient = littlebeamColor;
                lrend.material = material;
                lrend.widthMultiplier = .04f;
                Vector3[] pos = new[] { transform.position + 1 * (Vector3)dir + .1f * (Vector3)Random.insideUnitCircle, transform.position + 200 * (Vector3)dir + 8 * (Vector3)Random.insideUnitCircle };
                lrend.SetPositions(pos);
                RaycastHit2D hit = Physics2D.Linecast(pos[0], pos[1], 1 << LayerMask.NameToLayer("Player"));
                if (hit.collider != null)
                {
                    var tar = hit.transform.gameObject;
                    var dmgable = tar.GetComponent<IDamageable>();
                    if (dmgable != null)
                    {
                        dmgable.Damage(.05f * dmg, gameObject, shieldMult, bleedPerc);
                    }
                }
                yield return new WaitForSeconds(.02f);
            }

            ray = new GameObject("Beam");
            _spawns.Add(ray);
            ray.transform.position -= Vector3.forward * 2;
            lrend = ray.AddComponent<LineRenderer>();
            lrend.colorGradient = beamColor;
            lrend.material = material;
            Vector3[] rpos = new[] { transform.position + 1 * (Vector3)dir + .1f * (Vector3)Random.insideUnitCircle, transform.position + 200 * (Vector3)dir + 3 * (Vector3)Random.insideUnitCircle };
            lrend.SetPositions(rpos);
            lrend.widthMultiplier = .6f;
            RaycastHit2D rhit = Physics2D.Linecast(rpos[0], rpos[1], 1 << LayerMask.NameToLayer("Player"));
            if (rhit.collider != null)
            {
                var tar = rhit.transform.gameObject;
                var dmgable = tar.GetComponent<Damageable>();
                if (dmgable != null)
                {
                    dmgable.Damage(dmg, gameObject, shieldMult, bleedPerc);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                GameObject lilray = new GameObject("Mini-Beam");
                _spawns.Add(lilray);
                lilray.transform.position -= Vector3.forward * 2;
                beams.Add(lilray);
                lrend = lilray.AddComponent<LineRenderer>();
                lrend.colorGradient = littlebeamColor;
                lrend.material = material;
                lrend.widthMultiplier = .04f;
                Vector3[] pos = new[] { transform.position + 1 * (Vector3)dir + .1f * (Vector3)Random.insideUnitCircle, transform.position + 200 * (Vector3)dir + 4 * (Vector3)Random.insideUnitCircle };
                lrend.SetPositions(pos);
                RaycastHit2D hit = Physics2D.Linecast(pos[0], pos[1], 1 << LayerMask.NameToLayer("Player"));
                if (hit.collider != null)
                {
                    var tar = hit.transform.gameObject;
                    var dmgable = tar.GetComponent<Damageable>();
                    if (dmgable != null)
                    {
                        dmgable.Damage(.05f * dmg, gameObject, shieldMult, bleedPerc);
                    }
                }
            }

            GetComponent<WormSegment>().pathmode = WormSegment.PathMode.Around;
            GetComponent<WormSegment>().aroundPather.snakeyness *= 3;

            yield return new WaitForSeconds(.1f);
            Destroy(ray);
            _spawns.Remove(ray);
            foreach (var lilray in beams)
            {
                Destroy(lilray);
                _spawns.Remove(lilray);
            }

            _isShooting = false;
        }

        private void OnDestroy() { _spawns.ForEach(Destroy); }
    }
}
