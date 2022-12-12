namespace ValueObject;

public abstract class ValueObject<TBaseType, TThisType>: IEquatable<ValueObject<TBaseType,TThisType>>
{
       internal ValueObject(TBaseType value)
       {
           Value = value;
           if (Validate())
           {
               _value = value;
               Value = default(TBaseType);
           }
           else
           {
               throw new ArgumentException($"Validation failed. Can't set value to {value}");
           }
       }

       public ValueObject()
       {
           
       }

       protected TBaseType? Value { get; private set; }
       private TBaseType _value;
       protected virtual bool Validate() 
       {
              return true;
       }

       public bool Set(TBaseType newValue)
       {
              Value = newValue;
              if (Validate())
              {
                     _value = Value;
                     Value = default(TBaseType);
                     return true;
              }

              return false;
       }

       public virtual TBaseType Get()
       {
              return _value;
       }

       public override bool Equals(object? obj)
       {
           if (obj.GetType() != this.GetType())
           {
               return false;
           }

           return Equals((ValueObject<TBaseType, TThisType>) obj);
       }

       public override int GetHashCode()
       {
           return HashCode.Combine(_value, Value);
       }

       public override string ToString()
       {
              return _value.ToString();
       }

       public bool Equals(ValueObject<TBaseType, TThisType>? other)
       {
           return EqualityComparer<TBaseType>.Default.Equals(_value, other._value);
       }
}