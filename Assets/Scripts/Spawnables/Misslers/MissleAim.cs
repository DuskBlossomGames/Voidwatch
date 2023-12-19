using System;
using System.Collections;
using System.Collections.Generic;
using Spawnables;
using UnityEngine;
using Util;
using Complex = System.Numerics.Complex;

public class MissleAim : MonoBehaviour
{
    public float accelforce;
    public GameObject target;
    //public GameObject marker;
    public float maxFuel;
    public GameObject particles;
    public GameObject explosion;
    public float stallTime;
    public float dmg;

    private Vector2 _oVel;
    private Vector2 _nDir;
    private CustomRigidbody2D _rigid;

    private bool _stopped = false;
    private Quaternion _cRot;
    private float _fuel;
    private float _tickTimer;
    private float _timeToImpact;

    private void Start()
    {
        _fuel = maxFuel;
        _rigid = GetComponent<CustomRigidbody2D>();
        StartCoroutine(LeadShots());
    }

    IEnumerator LeadShots()
    {
        while (true)
        {
            Vector3 relPos = target.transform.position - transform.position;
            //Debug.LogFormat("target.name: {0}",target.name);
            Vector3 relVel = target.GetComponent<CustomRigidbody2D>().velocity - _rigid.velocity;
            float accel = (accelforce) / _rigid.mass;
            _nDir = LeadShot(relPos, relVel, accel);
            yield return new WaitForSeconds(.1f);
        }
    }

    private void FixedUpdate()
    {
        if (_fuel>0)
        {
            _fuel -= accelforce * Time.fixedDeltaTime;
            _rigid.AddForce(accelforce * _nDir);
            Vector2 realAccel = -(_oVel - _rigid.velocity) / Time.fixedDeltaTime;
            _oVel = _rigid.velocity;
            Quaternion tRot = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * Mathf.Atan2(_nDir.y, _nDir.x));
            _cRot = Quaternion.RotateTowards(_cRot, tRot, 360 * Time.deltaTime);
            transform.rotation = _cRot;

            if (_timeToImpact < 2 && !GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play();
            } else if(_timeToImpact > 2 && GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Stop();
            }
            
            var psmain = particles.GetComponent<ParticleSystem>().main;
            psmain.startSpeed = 1 + 14 * (_fuel / maxFuel);

        } else {
            if (!_stopped)
            {
                GetComponent<AudioSource>().Stop();
                var psmain = particles.GetComponent<ParticleSystem>().main;
                psmain.loop = false;
                _stopped = true;
                StopCoroutine(LeadShots());
                Destroy(GetComponent<PositionHinter>());
            }
            stallTime -= Time.fixedDeltaTime;
            if (stallTime < 0)
            {
                Explode();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
    }

    public void Explode()
    {
        explosion.transform.parent = null;
        explosion.GetComponent<ExplosionHandler>().Run();
        explosion.GetComponent<ParticleSystem>().Play();
        int rayNum = 16;
        for (int i = 0; i < rayNum; i++)
        {
            Vector2 raydir = new Vector2(Mathf.Cos(2 * Mathf.PI * i / rayNum), Mathf.Sin(2 * Mathf.PI * i / rayNum));
            LayerMask mask = 1<<LayerMask.NameToLayer("Enemies") | 1<<LayerMask.NameToLayer("Player") | 1<<LayerMask.NameToLayer("Scene Objects");
            RaycastHit2D hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + 5 * raydir,mask);
            if (hit.collider!=null)
            {
                var tar = hit.transform.gameObject;
                //Debug.LogFormat("Ray {0} collided with {1}", i, tar.name);
                var dmgable = tar.GetComponent<IDamageable>();
                if (dmgable != null)
                {
                    dmgable.Damage(dmg / rayNum, IDamageable.DmgType.Physical);
                }
            }
            //Debug.LogFormat("Ray {0} end", i);
        }
        Destroy(gameObject);
    }

    Vector2 LeadShot(Vector2 relPos, Vector2 relVel, float vesselAccel)
    {
        List<float> coefs = new List<float>()
        {
            .25f * vesselAccel * vesselAccel, //x**4
            0,                                //x**3
            -relVel.sqrMagnitude,             //x**2
            -2 * Vector2.Dot(relVel, relPos), //x
            -relPos.sqrMagnitude              //
        };
        float colTime = MinPosRoot(coefs);

        Vector2 colPos = relPos + colTime * relVel;
        //marker.transform.position = (Vector3)((Vector2)target.transform.position + colTime * target.GetComponent<CustomRigidbody2D>().velocity);
        //marker.name = string.Format("+{0};{1}", colTime.ToString(), Time.time + colTime);
        _timeToImpact = colTime;
        return colPos.normalized;

    }

