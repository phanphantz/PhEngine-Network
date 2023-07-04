﻿using System;
using PhEngine.Core.Operation;
using UnityEngine;

namespace PhEngine.Network
{
    public abstract class AccessTokenValidator : ScriptableObject
    {
        [SerializeField] DateTimeFormat timeFormat;
        [SerializeField] bool isCheckBeforeCall = true;
        [SerializeField] bool isCheckAfterFail = true;
        
        public void Track(APIOperation apiOp, Flow parentFlow = null)
        {
            if (isCheckBeforeCall)
                apiOp.AbortOn(() => TryHandleOnClientExpired(apiOp, parentFlow), "Client's Access Token is expired. Fixing...", FailureHandling.None, -1);

            if (isCheckAfterFail)
                apiOp.OnFail += result => TryHandleOnServerExpired(apiOp, parentFlow, result);
        }

        bool TryHandleOnClientExpired(APIOperation startOperation, Flow flow)
        {
            var currentTime = timeFormat == DateTimeFormat.Local ? DateTime.Now : DateTime.UtcNow;
            var isExpired = IsExpiredBeforeCall(currentTime);
            if (isExpired)
                Rerun(startOperation, flow);

            return isExpired;
        }

        protected virtual bool IsExpiredBeforeCall(DateTime currentTime)
        {
            return APICaller.Instance.IsAccessTokenExpired(currentTime);
        }

        void Rerun(APIOperation startOperation, Flow flow = null)
        {
            var newFlow = new Flow();
            if (flow != null)
                newFlow = flow.CreateRetryFlow(startOperation);
            else
                newFlow.Add(startOperation);

            var refreshTokenCall = CreateRefreshAccessTokenCall();
            if (refreshTokenCall != null)
                newFlow.Insert(0, refreshTokenCall);
            
            newFlow.RunAsSeries(); 
        }

        void TryHandleOnServerExpired(APIOperation startOperation, Flow flow, ServerResult result)
        {
            if (!IsExpiredAfterCall(result))
                return;
            
            Rerun(startOperation, flow);
        }

        protected abstract bool IsExpiredAfterCall(ServerResult result);
        protected abstract Operation CreateRefreshAccessTokenCall();
    }
}