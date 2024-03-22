using System;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace Bosses.Worm
{
    public class ShootLaser : MonoBehaviour
    {
        public float width, damage;
        public Sprite squareSprite;

        public void Shoot(float timeToLive, float riftDist, Vector3 otherRiftLoc, float otherRiftRot)
        {
            var angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            var rift1 = new GameObject("Laser rift")
            {
                transform =
                {
                    localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg + 180),
                    localPosition = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * riftDist,
                    localScale = new Vector3(1, 4)
                }
            };
            var spriteRenderer1 = rift1.AddComponent<SpriteRenderer>();
            spriteRenderer1.sprite = squareSprite;
            spriteRenderer1.color = Color.green;
            rift1.AddComponent<DestroyAfter>().timeToLive = timeToLive + 1;
            
            var rift2 = new GameObject("Laser rift 2")
            {
                transform =
                {
                    localRotation = Quaternion.Euler(0, 0, otherRiftRot),
                    localPosition = otherRiftLoc,
                    localScale = new Vector3(1, 4)
                }
            };
            var spriteRenderer2 = rift2.AddComponent<SpriteRenderer>();
            spriteRenderer2.sprite = squareSprite;
            spriteRenderer2.color = Color.green;
            rift2.AddComponent<DestroyAfter>().timeToLive = timeToLive + 1;

            var laser1 = new GameObject("Laser 1").AddComponent<LaserController>();
            laser1.transform.SetParent(transform);
                
            laser1.width = width;
            laser1.damage = damage;
            laser1.sprite = squareSprite;
            laser1.timeToLive = timeToLive;
            laser1.length = riftDist;
            laser1.fadeTime = 1;
            
            var laser2 = new GameObject("Laser 2").AddComponent<LaserController>();
            laser2.transform.SetParent(rift2.transform);
                
            laser2.width = width;
            laser2.damage = damage;
            laser2.sprite = squareSprite;
            laser2.timeToLive = timeToLive;
            laser2.length = 9999;
            laser2.fadeTime = 1;
        }
        
        public void Shoot(float timeToLive)
        {
            var laser = new GameObject("Laser").AddComponent<LaserController>();
            laser.transform.SetParent(transform);
            
            laser.width = width;
            laser.damage = damage;
            laser.sprite = squareSprite;
            laser.timeToLive = timeToLive;
            laser.length = 9999;
            laser.fadeTime = 1;
        }
    }
}