    float _Bisect(List<float> coefs, float lowbound, float upbound)
    {
        float lval, rval;
        lval = _Poly(coefs, lowbound);
        rval = _Poly(coefs, lowbound);
        for (int i = 0; i < 15; i++)//magic number is precision limit
        {
            float mid = (lowbound + upbound) / 2;
            float mval = _Poly(coefs, mid);
            if (Mathf.Sign(mval * lval) == -1)
            {
                rval = mval;
                upbound = mid; // (l- m+ u+) -> (l- ? m+)
            }
            else
            {
                lval = mval;
                lowbound = mid; // (l- m- u+) -> (m- ? u+)
            }
        }
        float x = (lowbound * rval - upbound * lval) / (rval - lval);
        return x;
    }

    float _Bisect(List<float> coefs, float lowbound)
    {
        float upbound = 1 + lowbound * 2;
        float lval = _Poly(coefs, lowbound);
        float rval = _Poly(coefs, upbound);

        int iter = 0;
        while (Mathf.Sign(lval * rval) != -1)
        {
            iter += 1;
            upbound *= 2;
            rval = _Poly(coefs, upbound);
            if (iter > 30)
            {
                Debug.LogErrorFormat("Unbound Bisect didn't converge, please verify that a zero exists");
                return -1; 
            }
        }

        return _Bisect(coefs, lowbound, upbound);

    }

    float MinPosRoot(List<float> coefs)
    {
        return RecMinPosRoot(coefs)[0];
    }

    List<float> RecMinPosRoot(List<float> coefs, int rep = 0)
    {
        int degree = coefs.Count - 1;
        var solutions = new List<float>();

        if (degree == 2)
        {
            if (coefs[1] * coefs[1] - 4 * coefs[0] * coefs[2] >= 0) {
                solutions.Add((-coefs[1] + Mathf.Sqrt(coefs[1] * coefs[1] - 4 * coefs[0] * coefs[2])) / (2 * coefs[0]));
            }
            if (coefs[1] * coefs[1] - 4 * coefs[0] * coefs[2] > 0)
            {
                solutions.Add((-coefs[1] - Mathf.Sqrt(coefs[1] * coefs[1] - 4 * coefs[0] * coefs[2])) / (2 * coefs[0]));
            }
            solutions.Sort();
            return solutions;
        }

        if (coefs[coefs.Count-1] == 0) //_Poly(coefs, 0)==0
        {
            solutions.Add(0);
        }

        float zSign = Mathf.Sign(coefs[coefs.Count - 1]); //_Poly(coefs, 0)

        List<float> extrema = RecMinPosRoot(_Derivate(coefs),rep+1);
        /*for (int i = 0; i < extrema.Count; i++)
        {
            Debug.LogFormat("Extrema_{3} {0}: {1} @ {2}", i, extrema[i],_Poly(coefs,extrema[i]),rep);
        }*/

        float prevSign = zSign;
        float low = 0;
        float high;
        for (int i = 0; i < extrema.Count; i++)
        {
            high = extrema[i];
            if (high > 0)
            {
                float y = _Poly(coefs, high);
                if (y == 0)
                {
                    solutions.Add(high);
                }
                float ySign = Mathf.Sign(y);
                if (ySign * prevSign == -1)//opposite sign and nonzero on either
                {
                    solutions.Add(_Bisect(coefs, low, high));
                }

                low = high;
                prevSign = ySign;
            }
        }

        bool endPos = true; //endbehaviors is posative
        for (int i = 0; i < coefs.Count; i++)
        {
            if (coefs[i] != 0)
            {
                if (coefs[i] < 0)//negative coefs invert endbehavior
                {
                    endPos = !endPos;
                }
                break;
            }
        }
        //Debug.LogFormat("End behaviour goes to +inf_{1} : {0}; prevsign: {2}", endPos, rep, prevSign);

        if (prevSign!=0 && endPos!=(prevSign>0))
        {
            //Debug.LogFormat("Right zero_{0}", rep);
            //Debug.LogFormat("Poly(low={2})_{0} = {1}",rep,_Poly(coefs,low),low);
            solutions.Add(_Bisect(coefs, low));
            //Debug.LogFormat("Right zero_{0} = {1}", rep, solutions[solutions.Count-1]);
        }
        solutions.Sort();
        return solutions;
    }

    List<float> _Derivate(List<float> coefs)
    {
        List<float> nCoefs = new List<float>();
        for (int i = 0; i < coefs.Count-1; i++)
        {
            nCoefs.Add(coefs[i] * (coefs.Count - i - 1));
        }
        return nCoefs;
    }

