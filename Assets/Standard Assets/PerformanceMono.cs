using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime;
using ILRuntime.Runtime.CLRBinding;
using UnityEngine;
using XLua;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class PerformanceMono : MonoBehaviour
{
    [CSharpCallLua]
    public delegate void LuaCallPerfCase(StringBuilder sb);

    private readonly LuaEnv _luaenv = new LuaEnv();

    private readonly List<string> _tests = new List<string>();
    private AppDomain _appdomain;

    private void Awake()
    {
        _tests.Add("TestMandelbrot");
        _tests.Add("Test0");
        _tests.Add("Test1");
        _tests.Add("Test2");
        _tests.Add("Test3");
        _tests.Add("Test4");
        _tests.Add("Test5");
        _tests.Add("Test6");
        _tests.Add("Test7");
        _tests.Add("Test8");
        _tests.Add("Test9");
        _tests.Add("Test10");
        _tests.Add("Test11");
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        yield return LoadHotFixAssembly();
        var assembly = Assembly.Load("Assembly-CSharp.dll");
        var huatuoAssembly = Assembly.Load(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "Assembly-CSharp.dll")));
        
        string luaStr = @"require 'performance'";
        _luaenv.DoString(luaStr);

        foreach (var testName in _tests)
        {
            Log($"================{testName}================");
            // var sb = new StringBuilder();
            // sb.AppendLine("xlua:");
            // var perf = _luaenv.Global.GetInPath<LuaCallPerfCase>(testName);
            // perf(sb);
            // Log(sb);
            // sb = new StringBuilder();
            // sb.AppendLine("ilruntime:");
            // _appdomain.Invoke("HotFix_Project.TestPerformance", testName, null, sb);
            // Log(sb);

            {
                var sb = new StringBuilder();
                sb.AppendLine("huatuo:");
                var type = huatuoAssembly.GetType("HotFix_Project.TestPerformance");
                var m = type.GetMethod(testName);
                Debug.Assert(m != null);
                m.Invoke(null, new object[] {sb});
                Log(sb);
            }
            
            {
                var sb = new StringBuilder();
                sb.AppendLine("il2cpp:");
                var type = assembly.GetType("HotFix_Project.TestPerformance");
                var m = type.GetMethod(testName);
                Debug.Assert(m != null);
                m.Invoke(null, new object[] {sb});
                Log(sb);
            }
        }
    }

    private static void Log(object s)
    {
        if (Application.isEditor)
        {
            Debug.Log(s);
        }
        else
        {
            Console.WriteLine(s);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private IEnumerator LoadHotFixAssembly()
    {
        //???????????????????????????????????????????????????dll??????????????????AssetBundle?????????????????????????????????????????????????????????StreammingAssets????????????
        //????????????????????????????????????????????????????????????dll

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //??????DLL?????????????????????HotFix_Project.sln??????????????????????????????????????????????????????StreamingAssets??????VS???????????????????????????????????????????????????????????????
        //???????????????Assets\Samples\ILRuntime\1.6\Demo\HotFix_Project~
        //?????????????????????????????????????????????????????????????????????Android????????????????????????????????????
#if UNITY_ANDROID
        WWW www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.dll");
#else
        var www = new WWW("file:///" + Application.streamingAssetsPath + "/Assembly-CSharp.dll");
#endif
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            Debug.LogError(www.error);
        var dll = www.bytes;
        www.Dispose();

        //PDB???????????????????????????????????????????????????????????????????????????????????????PDB??????????????????????????????????????????????????????????????????PDB???????????????LoadAssembly?????????pdb???null??????
#if UNITY_ANDROID
        www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.pdb");
#else
        www = new WWW("file:///" + Application.streamingAssetsPath + "/Assembly-CSharp.pdb");
#endif
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            Debug.LogError(www.error);
        var pdb = www.bytes;
        var fs = new MemoryStream(dll);
        var p = new MemoryStream(pdb);
        try
        {
            _appdomain = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntimeJITFlags.JITImmediately);
            _appdomain.LoadAssembly(fs, p, new PdbReaderProvider());
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }

        InitializeILRuntime();
    }

    private void InitializeILRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //??????Unity???Profiler????????????????????????????????????????????????????????????????????????ILRuntime??????????????????ID??????????????????????????????????????????Profiler
        _appdomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
#endif
        _appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
        _appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
        _appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
        CLRBindingUtils.Initialize(_appdomain);
    }

    public void LoadHotFixAssemblyStack()
    {
        //???????????????ILRuntime???AppDomain???AppDomain?????????????????????????????????AppDomain???????????????????????????
        _appdomain = new AppDomain();
        StartCoroutine(LoadHotFixAssembly());
    }

    public void LoadHotFixAssemblyRegister()
    {
        //???????????????ILRuntime???AppDomain???AppDomain?????????????????????????????????AppDomain???????????????????????????
        //ILRuntimeJITFlags.JITImmediately???????????????????????????VM??????????????????
        _appdomain = new AppDomain(ILRuntimeJITFlags.JITImmediately);
        StartCoroutine(LoadHotFixAssembly());
    }
}