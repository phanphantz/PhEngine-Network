using System;
using PhEngine.Core.Operation;
using UnityEngine;

#if UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace PhEngine.Network
{
    public abstract class AccessTokenValidator : ScriptableObject
    {
        [SerializeField] DateTimeFormat timeFormat;
        [SerializeField] bool isCheckBeforeCall = true;
        [SerializeField] bool isCheckAfterCall = true;

        Operation currentRefreshOperation;
        
        public void BindValidateActions(APIOperation apiOp)
        {
            //Don't interact with the refresh API operation itself!
            if (apiOp == currentRefreshOperation)
                return;
            
            var parentFlow = apiOp.ParentFlow;
            if (isCheckBeforeCall)
                apiOp.AbortOn(() => TryHandleOnClientExpired(apiOp, parentFlow), "Client's Access Token is expired. Fixing...", FailureHandling.None, -1);

            if (isCheckAfterCall)
                apiOp.OnReceiveResponse += () => TryHandleOnServerExpired(apiOp, parentFlow);
        }
        
#if UNITASK
        public async UniTask ValidateBeforeCallTask(APIOperation apiOp)
        {
            if (!isCheckBeforeCall)
                return;
            
            //Don't interact with the refresh API operation itself!
            if (apiOp == currentRefreshOperation)
                return;
            
            if (!IsExpiredBeforeCall())
                return;
            
            await CreateRefreshAccessTokenCall().Task();
        }

        public async UniTask ValidateAfterCallTask(APIOperation apiOp, ServerResult result)
        {
            if (!isCheckAfterCall)
                return;
            
            //Don't interact with the refresh API operation itself!
            if (apiOp == currentRefreshOperation)
                return;

            if (!IsExpiredAfterCall(result))
                return;

            await CreateRefreshAccessTokenCall().Task();
            await apiOp.Task();
        }
#endif

        bool TryHandleOnClientExpired(APIOperation startOperation, Flow flow)
        {
            var isExpired = IsExpiredBeforeCall();
            if (isExpired)
                Rerun(startOperation, flow);

            return isExpired;
        }

        protected virtual bool IsExpiredBeforeCall()
        {
            var currentTime = timeFormat == DateTimeFormat.Local ? DateTime.Now : DateTime.UtcNow;
            return APICaller.Instance.IsAccessTokenExpired(currentTime);
        }

        void Rerun(APIOperation startOperation, Flow flow = null)
        {
            if (flow != null && flow.RunningOperation is ChainedOperation)
            {
                currentRefreshOperation = CreateRefreshAccessTokenCall();
                if (currentRefreshOperation != null)
                {
                    var index = Array.IndexOf(flow.Operations, startOperation);
                    flow.InsertOneShot(index, currentRefreshOperation);
                    flow.RunAsSeries(OnStopBehavior.CancelAll, index);
                }
            }
            else
            {
                var retryFlow = new Flow();
                currentRefreshOperation = CreateRefreshAccessTokenCall();
                if (currentRefreshOperation != null)
                    retryFlow.Add(currentRefreshOperation);
                
                retryFlow.Add(startOperation);
                retryFlow.RunAsSeries();
            }
        }

        void TryHandleOnServerExpired(APIOperation startOperation, Flow flow)
        {
            if (!IsExpiredAfterCall(startOperation.Result))
                return;
            
            Rerun(startOperation, flow);
        }

        protected abstract bool IsExpiredAfterCall(ServerResult result);
        protected abstract Operation CreateRefreshAccessTokenCall();
    }
}