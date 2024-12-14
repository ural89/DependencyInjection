using System.Collections;
using System.Collections.Generic;
using DependencyInjection;
using UnityEngine;

public class Provider : MonoBehaviour, IDependencyProvider
{
    [SerializeField] private MBTest mbTest;
    [Provide]
    public IServiceA ProvideServiceA()
    {
        return new ServiceA();
    }
    [Provide]
    public ServiceB ProvideServiceB()
    {
        return new ServiceB();
    }
    [Provide]
    public FactoryA ProvideFactoryA()
    {
        return new FactoryA();
    }
    [Provide]
    public MBTest GetMBTest()
    {
        return mbTest;
    }
}
public interface IServiceA
{
    public void Initialize(string message);
}
public class ServiceA : IServiceA
{
    public void Initialize(string message)
    {
        Debug.Log("Service A initialized " + message);
    }
}

public class ServiceB
{
    public void Initialize(string message)
    {
        Debug.Log("Service B initialized " + message);
    }
}

public class FactoryA
{
    ServiceA cachedServiceA;
    public ServiceA CreateServiceA()
    {
        if (cachedServiceA == null)
        {
            cachedServiceA = new ServiceA();
        }
        return cachedServiceA;
    }
}