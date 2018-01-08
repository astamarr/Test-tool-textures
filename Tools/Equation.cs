using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equation<T> {
    public enum NodeType { Add, Substract, Multiply, Divide, Float, Value }

    public delegate T operation(T a, T b);

    public delegate T Resolver(float x, float y);

    public class Calculator
    {
        public operation add;
        public operation sub;
        public operation mult;
        public operation div;
    }

    public class Node
    {
        public NodeType type = NodeType.Value;
        public Resolver value;
        public float floatValue;
        public Node leftChild;
        public Node rightChild;
        public Calculator calculator;

        public T GetValue(float x, float y)
        {
            switch (type)
            {
                case NodeType.Add:
                    return calculator.add(leftChild.GetValue(x, y), rightChild.GetValue(x, y));
                case NodeType.Substract:
                    return calculator.sub(leftChild.GetValue(x, y), rightChild.GetValue(x, y));
                case NodeType.Multiply:
                    return calculator.mult(leftChild.GetValue(x, y), rightChild.GetValue(x, y));
                case NodeType.Divide:
                    return calculator.div(leftChild.GetValue(x, y), rightChild.GetValue(x, y));
                case NodeType.Value:
                    return value(x, y);
            }
            return default(T);
        }
    }

    Node root;
    Calculator calculator;
    public Dictionary<string, Resolver> parameters = new Dictionary<string, Resolver>();

    public Equation()
    {
    }

    public void SetCalculator(Calculator c)
    {
        calculator = c;
    }

    public Calculator GetCalculator()
    {
        return calculator;
    }

    public void Compute(string equation)
    {
        root = Parse(equation);
    }

    public T GetValue(float x, float y)
    {
        return root.GetValue(x, y);
    }

    int getFirst(string equation, string separator)
    {
        for(int i = 0; i < equation.Length; i++)
        {
            foreach(char c in separator)
            {
                if (equation[i] == c) return i;
            }
        }
        return -1;
    }

    Node Parse(string equation)
    {
        
        int next = getFirst(equation, "+-");
        if (next >= 0)
        {
            switch (equation[next])
            {
                case '+':
                    return CreateNode(NodeType.Add, equation, next);
                case '-':
                    return CreateNode(NodeType.Substract, equation, next);
            }
        }
        
        next = getFirst(equation, "*/");
        if(next >= 0)
        {
            switch (equation[next])
            {
                case '*':
                    return CreateNode(NodeType.Multiply, equation, next);
                case '/':
                    return CreateNode(NodeType.Divide, equation, next);
            }
        }
        float value = 0;
        if(float.TryParse(equation, out value))
        {
            Node n = new Node();
            n.calculator = calculator;
            n.type = NodeType.Float;
            n.floatValue = value;
            return n;
        }
        Resolver r;
        bool valueExist = parameters.TryGetValue(equation, out r);
        if (valueExist && r!=null)
        {
            Node n = new Node();
            n.calculator = calculator;
            n.type = NodeType.Value;
            n.value = r;
            return n;
        }
        return null;
    }

    Node CreateNode(NodeType type, string equation, int separator)
    {
        Node r = new Node();
        r.calculator = calculator;
        r.type = type;
        r.leftChild = Parse(equation.Substring(0, separator));
        r.rightChild = Parse(equation.Substring(separator + 1));
        return r;
    }

}
