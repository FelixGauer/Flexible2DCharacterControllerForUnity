using System;
using System.Collections.Generic;
using UnityEngine;

public class GroundModule
{
    private readonly PlayerControllerStats _playerControllerStats;
	
    private Vector2 _moveVelocity;

    private readonly JumpModule _jumpModule;
    private readonly DashModule _dashModule;

    public GroundModule(PlayerControllerStats playerControllerStats, JumpModule jumpModule, DashModule dashModule) 
    {
        _jumpModule = jumpModule;
        _playerControllerStats = playerControllerStats;
        _dashModule = dashModule;
    }

    public void Test()
    {
        
    }
}
//
// public class TestMonoBehaviour : MonoBehaviour
// {
//     private float _var; // 0.0 f
//     
//     private int _result; // 0
//
//     private TestClass newClass1 = new TestClass();
//     private TestClass newClass2;
//
//
//     int[] arrayName = new int[10];
//     
//     List<string> people = new List<string>();
//     
//     private void Awake()
//     {
//         newClass2 = new TestClass();
//
//         this._result = Doo();
//         
//         
//         TestClass testClass = new TestClass();
//         
//         testClass.Foo();
//     }
//
//     private int Doo()
//     {
//         int a = 5;
//         int b = 10;
//         
//         int sum = a + b;
//         
//         return sum;
//     }
// }
//
// public class TestClass : TestInterface
// {
//     private float _var;
//
//     public void Foo()
//     {
//         var test = 10f;
//         var test2 = 15f;
//         
//         var test3 = test + test2;
//         
//         _var = 10f;
//         
//         
//     }
// }

public interface ITestInterface<T>
{
    public void Foo()
    {
        Debug.Log("VAR");
    }
    
    public float Doo(T test);
    
    public abstract void Goo();
    
    string Name { get; set; }

    public event Action OnNotified;
}

public class TestClassInterface : ITestInterface<float>
{
    public float Doo(float test)
    {
        return test; 
    }

    public void Goo() { }

    public string Name { get; set; }
    public event Action OnNotified;
}

public class TestMono : MonoBehaviour, ITestInterface<float>
{
    private TestClassInterface _testClassInterface;

    private void Awake()
    {
        ((ITestInterface<float>)_testClassInterface).Foo();
        
        ((ITestInterface<float>)this).Foo();
    }

    public float Doo(float test)
    {
        return test;
    }

    public void Goo() { }
    public string Name { get; set; }
    public event Action OnNotified;
}

public struct TestStruct 
{
    private float _var;

    public void Foo()
    {
        var test = 10f;
        var test2 = 15f;
        
        var test3 = test + test2;
        
        _var = 10f;
    }
}

public class SRPMain : MonoBehaviour
{
    public SRPSystem1 SrpSystem1;
    public SRPSystem2 SrpSystem2;
    public SRPSystem3 SrpSystem3;
    
    
    private void Awake()
    {
        SrpSystem1 = new SRPSystem1();
        SrpSystem2 = new SRPSystem2();
        SrpSystem3 = new SRPSystem3();
    }

    private void Update()
    {
        SrpSystem1.Foo();
        SrpSystem2.Foo();
        SrpSystem3.Foo();
    }
}

public class SRPSystem1
{
    public void Foo()
    {
        Debug.Log("VAR");
    }
}

public class SRPSystem2
{
    public void Foo()
    {
        Debug.Log("VAR");
    }
}

public class SRPSystem3
{
    public void Foo()
    {
        Debug.Log("VAR");
    }
}







