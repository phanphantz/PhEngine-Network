﻿using System;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/WebRequestFormConfig", fileName = "WebRequestFormConfig", order = 0)]
    public class WebRequestFormConfig : ScriptableObject
    {
        [SerializeField] WebRequestForm form;
        public WebRequestForm Form => new WebRequestForm(form);

        [ContextMenu(nameof(Test))]
        public void Test()
        {
            var factory = FindObjectOfType<APICaller>();
            if (factory == null)
                throw new NullReferenceException("API Caller is missing.");

            if (form.parameterType == ParameterType.Path)
                new APIOperation(form, form.requestBodyTemplate).Run();
            else
                new APIOperation(form, new JSONObject(form.requestBodyTemplate)).Run();
        }
    }
}