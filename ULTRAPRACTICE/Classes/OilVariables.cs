using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace ULTRAPRACTICE.Classes
{
    public class OilVariables
    {
        public static Dictionary<Transform, int> stainIndices;
        public static Dictionary<Vector3Int, StainVoxel> stainVoxels;
        public static TransformAccessArray stainTransforms;
        public static int currentStainCount;

        public static void SaveVariables()
        {
            StainVoxelManager OilManager = MonoSingleton<StainVoxelManager>.Instance;

            currentStainCount = OilManager.currentStainCount;
            stainIndices = new Dictionary<Transform, int>(OilManager.stainIndices);
            stainVoxels = new Dictionary<Vector3Int, StainVoxel>(OilManager.stainVoxels);
            stainTransforms = new TransformAccessArray(10000);

            for (int i = 0; i < OilManager.stainTransforms.length; i++)
            {
                stainTransforms.Add(OilManager.stainTransforms[i]);
            }

        }

        public static void SetVariables()
        {
            StainVoxelManager OilManager = MonoSingleton<StainVoxelManager>.Instance;
            OilManager.stainVoxels.Clear();
            OilManager.stainIndices.Clear();

            CleanupTransforms();

            for (int i = 0; i < stainTransforms.length; i++)
            {
                OilManager.stainTransforms.Add(stainTransforms[i]);
            }

            foreach (var voxel in stainVoxels)
            {
                OilManager.stainVoxels.Add(voxel.Key, voxel.Value);
            }
            foreach (var voxel in stainIndices)
            {
                OilManager.stainIndices.Add(voxel.Key, voxel.Value);
            }

            OilManager.currentStainCount = currentStainCount;

        }

        private static void CleanupTransforms()
        {
            for (int num = MonoSingleton<StainVoxelManager>.Instance.currentStainCount - 1; num >= 0; num--)
            {
                RemoveStainAtIndex(num);
            }
        }

        private static void RemoveStainAtIndex(int removeIndex)
        {
            StainVoxelManager OilManager = MonoSingleton<StainVoxelManager>.Instance;

            int num = OilManager.currentStainCount - 1;
            if (removeIndex != num)
            {
                OilManager.instanceProps[removeIndex] = OilManager.instanceProps[num];
                OilManager.gasolineTransforms[removeIndex] = OilManager.gasolineTransforms[num];
                OilManager.gasolineTransforms[num] = float4x4.zero;
                Transform key = OilManager.stainTransforms[num];
                OilManager.stainIndices[key] = removeIndex;
            }
            OilManager.stainTransforms.RemoveAtSwapBack(removeIndex);
            OilManager.currentStainCount--;
        }
    }
}
