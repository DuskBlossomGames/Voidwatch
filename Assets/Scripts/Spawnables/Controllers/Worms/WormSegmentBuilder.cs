using System;
using JetBrains.Annotations;
using Spawnables.Damage;
using UnityEngine;

namespace Spawnables.Controllers.Worms
{
    public class WormSegmentBuilder : MonoBehaviour
    {
        public int length;
        public Sprite headSprite;
        public Sprite bodySprite;
        public Sprite tailSprite;

        public GameObject segmentPrefab;

        [NonSerialized] [CanBeNull] public Action buildCallback;

        // Start is called before the first frame update
        void Start()
        {
            if (length < 2)
            {
                throw new Exception(string.Format("Length: {0} is less than 2, the minimum length", length));
            }

            var segLength = segmentPrefab.GetComponent<WormSegment>().segLength;
        
            GameObject child, oldChild;
            Vector3 relPos = Vector3.zero;
            child = Instantiate(segmentPrefab, transform);
            child.transform.localRotation = Quaternion.Euler(0, 0, -90);
            child.GetComponent<SpriteRenderer>().sprite = headSprite;
            child.GetComponent<WormSegment>().form = WormSegment.Form.Head;
            child.GetComponent<WormDamageable>().root = gameObject;
            child.GetComponent<WormDamageable>().trackDeath = true;
            child.transform.localPosition = relPos;
            oldChild = child;
            relPos.x -= segLength;

            var head = child;
            GetComponent<MultiDamageable>().HealthChanged += (_, n) =>
            {
                if (n <= 0) head.GetComponent<EnemyDamageable>().SpawnHealthPickups();
            };
        
            for (int i = 0; i < length-2; i++)
            {
                child = Instantiate(segmentPrefab, transform);
                child.transform.localRotation = Quaternion.Euler(0, 0, -90);
                child.GetComponent<SpriteRenderer>().sprite = bodySprite;
                child.GetComponent<WormSegment>().form = WormSegment.Form.Middle;
                child.GetComponent<WormSegment>().prev = oldChild;
                oldChild.GetComponent<WormSegment>().next = child;
                child.transform.localPosition = relPos;
                child.GetComponent<WormDamageable>().root = gameObject;
                child.GetComponent<PositionHinter>().enabled = false;
                oldChild = child;
                relPos.x -= segLength;
            }
            child = Instantiate(segmentPrefab, transform);
            child.transform.localRotation = Quaternion.Euler(0, 0, -90);
            child.GetComponent<SpriteRenderer>().sprite = tailSprite;
            child.GetComponent<WormSegment>().form = WormSegment.Form.Tail;
            child.GetComponent<WormSegment>().prev = oldChild;
            oldChild.GetComponent<WormSegment>().next = child;
            child.transform.localPosition = relPos;
            child.GetComponent<WormDamageable>().root = gameObject;
            child.GetComponent<PositionHinter>().enabled = false;

            buildCallback?.Invoke();
        }

        private void Update()
        {
            if (transform.childCount == 0) Destroy(gameObject);
        }
    }
}
