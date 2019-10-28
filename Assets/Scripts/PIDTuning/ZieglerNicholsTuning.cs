using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PIDTuning
{
    public enum ZieglerNicholsVariant
    {
        Classic,
        PessenIntegralRule,
        SomeOvershoot,
        NoOvershoot
    }

    public static class ZieglerNicholsTuning
    {
        public static PidParameters FromBangBangAnalysis(ZieglerNicholsVariant variant, OscillationAnalysisResult analysis, float relayConstantForce, float simTimeStretchFactor)
        {
            float tu = analysis.UltimatePeriod * simTimeStretchFactor;
            float ku = (4f * relayConstantForce) / (Mathf.PI * (analysis.Amplitude * Mathf.Deg2Rad));

            switch (variant)
            {
                case ZieglerNicholsVariant.Classic:
                    return PidParameters.FromParallelForm(
                        0.6f * ku, 
                        1.2f * (ku / tu), 
                        (3f * ku * tu) / 40f);

                case ZieglerNicholsVariant.PessenIntegralRule:
                    return PidParameters.FromParallelForm(
                        (7f * ku) / 10f, 
                        1.75f * (ku / tu), 
                        (21f * ku * tu) / 200);

                case ZieglerNicholsVariant.SomeOvershoot:
                    return PidParameters.FromParallelForm(
                        ku / 3f,
                        0.666f * (ku / tu),
                        (ku * tu) / 9f);

                case ZieglerNicholsVariant.NoOvershoot:
                    return PidParameters.FromParallelForm(
                        ku / 5f,
                        ((2f / 5f) * ku) / tu,
                        (ku * tu) / 15f);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}