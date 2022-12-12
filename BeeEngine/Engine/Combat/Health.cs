namespace BeeEngine.Combat
{
    public struct Health
    {
        public int Value { get; private set; }
        public int Max { get; private set; }
        public int Min { get; private set; }

        public bool IsAlive
        {
            get
            {
                return Value > 0;
            }
        }

        public Health(int value)
        {
            Value = value;
            Max = int.MaxValue;
            Min = int.MinValue;
        }

        public Health(int current, int min, int max)
        {
            if (max <= min || current > max || current < min)
                throw new ArgumentOutOfRangeException();
            Max = max;
            Min = min;
            Value = current;
        }

        public bool DealDamage(int damagePoints)
        {
            if (Value <= Min || Value + damagePoints <= int.MinValue + damagePoints)
                return false;
            Value -= damagePoints;
            return true;
        }

        public bool Heal(int healthPoints)
        {
            if (Value - healthPoints >= int.MaxValue - healthPoints)
                return false;
            if (Value + healthPoints > Max)
                Value = Max;
            else
                Value += healthPoints;
            return true;
        }

        public bool Kill()
        {
            if (Value <= 0) return false;
            Value = 0;
            return true;
        }

        public bool Resurrect(int newHealth)
        {
            if (Value > 0 || newHealth > Max || newHealth <= 0)
                return false;
            Value = newHealth;
            return true;
        }

        public bool SetMax(int max)
        {
            if (max < Min)
                return false;
            Max = max;
            if (Value > Max)
                Value = Max;
            return true;
        }

        public bool SetMin(int min)
        {
            if (min > Max)
                return false;
            Min = min;
            if (Value < Min)
                Value = Min;
            return true;
        }
    }
}