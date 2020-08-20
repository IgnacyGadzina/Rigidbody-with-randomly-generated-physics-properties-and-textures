using UnityEngine;

public class AtomScript : MonoBehaviour
{
    /// <summary>
    /// The seed of this gameObject.
    /// </summary>
    ///
    public float range;
    public bool refresh;
    public int seed;
    new Rigidbody rigidbody;
    Rigidbody[] rigidbodys;
    PhysicMaterial physicMaterial;
    new Renderer renderer;
    /// <summary>
    /// CubeScript of colliding gameobject.
    /// </summary>
    AtomScript cubeScript;
    public Shape shape;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        physicMaterial = GetComponent<Collider>().material;
        renderer = GetComponent<Renderer>();
        rigidbodys = FindObjectsOfType<Rigidbody>();
        GenerateMaterial();
        GeneratePhysicMaterial();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Cube")
        {
            cubeScript = collision.gameObject.GetComponent<AtomScript>();
            //Strenght of this cube.
            float f = Random.Range(3f, 7f);
            if (collision.relativeVelocity.magnitude > f)
            {
                Destroy(collision.gameObject);
            }
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.tag == "Cube")
        {
            cubeScript = collision.gameObject.GetComponent<AtomScript>();
            float interiaTensorRotation = collision.rigidbody.inertiaTensorRotation.eulerAngles.magnitude;

            //Destroying
            if (interiaTensorRotation > MaterialStreght())
            {
                Destroy(collision.gameObject);
            }

            //Sticking.
            if (IsSticking(cubeScript.seed))
            {
                gameObject.AddComponent<FixedJoint>();
                FixedJoint fixedJoint = GetComponent<FixedJoint>();
                fixedJoint.connectedBody = collision.rigidbody;

                /*
                collision.gameObject.transform.SetParent(transform);
                if (collision.transform != transform.parent)
                {
                    collision.rigidbody.isKinematic = true;
                }*/
            }

            //Detaching.
            if (IsDetaching(cubeScript.seed))
            {

            }
        }
    }
    public enum Shape
    {
        sphere,
        cube
    }
    private void FixedUpdate()
    {
        foreach (Rigidbody otherBody in rigidbodys)
        {
            if (otherBody.gameObject != gameObject)
            {
                bool isColliding = false;
                AtomScript otherScript = otherBody.GetComponent<AtomScript>();
                float distance = Vector3.Distance(otherBody.position, transform.position) - range - otherScript.range;
                float sqrDistance = (otherBody.position - transform.position).sqrMagnitude;
                if (shape == Shape.sphere)
                {
                    if (distance < range)
                    {
                        isColliding = true;
                    }
                }
                if (shape == Shape.cube)
                {
                    Vector3 vector3 = otherBody.position - transform.position;
                    if (vector3.x < range && vector3.y < range && vector3.z < range)
                    {
                        isColliding = true;
                    }
                }
                if (isColliding)
                {
                    Vector3 direction = (otherBody.position - rigidbody.position).normalized;
                    Vector3 force = (direction * rigidbody.mass * otherBody.mass) / sqrDistance;
                    if (!isNaN(force) && force.sqrMagnitude < 1024)
                    {
                        otherBody.AddForce(force);
                    }
                    bool isNaN(Vector3 vector3)
                    {
                        if (float.IsNaN(vector3.x))
                        {
                            return true;
                        }
                        else if (float.IsNaN(vector3.y))
                        {
                            return true;
                        }
                        else if (float.IsNaN(vector3.z))
                        {
                            return true;
                        }
                        return false;
                    }
                }
                else if (!isColliding && otherScript.seed == seed)
                {
                    Vector3 direction = (otherBody.position - transform.position).normalized;
                    Vector3 force = (direction * rigidbody.mass * otherBody.mass) / sqrDistance;
                    rigidbody.AddForce(force);
                }
            }
        }
    }
    private void Update()
    {
        if (refresh)
        {
            GeneratePhysicMaterial();
            refresh = false;
        }
    }
    private void GeneratePhysicMaterial()
    {
        Random.InitState(seed);
        rigidbody.mass = Random.Range(0.01f, 1f);
        physicMaterial.bounciness = Random.value;
        physicMaterial.dynamicFriction = Random.value;
        physicMaterial.staticFriction = Random.value;
        physicMaterial.frictionCombine = RandomCombine();
        physicMaterial.bounceCombine = RandomCombine();

        PhysicMaterialCombine RandomCombine()
        {
            PhysicMaterialCombine physicMaterialCombine = new PhysicMaterialCombine();
            int randomNumber = Random.Range(1, 4);
            switch (randomNumber)
            {
                case 1:
                    physicMaterialCombine = PhysicMaterialCombine.Average;
                    break;
                case 2:
                    physicMaterialCombine = PhysicMaterialCombine.Minimum;
                    break;
                case 3:
                    physicMaterialCombine = PhysicMaterialCombine.Multiply;
                    break;
                case 4:
                    physicMaterialCombine = PhysicMaterialCombine.Maximum;
                    break;
            }
            return physicMaterialCombine;
        }
    }
    private float MaterialStreght()
    {
        int min = 3,
            max = 12;
        Random.InitState(seed);
        float f = Random.Range(min, max);
        return f;
    }
    private bool IsDetaching(int seed2)
    {
        bool b = false;
        Random.InitState(UnitedSeed(seed, seed2));
        if (Random.Range(0, 1) == 1)
        {
            b = true;
        }
        return b;
    }
    private bool IsSticking(int seed2)
    {
        bool output = false;
        Random.InitState(UnitedSeed(seed, seed2));
        if (Random.Range(0, 1) == 1)
        {
            output = true;
        }
        return output;
    }
    private int UnitedSeed(int seed1, int seed2)
    {
        int output = seed1 - seed2;
        return output;
    }
}

#region Notes

/*void Joint()
{

    if (joinedGameObjects.Count == 0)
    {
        AddFixedJoint();
    }
    else
    {
        for (int i = 0; i < joinedGameObjects.Count; i++)
        {
            if (joinedGameObjects[i] == collision.gameObject)
            {
                break;
            }
            else if (i == joinedGameObjects.Count - 1)
            {
                AddFixedJoint();
            }
        }
    }
    void AddFixedJoint()
    {
        float f = 3;
        f *= rigidbody.mass;
        f *= physicMaterial.dynamicFriction;
        f *= physicMaterial.staticFriction;
        f /= collisionRigidbody.mass;

        if (collision.relativeVelocity.magnitude < f)
        {
            *//*
            collisionRigidbody.isKinematic = rigidbody.isKinematic;
            collisionRigidbody.useGravity = rigidbody.useGravity;*//*
            gameObject.AddComponent<FixedJoint>().connectedBody = collisionRigidbody;
            joinedGameObjects.Add(collision.gameObject);
        }
    }
}*/

/*
        Rigidbody collisionRigidbody = collision.collider.GetComponent<Rigidbody>();
            f += physicMaterial.bounciness;
            f -= physicMaterial.dynamicFriction;
            f -= physicMaterial.staticFriction;
            f += rigidbody.mass;
            f /= collisionRigidbody.mass;*/

#endregion