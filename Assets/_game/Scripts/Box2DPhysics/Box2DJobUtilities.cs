using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Advanced job system for batch operations on physics bodies.
/// Provides high-performance batch force application, queries, and more.
/// </summary>
public static class Box2DJobUtilities
{
    /// <summary>
    /// Applies forces to multiple physics bodies in parallel using Burst.
    /// </summary>
    public static void ApplyForceBatch(NativeArray<PhysicsBody> bodies, NativeArray<float2> forces)
    {
        if (bodies.Length != forces.Length)
        {
            Debug.LogError("Bodies and forces arrays must have the same length!");
            return;
        }

        var job = new ApplyForceBatchJob
        {
            bodies = bodies,
            forces = forces
        };

        job.Schedule(bodies.Length, 64).Complete();
    }

    /// <summary>
    /// Applies impulses to multiple physics bodies in parallel using Burst.
    /// </summary>
    public static void ApplyImpulseBatch(NativeArray<PhysicsBody> bodies, NativeArray<float2> impulses)
    {
        if (bodies.Length != impulses.Length)
        {
            Debug.LogError("Bodies and impulses arrays must have the same length!");
            return;
        }

        var job = new ApplyImpulseBatchJob
        {
            bodies = bodies,
            impulses = impulses
        };

        job.Schedule(bodies.Length, 64).Complete();
    }

    /// <summary>
    /// Sets velocities for multiple physics bodies in parallel using Burst.
    /// </summary>
    public static void SetVelocityBatch(NativeArray<PhysicsBody> bodies, NativeArray<float2> velocities)
    {
        if (bodies.Length != velocities.Length)
        {
            Debug.LogError("Bodies and velocities arrays must have the same length!");
            return;
        }

        var job = new SetVelocityBatchJob
        {
            bodies = bodies,
            velocities = velocities
        };

        job.Schedule(bodies.Length, 64).Complete();
    }

    /// <summary>
    /// Reads positions and velocities from multiple bodies efficiently.
    /// </summary>
    public static void ReadPhysicsDataBatch(
        NativeArray<PhysicsBody> bodies,
        NativeArray<float2> outPositions,
        NativeArray<float2> outVelocities,
        NativeArray<PhysicsRotate> outRotations)
    {
        var job = new ReadPhysicsDataBatchJob
        {
            bodies = bodies,
            positions = outPositions,
            velocities = outVelocities,
            rotations = outRotations
        };

        job.Schedule(bodies.Length, 64).Complete();
    }

    /// <summary>
    /// Applies a radial force from a point (like an explosion).
    /// </summary>
    public static void ApplyRadialForce(
        NativeArray<PhysicsBody> bodies,
        float2 center,
        float force,
        float radius)
    {
        var job = new ApplyRadialForceJob
        {
            bodies = bodies,
            center = center,
            force = force,
            radiusSquared = radius * radius
        };

        job.Schedule(bodies.Length, 64).Complete();
    }
}

/// <summary>
/// Burst-compiled job for applying forces in batch.
/// </summary>
[BurstCompile]
public struct ApplyForceBatchJob : IJobParallelFor
{
    public NativeArray<PhysicsBody> bodies;
    [ReadOnly] public NativeArray<float2> forces;

    public void Execute(int index)
    {
        var body = bodies[index];
        if (body.isValid && body.type == PhysicsBody.BodyType.Dynamic)
        {
            body.ApplyForce(forces[index], body.position);
        }
    }
}

/// <summary>
/// Burst-compiled job for applying impulses in batch.
/// </summary>
[BurstCompile]
public struct ApplyImpulseBatchJob : IJobParallelFor
{
    public NativeArray<PhysicsBody> bodies;
    [ReadOnly] public NativeArray<float2> impulses;

    public void Execute(int index)
    {
        var body = bodies[index];
        if (body.isValid && body.type == PhysicsBody.BodyType.Dynamic)
        {
            body.ApplyLinearImpulse(impulses[index], body.position);
        }
    }
}

/// <summary>
/// Burst-compiled job for setting velocities in batch.
/// </summary>
[BurstCompile]
public struct SetVelocityBatchJob : IJobParallelFor
{
    public NativeArray<PhysicsBody> bodies;
    [ReadOnly] public NativeArray<float2> velocities;

    public void Execute(int index)
    {
        var body = bodies[index];
        if (body.isValid && body.type == PhysicsBody.BodyType.Dynamic)
        {
            body.linearVelocity = velocities[index];
        }
    }
}

/// <summary>
/// Burst-compiled job for reading physics data in batch.
/// </summary>
[BurstCompile]
public struct ReadPhysicsDataBatchJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PhysicsBody> bodies;
    [WriteOnly] public NativeArray<float2> positions;
    [WriteOnly] public NativeArray<float2> velocities;
    [WriteOnly] public NativeArray<PhysicsRotate> rotations;

    public void Execute(int index)
    {
        var body = bodies[index];
        if (body.isValid)
        {
            positions[index] = body.position;
            velocities[index] = body.linearVelocity;
            rotations[index] = body.rotation;
        }
    }
}

/// <summary>
/// Burst-compiled job for applying radial forces (explosions).
/// </summary>
[BurstCompile]
public struct ApplyRadialForceJob : IJobParallelFor
{
    public NativeArray<PhysicsBody> bodies;
    [ReadOnly] public float2 center;
    [ReadOnly] public float force;
    [ReadOnly] public float radiusSquared;

    public void Execute(int index)
    {
        var body = bodies[index];
        if (!body.isValid || body.type != PhysicsBody.BodyType.Dynamic)
            return;

        float2 position = body.position;
        float2 direction = position - center;
        float distanceSquared = math.lengthsq(direction);

        // Only apply force if within radius
        if (distanceSquared < radiusSquared && distanceSquared > 0.001f)
        {
            float distance = math.sqrt(distanceSquared);
            float2 normalizedDir = direction / distance;

            // Falloff based on distance
            float falloff = 1f - (distance / math.sqrt(radiusSquared));
            float2 forceVector = normalizedDir * force * falloff;

            body.ApplyForce(forceVector, position);
        }
    }
}