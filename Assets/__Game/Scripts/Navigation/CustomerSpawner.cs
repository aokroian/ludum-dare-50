using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using __Game.Scripts.Actors;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider), typeof(CustomerCache))]
public class CustomerSpawner : MonoBehaviour
{
    [Serializable]
    public class CustomerCostHolder
    {
        public GameObject customerPrefab;
        public float cost;
    }

    #region Inspector

    [SerializeField] private float startCost = 1f;
    [SerializeField] private float costDelta = 0.01f;
    [SerializeField] private float minSpawnTime = 0.5f;
    [SerializeField] private float maxSpawnTime = 1f;
    [SerializeField] private BoxCollider endCollider;
    [SerializeField] private CustomerCostHolder[] customersPrefabs;

    #endregion

    #region Fields

    private BoxCollider spawnerCollider;
    private Bounds spawnerBounds;
    private GoodType[] allGoodsTypes;
    private float currentCost;
    private float nextSpawnTime;
    private float difficultyMultiplier = 1f;
    private CustomerCache cache;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        allGoodsTypes = Resources.LoadAll<GoodType>("");
        cache = GetComponent<CustomerCache>();
        cache.Initialize(customersPrefabs.Select(holder => holder.customerPrefab).ToArray());
        Debug.Log("Products count: " + allGoodsTypes.Length);
        currentCost = startCost;
        spawnerCollider = GetComponent<BoxCollider>();
        spawnerBounds = spawnerCollider.bounds;
        nextSpawnTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (nextSpawnTime <= Time.time)
        {
            nextSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
            SpawnCustomer();
        }
    }

    private void FixedUpdate()
    {
        currentCost += costDelta;
    }

    private GameObject SpawnCustomer()
    {
        //var customerCost = Random.Range(0f, currentCost);
        // var customer = Instantiate(customersPrefabs[0].customerPrefab, CalcCustomerPosition(), Quaternion.identity);
        var prefabHolder = customersPrefabs[Random.Range(0, customersPrefabs.Length)];
        var customer = cache.Get(prefabHolder.customerPrefab, CalcCustomerPosition(), Quaternion.identity);
        PrepareCustomer(customer);
        return customer;
    }

    // consider of previous positions?
    private Vector3 CalcCustomerPosition()
    {
        return new Vector3(Random.Range(spawnerBounds.min.x, spawnerBounds.max.x), 0f,
            Random.Range(spawnerBounds.min.z, spawnerBounds.max.z));
    }

    private void PrepareCustomer(GameObject customer)
    {
        var movement = customer.GetComponent<CharacterMovement>();
        var bounds = endCollider.bounds;
        movement.target = new Vector3(Random.Range(bounds.min.x, bounds.max.x), 0f,
            Random.Range(bounds.min.z, bounds.max.z));
        movement.onReachDest = () => cache.Release(customer);

        var controller = customer.GetComponent<MobController>();
        controller.SetDemands(CalcProducts());
    }

    private Dictionary<GoodType, int> CalcProducts()
    {
        var result = new Dictionary<GoodType, int>();
        var type = allGoodsTypes[Random.Range(0, allGoodsTypes.Length)];
        result[type] = 5;
        return result;
    }
}