using System;
using System.Collections.Generic;
using RunRich.Data.Definitions;
using UnityEngine;

namespace RunRich.Runtime.Pickups
{
    /// <summary>
    /// Designer-authored pickup strip. The strip and its four-slot rows are part of the scene.
    /// Runtime may instantiate only the collectible prefabs referenced by the rows.
    /// </summary>
    public sealed class PickupStrip : MonoBehaviour
    {
        [Serializable]
        public struct PickupRow
        {
            [SerializeField] private PickupDefinitionSO slot0;
            [SerializeField] private PickupDefinitionSO slot1;
            [SerializeField] private PickupDefinitionSO slot2;
            [SerializeField] private PickupDefinitionSO slot3;

            public PickupRow(PickupDefinitionSO slot0, PickupDefinitionSO slot1, PickupDefinitionSO slot2, PickupDefinitionSO slot3)
            {
                this.slot0 = slot0;
                this.slot1 = slot1;
                this.slot2 = slot2;
                this.slot3 = slot3;
            }

            public PickupDefinitionSO GetSlot(int index)
            {
                return index switch
                {
                    0 => slot0,
                    1 => slot1,
                    2 => slot2,
                    3 => slot3,
                    _ => null
                };
            }
        }

        [SerializeField, Min(0.1f)] private float rowSpacing = 1.8f;
        [SerializeField, Min(0.1f)] private float slotSpacing = 1.3f;
        [SerializeField] private float verticalOffset = 0.25f;
        [SerializeField] private PickupRow[] rows = Array.Empty<PickupRow>();

        private readonly List<GameObject> _spawned = new();

        public float RowSpacing => rowSpacing;
        public float SlotSpacing => slotSpacing;
        public IReadOnlyList<PickupRow> Rows => rows;

        public void Configure(float newRowSpacing, float newSlotSpacing, float newVerticalOffset, PickupRow[] newRows)
        {
            rowSpacing = Mathf.Max(0.1f, newRowSpacing);
            slotSpacing = Mathf.Max(0.1f, newSlotSpacing);
            verticalOffset = newVerticalOffset;
            rows = newRows ?? Array.Empty<PickupRow>();
        }

        public void PrepareForRun()
        {
            EnsureSpawned();
            for (var i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                    _spawned[i].SetActive(true);
            }
        }

        private void EnsureSpawned()
        {
            if (_spawned.Count > 0)
                return;

            for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                for (var slotIndex = 0; slotIndex < 4; slotIndex++)
                {
                    var definition = rows[rowIndex].GetSlot(slotIndex);
                    if (definition == null || definition.Prefab == null)
                        continue;

                    var instance = Instantiate(definition.Prefab, transform, false);
                    instance.name = $"{definition.name}_R{rowIndex:00}_S{slotIndex}";
                    instance.transform.localPosition = GetLocalSlotPosition(rowIndex, slotIndex);
                    instance.transform.localRotation = Quaternion.identity;
                    _spawned.Add(instance);
                }
            }
        }

        private Vector3 GetLocalSlotPosition(int rowIndex, int slotIndex)
        {
            var x = (slotIndex - 1.5f) * slotSpacing;
            var z = rowIndex * rowSpacing;
            return new Vector3(x, verticalOffset, z);
        }

        private void OnDrawGizmos()
        {
            if (rows == null) return;

            for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                for (var slotIndex = 0; slotIndex < 4; slotIndex++)
                {
                    var definition = rows[rowIndex].GetSlot(slotIndex);
                    var world = transform.TransformPoint(GetLocalSlotPosition(rowIndex, slotIndex));

                    if (definition == null)
                    {
                        Gizmos.color = new Color(1f, 1f, 1f, 0.22f);
                        Gizmos.DrawWireSphere(world, 0.12f);
                    }
                    else
                    {
                        Gizmos.color = definition.WealthDelta < 0
                            ? new Color(1f, 0.2f, 0.2f, 0.9f)
                            : new Color(0.2f, 1f, 0.35f, 0.9f);
                        Gizmos.DrawSphere(world, 0.16f);
                    }
                }
            }
        }
    }
}
