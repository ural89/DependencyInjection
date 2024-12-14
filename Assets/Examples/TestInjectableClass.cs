using DependencyInjection;
using UnityEngine;

public class TestInjectableClass : MonoBehaviour
{
    [Inject] private IServiceA serviceA;
    [Inject] ServiceB serviceB;
    [Inject] private MBTest mBTest;
    FactoryA factoryA;
    [Inject]
    public void Init(FactoryA factoryA)
    {
        this.factoryA = factoryA;
        Debug.Log("Injected " + factoryA + " into a method");
    }
    private void Awake()
    {
        serviceA.Initialize("Hello");
        serviceB.Initialize("Hello");
        mBTest.Init();
    }
}