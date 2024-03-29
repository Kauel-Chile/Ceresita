﻿// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEditor;
using UnityEngine;
using System.Collections;

namespace TMPro.EditorUtilities
{

    //[InitializeOnLoad]
    class TMP_ResourcesLoader
    {

        /// <summary>
        /// Function to pre-load the TMP Resources
        /// </summary>
        public static void LoadTextMeshProResources()
        {
            TMP_Settings.LoadDefaultSettings();
            TMP_StyleSheet.LoadDefaultStyleSheet();
        }


        static TMP_ResourcesLoader()
        {
            Debug.Log("Loading TMP Resources...");

            //TMPro.TMP_Settings.LoadDefaultSettings();
            //TMPro.TMP_StyleSheet.LoadDefaultStyleSheet();
        }



        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //static void OnBeforeSceneLoaded()
        //{
            //Debug.Log("Before scene is loaded.");

            //    //TMPro.TMP_Settings.LoadDefaultSettings();
            //    //TMPro.TMP_StyleSheet.LoadDefaultStyleSheet();

            //    //ShaderVariantCollection collection = new ShaderVariantCollection();
            //    //Shader s0 = Shader.Find("TextMeshPro/Mobile/Distance Field");
            //    //ShaderVariantCollection.ShaderVariant tmp_Variant = new ShaderVariantCollection.ShaderVariant(s0, UnityEngine.Rendering.PassType.Normal, string.Empty);

            //    //collection.Add(tmp_Variant);
            //    //collection.WarmUp();
        //}

    }
}
