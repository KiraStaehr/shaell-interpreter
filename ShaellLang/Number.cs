using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShaellLang
{
    public class Number : BaseValue, IKeyable
    {
        private dynamic _numberRepresentation;

        private static NumberTable _numberTable = NumberTable.getInstance();
        
        public Number(long value)
            : base("integernumber")
        {
            _numberRepresentation = value;
        }

        public Number(double value)
            : base("floatingnumber")
        {
            _numberRepresentation = value;
        }

        public override int GetHashCode()
        {
            return (UniquePrefix + KeyValue).GetHashCode();
        }

        public bool IsInteger => _numberRepresentation is long;

        public bool IsFloating => _numberRepresentation is double;

        public long ToInteger() => Convert.ToInt64(_numberRepresentation);

        public int ToInt32()
        {
            if(_numberRepresentation <= int.MaxValue) 
                return Convert.ToInt32(_numberRepresentation);
            else throw new Exception("Number is too big to convert to int");
        } 
        public double ToFloating() => Convert.ToDouble(_numberRepresentation);
        public string KeyValue => Convert.ToString(_numberRepresentation);
        public string UniquePrefix => "N";

        public override bool ToBool() => true;

        public override Number ToNumber() => this;

        public override SString ToSString()
        {
            if (IsFloating)
            {
                return new SString(ToFloating().ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                return new SString(ToInteger().ToString());
            }
        }

        public override ITable ToTable()
        {
            _numberTable.Number = this; //TODO: this is a hack, fix it
            return _numberTable;
        }

        public static Number operator +(Number a, Number b)
        {
            if (a.IsFloating || b.IsFloating)
            {
                return new Number(a.ToFloating() + b.ToFloating());
            }
            else
            {
                try
                {
                    return new Number(checked(a.ToInteger() + b.ToInteger()));
                }
                catch (OverflowException e)
                {
                    return new Number(a.ToFloating() + b.ToFloating());
                }
            }
        }
        
        public static Number operator -(Number a, Number b)
        {
            if (a.IsFloating || b.IsFloating)
            {
                return new Number(a.ToFloating() - b.ToFloating());
            }
            else
            {
                try
                {
                    return new Number(checked(a.ToInteger() - b.ToInteger()));
                }
                catch (OverflowException e)
                {
                    return new Number(a.ToFloating() - b.ToFloating());
                }
            }
        }

        public static Number operator *(Number a, Number b)
        {
            if (a.IsFloating || b.IsFloating)
            {
                return new Number(a.ToFloating() * b.ToFloating());
            }
            else
            {
                try
                {
                    return new Number(checked(a.ToInteger() * b.ToInteger()));
                }
                catch (OverflowException e)
                {
                    return new Number(a.ToFloating() * b.ToFloating());
                }
            }
        }

        public static Number operator /(Number a, Number b)
        {
            if (a.IsFloating || b.IsFloating || a.ToInteger() % b.ToInteger() != new Number(0).ToInteger())
            {
                return new Number(a.ToFloating() / b.ToFloating());
            }
            else
            {
                return new Number(a.ToInteger() / b.ToInteger());
            }
        }
        
        public static Number operator %(Number a, Number b)
        {
            if (a.IsFloating || b.IsFloating)
            {
                return new Number(a.ToFloating() % b.ToFloating());
            }
            return new Number(a.ToInteger() % b.ToInteger());
        }

        public static Number Power(Number a, Number b)
        {
            return new Number(Math.Pow(a.ToFloating(), b.ToFloating()));
        }
        
        //overide unary - and return the negative of the number
        public static Number operator -(Number a)
        {
            if (a.IsFloating)
            {
                return new Number(-a.ToFloating());
            }
            return new Number(-a.ToInteger());
        }
        
        //Implement less than operator and convert to floating and integer comparison correctly
        public static bool operator <(Number a, Number b)
        {
            if (a.IsFloating && b.IsFloating)
                return a.ToFloating() < b.ToFloating();
            if (a.IsFloating && b.IsInteger)
                return a.ToFloating() < b.ToInteger();
            if (a.IsInteger && b.IsFloating)
                return a.ToInteger() < b.ToFloating();
            return a.ToInteger() < b.ToInteger();
        }
        
        //Implement greater than operator and convert to floating and integer comparison correctly
        public static bool operator >(Number a, Number b)
        {
            if (a.IsFloating && b.IsFloating)
                return a.ToFloating() > b.ToFloating();
            if (a.IsFloating && b.IsInteger)
                return a.ToFloating() > b.ToInteger();
            if (a.IsInteger && b.IsFloating)
                return a.ToInteger() > b.ToFloating();
            return a.ToInteger() > b.ToInteger();
        }

        public static bool operator <=(Number a, Number b)
        {
            if (a.IsFloating && b.IsFloating)
                return a.ToFloating() <= b.ToFloating();
            if (a.IsFloating && b.IsInteger)
                return a.ToFloating() <= b.ToInteger();
            if (a.IsInteger && b.IsFloating)
                return a.ToInteger() <= b.ToFloating();
            return a.ToInteger() <= b.ToInteger();
        }
        
        public static bool operator >=(Number a, Number b)
        {
            if (a.IsFloating && b.IsFloating)
                return a.ToFloating() >= b.ToFloating();
            if (a.IsFloating && b.IsInteger)
                return a.ToFloating() >= b.ToInteger();
            if (a.IsInteger && b.IsFloating)
                return a.ToInteger() >= b.ToFloating();
            return a.ToInteger() >= b.ToInteger();
        }
        
        public override bool IsEqual(IValue other)
        {
            if (other is Number otherNumber)
            {
                return this == otherNumber;
            }

            return false;
        }
        
        public static bool operator ==(Number a, Number b)
        {
            if (a.IsFloating && b.IsFloating)
                return a.ToFloating() == b.ToFloating();
            if (a.IsFloating && b.IsInteger)
                return a.ToFloating() == b.ToInteger();
            if (a.IsInteger && b.IsFloating)
                return a.ToInteger() == b.ToFloating();
            return a.ToInteger() == b.ToInteger();
        }

        public static bool operator !=(Number a, Number b)
        {
            if (a.IsFloating && b.IsFloating)
                return a.ToFloating() != b.ToFloating();
            if (a.IsFloating && b.IsInteger)
                return a.ToFloating() != b.ToInteger();
            if (a.IsInteger && b.IsFloating)
                return a.ToInteger() != b.ToFloating();
            return a.ToInteger() != b.ToInteger();
        }

        public static bool operator !(Number a)
        {
            return !a.ToBool();
        }
    }
}