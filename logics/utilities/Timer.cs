using System;

class Timer
{
    private DateTime start;

    public Timer()
    {
        Reset();
    }

    public void Reset() => start = DateTime.Now;
    public double Evaluate() => (DateTime.Now - start).TotalMilliseconds;
    public double EvaluateAndReset() 
    { 
        double res = (DateTime.Now - start).TotalMilliseconds; 
        start = DateTime.Now; 
        return res;
    }
}