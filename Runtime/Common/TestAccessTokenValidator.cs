using System;
using PhEngine.Core.Operation;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/TestAccessTokenValidator", fileName = "TestAccessTokenValidator", order = 0)]
    public class TestAccessTokenValidator : AccessTokenValidator
    {
        [SerializeField] bool isExpiredBeforeCall;
        [SerializeField] bool isExpiredAfterCall;
        [SerializeField] float mockResolveTime = 1f;
        protected override bool IsExpiredAfterCall(ServerResult result)
        {
            return isExpiredAfterCall;
        }

        protected override bool IsExpiredBeforeCall()
        {
            return isExpiredBeforeCall;
        }

        protected override Operation CreateRefreshAccessTokenCall()
        {
            return Operation.Create()
                .DoFor(TimeSpan.FromSeconds(mockResolveTime), true)
                .SetOnStart(()=> Debug.Log("Faking Refresh AccessToken API..."))
                .SetOnFinish(()=>
                {
                    Debug.Log("Received new Fake AccessToken!");
                    isExpiredBeforeCall = false;
                    isExpiredAfterCall = false;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                });
        }
    }
}