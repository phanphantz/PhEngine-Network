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

        static Operation currentRefreshOperation;
        
        const string ClientAccessTokenExpireErrorMessage = "Client's Access Token is expired. Fixing...";
        
        public void BindValidateActions(APIOperation apiOp)
        {
            if (!APICaller.Instance.HasAccessToken)
                return;
            
            //Don't interact with the refresh API operation itself!
            if (apiOp == currentRefreshOperation)
                return;
            
            var parentFlow = apiOp.ParentFlow;
            if (isCheckBeforeCall)
            {
                apiOp.AbortOn(() => TryHandleOnClientExpired(apiOp, parentFlow), ClientAccessTokenExpireErrorMessage, FailureHandling.None, -1);
            }

            if (isCheckAfterCall)
                apiOp.OnReceiveResponse += () => TryHandleOnServerExpired(apiOp, parentFlow);
        }
        
#if UNITASK
        public async UniTask ValidateBeforeCallTask(APIOperation apiOp)
        {
            if (!APICaller.Instance.HasAccessToken)
                return;
            
            if (!isCheckBeforeCall)
                return;
            
            //Don't interact with the refresh API operation itself!
            if (apiOp == currentRefreshOperation)
                return;
            
            if (!IsExpiredBeforeCall())
                return;
            
            Debug.LogError(ClientAccessTokenExpireErrorMessage);
            currentRefreshOperation = CreateRefreshAccessTokenCall();
            await currentRefreshOperation.Task();
        }

        public async UniTask<bool> TryValidateAfterCallTask(APIOperation apiOp, ServerResult result)
        {
            if (!APICaller.Instance.HasAccessToken)
                return false;
            
            if (!isCheckAfterCall)
                return false;
            
            //Don't interact with the refresh API operation itself!
            if (apiOp == currentRefreshOperation)
                return false;

            if (!IsExpiredAfterCall(result))
                return false;

            currentRefreshOperation = CreateRefreshAccessTokenCall();
            await currentRefreshOperation.Task();
            await apiOp.Task();
            return true;
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