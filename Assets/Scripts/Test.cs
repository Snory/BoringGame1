using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class Test : MonoBehaviour
{
    private CancellationTokenSource _source;

    private void Start()
    {
        _source = new CancellationTokenSource();
        StartTest(_source.Token);
        Debug.Log("Coze");
    }

    private async Task StartTest(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Debug.Log("Lol");
            await Task.Delay(System.TimeSpan.FromSeconds(5)); //počkej tam jak dlouho uznáš za vhodné
        }
    }

    private void OnDisable()
    {
        Debug.Log("Disable");
        Stop();
    }

    private void OnDestroy()
    {
        Debug.Log("Destroy");
        Stop();
    }

    private void Stop()
    {
        if(_source is not null)
        {
            _source.Cancel();
            _source.Dispose();
            _source = null;
        }
    }

}
