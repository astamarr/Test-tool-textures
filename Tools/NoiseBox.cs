using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class NoiseMaterial
{
    public FastNoise noise = new FastNoise();
    public string name = "0";
    public float strength = 1.0f;
    public float pureStrength = 1.0f;
    public bool invert = false;
    public float rotation = 0;
    public Vector2 scale = new Vector2(1, 1);
    public float exageration = 1.0f;

    public Color upColor = Color.white;
    public Color downColor = Color.black;

    public float GetValue(float x, float y)
    {
        Vector2 finalPos = new Vector2(x, y);
        finalPos = Quaternion.AngleAxis(rotation, Vector3.forward) * finalPos;
        finalPos.Scale(scale);
        float r = noise.GetNoise(finalPos.x, finalPos.y) * strength;
        if (invert) r = 1 - r;

        r -= 0.5f;
        r *= exageration;
        r += 0.5f;

        r = Mathf.Clamp(r, 0, 1);
        return r * pureStrength;
    }

    public Color GetValueAsColor(float x, float y)
    {
        float value = GetValue(x, y);
        Color color = value * upColor + (1 - value) * downColor;
        return color;
    }
}

public class NoiseBox : ScriptableObject {



    [SerializeField]
    public List<NoiseMaterial> noises = new List<NoiseMaterial>();
    public bool colored = true;
    public bool useEquation = false;
    public string equation = "";
    Equation<Color> colorEquation = new Equation<Color>();
    Equation<float> floatEquation = new Equation<float>();
    Equation<float>.Calculator floatCalculator = new Equation<float>.Calculator();
    Equation<Color>.Calculator colorCalculator = new Equation<Color>.Calculator();

    public delegate T Adder<T>(T a, T b);
    public delegate T Resolver<T>(NoiseMaterial n, float x, float y);

    public NoiseBox()
    {
        floatCalculator.add = (a, b) => a + b;
        floatCalculator.sub = (a, b) => a - b;
        floatCalculator.mult = (a, b) => a * b;
        floatCalculator.div = (a, b) => a / b;
        colorCalculator.add = (a, b) => a + b;
        colorCalculator.sub = (a, b) => a - b;
        colorCalculator.mult = (a, b) => a * b;
        floatEquation.SetCalculator(floatCalculator);
        colorEquation.SetCalculator(colorCalculator);
    }

    public void Compute()
    {
        if(colored)
        {
            colorEquation.parameters.Clear();
            foreach (NoiseMaterial n in noises)
            {
                Debug.Log("adding " + n.name);
                colorEquation.parameters.Add(n.name, n.GetValueAsColor);
            }
            colorEquation.Compute(equation);
        }
        else
        {
            floatEquation.parameters.Clear();
            foreach (NoiseMaterial n in noises)
            {
                floatEquation.parameters.Add(n.name, n.GetValue);
            }
            floatEquation.Compute(equation);
        }
    }

    public T Resolve<T>(float x, float y, Equation<T> equation, T baseValue)
    {
        if(useEquation)
        {
            return equation.GetValue(x, y);
        }
        float total = 0;
        T res = baseValue;
        foreach (NoiseMaterial n in noises)
        {
            res = equation.GetCalculator().add(res, equation.parameters[n.name](x,y));
            total += n.strength;
        }
        return res;///total;
    }

    public float Resolve(float x, float y)
    {
        return Resolve<float>(x, y, floatEquation, 0);
    }

    public Color ResolveAsColor(float x, float y)
    {
        return Resolve<Color>(x, y, colorEquation, Color.black);
    }
}
