using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace SDGA{
    public class Utils
    {
        public static void CancelAndDispose(ref CancellationTokenSource cts)
        {
            if (cts != null)
            {
                if (!cts.IsCancellationRequested) cts.Cancel();
                cts.Dispose();
            }
        }

        public static void CancelAndDisposeNew(ref CancellationTokenSource cts)
        {
            CancelAndDispose(ref cts);
            cts = new CancellationTokenSource();
        }
        
        public static void CancelAndDisposeNew(ref CancellationTokenSource cts, MonoBehaviour mono)
        {
            CancelAndDispose(ref cts);
            cts = CancellationTokenSource.CreateLinkedTokenSource(mono.destroyCancellationToken); // New token just linked to lifetime of mono
        }
    }
}
