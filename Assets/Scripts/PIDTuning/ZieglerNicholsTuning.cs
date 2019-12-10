using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PIDTuning
{
    public enum ZieglerNicholsHeuristic
    {
        Classic,
        PessenIntegralRule,
        SomeOvershoot,
        NoOvershoot,
        PControl,
        PIControl,
        PDControl
    }

    public static class ZieglerNicholsTuning
    {
        public static PidParameters FromBangBangAnalysis(ZieglerNicholsHeuristic heuristic, OscillationAnalysisResult analysis, float relayConstantForce, float _timeStretchFactor)
        {
            // Values according to https://en.wikipedia.org/wiki/Ziegler%E2%80%93Nichols_method - version of 22.11.2019

            float tu = analysis.UltimatePeriod;
            float ku = (4f * relayConstantForce) / (Mathf.PI * (analysis.Amplitude * Mathf.Deg2Rad));

            switch (heuristic)
            {
                case ZieglerNicholsHeuristic.Classic:
                    return PidParameters.FromParallelForm(
                        0.6f * ku, 
                        1.2f * (ku / tu), 
                        (3f * ku * tu) / 40f);

                case ZieglerNicholsHeuristic.PessenIntegralRule:
                    return PidParameters.FromParallelForm(
                        (7f * ku) / 10f, 
                        1.75f * (ku / tu), 
                        (21f * ku * tu) / 200);

                case ZieglerNicholsHeuristic.SomeOvershoot:
                    return PidParameters.FromParallelForm(
                        ku / 3f,
                        0.666f * (ku / tu),
                        (ku * tu) / 9f);

                case ZieglerNicholsHeuristic.NoOvershoot:
                    return PidParameters.FromParallelForm(
                        ku / 5f,
                        ((2f / 5f) * ku) / tu,
                        (ku * tu) / 15f);

                case ZieglerNicholsHeuristic.PControl:
                    return PidParameters.FromParallelForm(0.5f * ku, 0f, 0f);

                case ZieglerNicholsHeuristic.PIControl:
                    return PidParameters.FromParallelForm(0.45f * ku, 0.54f * (ku / tu), 0f);

                case ZieglerNicholsHeuristic.PDControl:
                    return PidParameters.FromParallelForm(0.8f * ku, 0f, (ku * tu) / 10f);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}