    float _Poly(List<float> coefs, float x)
    {
        float ret = 0;
        foreach (float coef in coefs)
        {
            ret *= x;
            ret += coef;
        }
        return ret;
    }

    Complex DEPRECATED_Poly(float a, float b, float c, float d, float e, Complex x)
    {
        return x*x*x*x*a + x*x*x*b + x*x*c + x*d + e;
    }

    Vector2 DEPRECATEDLeadShot2(Vector2 relPos, Vector2 relVel, float vesselAccel)
    {
        float colTime = DEPRECATEDMinPosRoot(.25f * vesselAccel * vesselAccel,
            0,
            -relVel.sqrMagnitude,
            -2 * Vector2.Dot(relVel, relPos),
            -relPos.sqrMagnitude, .01f);
        //float colTime = (lowbound + upbound) / 2;
        //Debug.Log(colTime);

        Vector2 colPos = relPos + colTime * relVel;
        //marker.transform.position = (Vector3)colPos + transform.position;
        //marker.name = string.Format("+{0};{1}", colTime.ToString(), Time.time + colTime);
        return colPos.normalized;

    }

    Vector2 DEPRECATEDLeadShot(Vector2 relPos, Vector2 relVel, float vesselAccel)
    {
        float lowbound = 0;
        float upbound = 1;
        float val = DEPRECATED_QVal(relPos, relVel, vesselAccel, upbound);
        while (val <= 0)
        {
            lowbound = upbound;
            upbound *= 2;
            val = DEPRECATED_QVal(relPos, relVel, vesselAccel, upbound);
            //Debug.Log(string.Format("Doubling upbound to {0}, due to {1} <= 0", upbound, val));
        }
        //establish bounds of zero

        for (int i = 0; i < 15; i++)//magic number is precision limit
        {
            float mid = (lowbound + upbound) / 2;
            if (DEPRECATED_QVal(relPos, relVel, vesselAccel, mid) > 0)
            {
                upbound = mid; // (l- m+ u+) -> (l- ? m+)
            }
            else
            {
                lowbound = mid; // (l- m- u+) -> (m- ? u+)
            }
        }
        float colTime = (lowbound + upbound) / 2;
        //Debug.Log(colTime);

        Vector2 colPos = relPos + colTime * relVel;
        //marker.transform.position = (Vector3)colPos + transform.position;
        //marker.name = string.Format("+{0};{1}", colTime.ToString(), Time.time + colTime);
        return colPos.normalized;

    }

    float DEPRECATED_QVal(Vector2 pos, Vector2 vel, float accel, float time)
    {
        //Debug.Log(string.Format("Pos: {0}, Vel: {1}, Acc: {2}, Time: {3}", pos, vel, accel, time));
        return time * time * time * time * (.25f * accel * accel)
            - time * time * (vel.sqrMagnitude)
            - time * (2 * Vector2.Dot(vel, pos))
            - (pos.sqrMagnitude);
    }

    float DEPRECATEDMinPosRoot(float a, float b, float c, float d, float e, float prec)
    {
        //float k = 1 + Mathf.Max(Mathf.Abs(b / a), Mathf.Abs(c / a), Mathf.Abs(d / a), Mathf.Abs(e / a));
        float k = 1;
        Complex[] aproxRoots = {k*Complex.Pow(new Complex(.4,.9), 0),
                                k*Complex.Pow(new Complex(.4,.9), 1),
                                k*Complex.Pow(new Complex(.4,.9), 2),
                                k*Complex.Pow(new Complex(.4,.9), 3)};
        Complex[] newAproxRoots = { 0, 0, 0, 0 };

        for (int it = 0; it < 15; it++)
        {
            for (int i = 0; i < 4; i++)
            {
                Complex denom = 1;
                for (int j = 0; j < 4; j++)
                {
                    if (j != i)
                    {
                        denom *= (aproxRoots[i] - aproxRoots[j]);
                    }
                }

                newAproxRoots[i] = aproxRoots[i] - DEPRECATED_Poly(a, b, c, d, e, aproxRoots[i]) / denom;
            }
            aproxRoots = newAproxRoots;
        }

        float ret = -1;
        for (int i = 0; i < 4; i++)
        {
            //Debug.LogFormat("Root {0} = {1}", i, newAproxRoots[i]);
            if (Math.Abs(newAproxRoots[i].Imaginary) < prec)
            {
                if ((newAproxRoots[i].Real < ret || ret == -1) && newAproxRoots[i].Real > 0)
                {
                    ret = (float)newAproxRoots[i].Real;
                }
            }
        }
        return ret;
    }
}
