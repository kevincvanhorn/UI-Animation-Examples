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
    }
}
